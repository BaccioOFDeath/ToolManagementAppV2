using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Utilities.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryManagementApp.Services.MobileCapture
{
    public sealed class MobileCaptureService : IDisposable, IAsyncDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MobileCaptureService> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private WebApplication? _app;
        private string _token = string.Empty;
        private DateTime _expiresAt;
        private int _port;

        public MobileCaptureService(IServiceProvider services, IConfiguration configuration, ILogger<MobileCaptureService> logger)
        {
            _services = services;
            _configuration = configuration;
            _logger = logger;
        }

        public bool IsRunning => _app != null;

        public async Task<MobileCaptureSession> StartSessionAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _token = CreateToken();
                _expiresAt = DateTime.Now.AddHours(4);
                _port = GetPort();

                if (_app == null)
                {
                    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                    {
                        ApplicationName = typeof(MobileCaptureService).Assembly.GetName().Name,
                        ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
                    });
                    builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
                    builder.Services.Configure<FormOptions>(options =>
                    {
                        options.MultipartBodyLengthLimit = 32L * 1024L * 1024L;
                        options.ValueLengthLimit = 1024 * 1024;
                    });

                    var app = builder.Build();
                    MapEndpoints(app);
                    await app.StartAsync(cancellationToken).ConfigureAwait(false);
                    _app = app;
                    _logger.LogInformation("Mobile capture server started on port {Port}", _port);
                }

                return new MobileCaptureSession(BuildPublicUrl(_token), _token, _expiresAt);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_app == null)
                    return;

                await _app.StopAsync(cancellationToken).ConfigureAwait(false);
                await _app.DisposeAsync().ConfigureAwait(false);
                _app = null;
                _token = string.Empty;
                _logger.LogInformation("Mobile capture server stopped");
            }
            finally
            {
                _gate.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
            _gate.Dispose();
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            _gate.Dispose();
        }

        private void MapEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => Results.Redirect($"/mobile-capture?token={Uri.EscapeDataString(_token)}"));
            app.MapGet("/mobile-capture", (HttpRequest request) =>
            {
                if (!IsValidToken(request.Query["token"]))
                    return Results.Content(CreateExpiredHtml(), "text/html", Encoding.UTF8);

                return Results.Content(CreateCaptureHtml(_token), "text/html", Encoding.UTF8);
            });
            app.MapPost("/mobile-capture/items", SubmitItemAsync);
            app.MapPost("/mobile-capture/rental-photos", SubmitRentalPhotoAsync);
        }

        private async Task<IResult> SubmitItemAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var form = await request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
            if (!IsValidToken(form["token"]))
                return Results.Content(CreateExpiredHtml(), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status403Forbidden);

            var itemService = _services.GetRequiredService<IItemService>();
            var item = new ItemModel
            {
                ItemNumber = form["itemNumber"].ToString().Trim(),
                Name = form["name"].ToString().Trim(),
                Brand = form["brand"].ToString().Trim(),
                PartNumber = form["partNumber"].ToString().Trim(),
                Supplier = form["supplier"].ToString().Trim(),
                Location = form["location"].ToString().Trim(),
                Notes = form["notes"].ToString().Trim(),
                Keywords = form["keywords"].ToString().Trim(),
                QuantityOnHand = ParseInt(form["quantity"], 1),
                IsRentalItem = string.Equals(form["isRentalItem"], "on", StringComparison.OrdinalIgnoreCase),
                IsPowered = string.Equals(form["isPowered"], "on", StringComparison.OrdinalIgnoreCase),
                Price = ParseDecimal(form["price"])
            };

            if (string.IsNullOrWhiteSpace(item.Name))
                return Results.Content(CreateResultHtml("Missing details", "Name is required.", _token), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status400BadRequest);

            var image = form.Files.GetFile("photo");
            if (image is { Length: > 0 })
                item.ImagePath = await SaveUploadAsync(image, "ItemImages", item.ItemNumber, cancellationToken).ConfigureAwait(false);

            await itemService.AddItemAsync(item, cancellationToken).ConfigureAwait(false);
            return Results.Content(CreateResultHtml("Item added", $"{Escape(item.ItemNumber)} {Escape(item.Name)} was saved.", _token), "text/html", Encoding.UTF8);
        }

        private async Task<IResult> SubmitRentalPhotoAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            var form = await request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
            if (!IsValidToken(form["token"]))
                return Results.Content(CreateExpiredHtml(), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status403Forbidden);

            var itemNumber = form["rentalItemNumber"].ToString().Trim();
            if (string.IsNullOrWhiteSpace(itemNumber))
                return Results.Content(CreateResultHtml("Missing item", "Item number is required for rental photos.", _token), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status400BadRequest);

            var photo = form.Files.GetFile("rentalPhoto");
            if (photo is not { Length: > 0 })
                return Results.Content(CreateResultHtml("Missing photo", "Choose a before or after photo before submitting.", _token), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status400BadRequest);

            var item = await FindItemByNumberAsync(itemNumber, cancellationToken).ConfigureAwait(false);
            if (item == null)
                return Results.Content(CreateResultHtml("Item not found", $"No item exists with number {Escape(itemNumber)}.", _token), "text/html", Encoding.UTF8, statusCode: StatusCodes.Status404NotFound);

            var stage = NormalizeStage(form["photoStage"]);
            var rentalId = ParseNullableInt(form["rentalId"]);
            var filePath = await SaveUploadAsync(photo, "RentalPhotos", $"{item.ItemNumber}-{stage}", cancellationToken).ConfigureAwait(false);
            await InsertRentalPhotoAsync(item.ItemID, rentalId, stage, filePath, form["rentalPhotoNotes"].ToString(), cancellationToken).ConfigureAwait(false);

            return Results.Content(CreateResultHtml("Rental photo saved", $"{Escape(stage)} photo saved for {Escape(item.ItemNumber)}.", _token), "text/html", Encoding.UTF8);
        }

        private async Task<ItemModel?> FindItemByNumberAsync(string itemNumber, CancellationToken cancellationToken)
        {
            var itemService = _services.GetRequiredService<IItemService>();
            await foreach (var item in itemService.SearchItemsAsync(itemNumber, new ItemPage(1, 10), SortField.ItemNumber, SortDirection.Ascending, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                if (string.Equals(item.ItemNumber, itemNumber, StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        private async Task InsertRentalPhotoAsync(int itemId, int? rentalId, string stage, string filePath, string notes, CancellationToken cancellationToken)
        {
            var db = _services.GetRequiredService<DatabaseService>();
            var context = _services.GetService<IUserContext>();
            const string sql = @"INSERT INTO RentalPhotos (RentalID, ItemID, PhotoStage, FilePath, Notes, CreatedAt, CreatedBy)
                                 VALUES (@RentalID, @ItemID, @PhotoStage, @FilePath, @Notes, @CreatedAt, @CreatedBy)";
            using var conn = db.CreateConnection();
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddRange(new[]
            {
                new SqliteParameter("@RentalID", rentalId.HasValue ? rentalId.Value : DBNull.Value),
                new SqliteParameter("@ItemID", itemId),
                new SqliteParameter("@PhotoStage", stage),
                new SqliteParameter("@FilePath", filePath),
                new SqliteParameter("@Notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes.Trim()),
                new SqliteParameter("@CreatedAt", DateTime.Now),
                new SqliteParameter("@CreatedBy", context?.UserName ?? string.Empty)
            });
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> SaveUploadAsync(IFormFile file, string assetFolder, string nameSeed, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !IsSupportedImage(extension))
                extension = ".jpg";

            var safeSeed = MakeSafeFileName(string.IsNullOrWhiteSpace(nameSeed) ? "capture" : nameSeed);
            var fileName = $"{safeSeed}-{DateTime.Now:yyyyMMdd-HHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}{extension.ToLowerInvariant()}";
            var relativePath = Path.Combine(AppAssetHelper.AssetsDirectoryName, assetFolder, fileName).Replace('\\', '/');
            var targetDir = AppAssetHelper.EnsureAssetFolder(assetFolder);
            var targetPath = Path.Combine(targetDir, fileName);

            await using var stream = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            return relativePath;
        }

        private bool IsValidToken(string? value)
            => !string.IsNullOrWhiteSpace(value)
               && !string.IsNullOrWhiteSpace(_token)
               && DateTime.Now <= _expiresAt
               && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(_token));

        private int GetPort()
            => int.TryParse(_configuration["MobileCapture:Port"], out var configured) && configured is > 0 and < 65536
                ? configured
                : 5075;

        private string BuildPublicUrl(string token)
            => $"http://{GetLanAddress()}:{_port}/mobile-capture?token={Uri.EscapeDataString(token)}";

        private static string GetLanAddress()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ip = ni.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddressIsAutoPrivate(a.Address.ToString()));
                if (ip != null)
                    return ip.Address.ToString();
            }

            return "127.0.0.1";
        }

        private static bool IPAddressIsAutoPrivate(string address)
            => address.StartsWith("169.254.", StringComparison.Ordinal);

        private static string CreateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static int ParseInt(string? value, int fallback)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0 ? parsed : fallback;

        private static int? ParseNullableInt(string? value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0 ? parsed : null;

        private static decimal ParseDecimal(string? value)
            => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0 ? parsed : 0m;

        private static string NormalizeStage(string? value)
            => string.Equals(value, "Before", StringComparison.OrdinalIgnoreCase)
                ? "Before"
                : string.Equals(value, "After", StringComparison.OrdinalIgnoreCase)
                    ? "After"
                    : "General";

        private static bool IsSupportedImage(string extension)
            => extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
               || extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase);

        private static string MakeSafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = value.Select(ch => invalid.Contains(ch) || char.IsWhiteSpace(ch) ? '-' : ch).ToArray();
            var result = new string(chars).Trim('-');
            return string.IsNullOrWhiteSpace(result) ? "capture" : result;
        }

        private static string Escape(string value)
            => System.Net.WebUtility.HtmlEncode(value);

        private static string CreateExpiredHtml()
            => """
               <!doctype html><html><head><meta name="viewport" content="width=device-width, initial-scale=1"><title>Mobile Capture</title>
               <style>body{font-family:Segoe UI,Arial,sans-serif;margin:0;padding:24px;background:#111827;color:#f8fafc}main{max-width:520px;margin:auto;background:#1f2937;padding:20px;border:1px solid #374151}h1{font-size:24px}</style></head>
               <body><main><h1>Session expired</h1><p>Open Mobile Capture again from the desktop app and scan the new QR code.</p></main></body></html>
               """;

        private static string CreateResultHtml(string title, string message, string token)
            => $$"""
               <!doctype html><html><head><meta name="viewport" content="width=device-width, initial-scale=1"><title>{{Escape(title)}}</title>
               <style>{{MobileCss}}</style></head><body><main><h1>{{Escape(title)}}</h1><p>{{message}}</p><a class="button" href="/mobile-capture?token={{Uri.EscapeDataString(token)}}">Add another</a></main></body></html>
               """;

        private static string CreateCaptureHtml(string token)
            => $$"""
               <!doctype html>
               <html>
               <head>
                 <meta name="viewport" content="width=device-width, initial-scale=1">
                 <title>Mobile Capture</title>
                 <style>{{MobileCss}}</style>
               </head>
               <body>
                 <main>
                   <h1>Mobile Capture</h1>
                   <section>
                     <h2>New item</h2>
                     <form method="post" action="/mobile-capture/items" enctype="multipart/form-data">
                       <input type="hidden" name="token" value="{{Escape(token)}}">
                       <label>Photo<input name="photo" type="file" accept="image/*" capture="environment"></label>
                       <label>Item number<input name="itemNumber" autocomplete="off" placeholder="Leave blank for next number"></label>
                       <label>Name<input name="name" required autocomplete="off"></label>
                       <label>Brand<input name="brand" autocomplete="off"></label>
                       <label>Part number<input name="partNumber" autocomplete="off"></label>
                       <label>Supplier<input name="supplier" autocomplete="off"></label>
                       <label>Location<input name="location" autocomplete="off"></label>
                       <label>Quantity<input name="quantity" type="number" min="0" value="1"></label>
                       <label>Price<input name="price" inputmode="decimal" placeholder="0.00"></label>
                       <label>Keywords<input name="keywords" autocomplete="off"></label>
                       <label>Notes<textarea name="notes" rows="3"></textarea></label>
                       <div class="checks"><label><input name="isRentalItem" type="checkbox"> Rental item</label><label><input name="isPowered" type="checkbox"> Powered</label></div>
                       <button type="submit">Save item</button>
                     </form>
                   </section>
                   <section>
                     <h2>Rental photo</h2>
                     <form method="post" action="/mobile-capture/rental-photos" enctype="multipart/form-data">
                       <input type="hidden" name="token" value="{{Escape(token)}}">
                       <label>Item number<input name="rentalItemNumber" required autocomplete="off"></label>
                       <label>Rental ID, optional<input name="rentalId" type="number" min="1"></label>
                       <label>Stage<select name="photoStage"><option>Before</option><option>After</option><option>General</option></select></label>
                       <label>Photo<input name="rentalPhoto" required type="file" accept="image/*" capture="environment"></label>
                       <label>Notes<textarea name="rentalPhotoNotes" rows="3"></textarea></label>
                       <button type="submit">Save rental photo</button>
                     </form>
                   </section>
                 </main>
               </body>
               </html>
               """;

        private const string MobileCss = """
            :root{color-scheme:light dark;--bg:#f4f6f8;--panel:#ffffff;--text:#111827;--muted:#5b6472;--line:#cbd5e1;--accent:#2563eb;--accentText:#fff}
            @media (prefers-color-scheme: dark){:root{--bg:#10141d;--panel:#1d2430;--text:#f8fafc;--muted:#cbd5e1;--line:#334155;--accent:#60a5fa;--accentText:#0f172a}}
            *{box-sizing:border-box}body{margin:0;background:var(--bg);color:var(--text);font-family:Segoe UI,Arial,sans-serif}main{max-width:680px;margin:0 auto;padding:16px}h1{font-size:28px;margin:8px 0 16px}h2{font-size:18px;margin:0 0 12px}section{background:var(--panel);border:1px solid var(--line);border-radius:8px;padding:16px;margin:0 0 14px;box-shadow:0 1px 2px rgba(15,23,42,.08)}label{display:block;font-weight:600;font-size:13px;color:var(--muted);margin:0 0 10px}input,textarea,select{display:block;width:100%;margin-top:4px;padding:11px 10px;border:1px solid var(--line);border-radius:6px;background:transparent;color:var(--text);font:inherit}textarea{resize:vertical}.checks{display:grid;grid-template-columns:1fr 1fr;gap:8px;margin:2px 0 12px}.checks label{display:flex;gap:8px;align-items:center;margin:0}.checks input{width:auto;margin:0}button,.button{display:inline-block;width:100%;border:0;border-radius:6px;background:var(--accent);color:var(--accentText);font-weight:700;padding:12px 14px;text-align:center;text-decoration:none;font:inherit}p{line-height:1.45;color:var(--muted)}
            """;
    }
}
