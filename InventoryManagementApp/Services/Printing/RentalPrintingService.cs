using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.Services.Printing
{
    /// <summary>
    /// Service for generating printable documents for rentals.
    /// </summary>
    public class RentalPrintingService
    {
        private readonly string _companyName;
        private readonly string _companyAddress;
        private readonly string _companyPhone;
        private static readonly string[] ItemImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        public RentalPrintingService(
            string companyName = "Equipment Rentals",
            string companyAddress = "",
            string companyPhone = "")
        {
            _companyName = companyName;
            _companyAddress = companyAddress;
            _companyPhone = companyPhone;
        }

        /// <summary>
        /// Generates a picking slip document for a rental.
        /// </summary>
        public FlowDocument GeneratePickingSlip(Rental rental)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = 12
            };

            doc.Blocks.Add(new Paragraph(new Bold(new Run(_companyName)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new Paragraph(new Bold(new Run("PICKING SLIP")))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var infoTable = new Table();
            infoTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            infoTable.Columns.Add(new TableColumn());
            var infoGroup = new TableRowGroup();
            infoTable.RowGroups.Add(infoGroup);

            AddTableRow(infoGroup, "Date:", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            AddTableRow(infoGroup, "Rental ID:", rental.RentalID.ToString());
            AddTableRow(infoGroup, "", "");

            AddTableRow(infoGroup, "Customer:", rental.CustomerName, true);
            if (!string.IsNullOrWhiteSpace(rental.CustomerContact))
                AddTableRow(infoGroup, "Contact:", rental.CustomerContact);
            if (!string.IsNullOrWhiteSpace(rental.CustomerPhone))
                AddTableRow(infoGroup, "Phone:", rental.CustomerPhone);
            if (!string.IsNullOrWhiteSpace(rental.CustomerEmail))
                AddTableRow(infoGroup, "Email:", rental.CustomerEmail);
            AddTableRow(infoGroup, "", "");

            AddTableRow(infoGroup, "Item Number:", rental.ItemNumber, true);
            if (!string.IsNullOrWhiteSpace(rental.ItemLocation))
                AddTableRow(infoGroup, "Location:", rental.ItemLocation);
            AddTableRow(infoGroup, "Rental Date:", rental.RentalDate.ToString("yyyy-MM-dd"));
            AddTableRow(infoGroup, "Due Date:", rental.DueDate.ToString("yyyy-MM-dd"));

            var itemImage = CreateItemPhotoBlock(rental);
            if (itemImage != null)
                doc.Blocks.Add(itemImage);

            doc.Blocks.Add(infoTable);

            doc.Blocks.Add(new Paragraph(new Run(""))
            {
                Margin = new Thickness(0, 20, 0, 0)
            });

            doc.Blocks.Add(new Paragraph(new Run("_________________________________"))
            {
                Margin = new Thickness(0, 20, 0, 5)
            });
            doc.Blocks.Add(new Paragraph(new Run("Picked By / Date"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 20)
            });

            doc.Blocks.Add(new Paragraph(new Run("_________________________________"))
            {
                Margin = new Thickness(0, 0, 0, 5)
            });
            doc.Blocks.Add(new Paragraph(new Run("Customer Signature / Date"))
            {
                FontSize = 10
            });

            return doc;
        }

        private static BlockUIContainer? CreateItemPhotoBlock(Rental rental)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                return null;

            if (!TryLoadItemImage(rental, out var bitmap))
                return null;

            var image = new Image
            {
                Source = bitmap,
                Width = 220,
                Height = 165,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8)
            };

            return new BlockUIContainer(image)
            {
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static bool TryLoadItemImage(Rental rental, out BitmapImage image)
        {
            image = null!;

            foreach (var path in GetItemImageCandidates(rental))
            {
                try
                {
                    var absolutePath = ResolveImagePath(path);
                    if (string.IsNullOrWhiteSpace(absolutePath) || !File.Exists(absolutePath))
                        continue;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 440;
                    bitmap.UriSource = new Uri(absolutePath, UriKind.Absolute);
                    bitmap.EndInit();
                    bitmap.Freeze();

                    image = bitmap;
                    return true;
                }
                catch
                {
                    // Keep printed picking slips available even if an item photo is missing or corrupt.
                }
            }

            return false;
        }

        private static IEnumerable<string> GetItemImageCandidates(Rental rental)
        {
            if (!string.IsNullOrWhiteSpace(rental.ImagePath))
                yield return rental.ImagePath;

            var itemNumber = rental.ItemNumber?.Trim();
            if (string.IsNullOrWhiteSpace(itemNumber))
                yield break;

            foreach (var extension in ItemImageExtensions)
                yield return Path.Combine("Assets", "ItemImages", itemNumber + extension);
        }

        private static string? ResolveImagePath(string path)
        {
            if (Path.IsPathFullyQualified(path))
                return path;

            return PathHelper.GetAbsolutePath(path, false);
        }

        /// <summary>
        /// Generates an invoice document for a rental.
        /// </summary>
        public FlowDocument GenerateInvoice(Rental rental, decimal dailyRate = 0, decimal lateFee = 0)
        {
            if (rental == null)
                throw new ArgumentNullException(nameof(rental));

            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = 12
            };

            doc.Blocks.Add(new Paragraph(new Bold(new Run(_companyName)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });

            if (!string.IsNullOrWhiteSpace(_companyAddress))
            {
                doc.Blocks.Add(new Paragraph(new Run(_companyAddress))
                {
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }

            if (!string.IsNullOrWhiteSpace(_companyPhone))
            {
                doc.Blocks.Add(new Paragraph(new Run(_companyPhone))
                {
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                });
            }

            doc.Blocks.Add(new Paragraph(new Bold(new Run("INVOICE")))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            var headerTable = new Table();
            headerTable.Columns.Add(new TableColumn());
            headerTable.Columns.Add(new TableColumn());
            var headerGroup = new TableRowGroup();
            headerTable.RowGroups.Add(headerGroup);

            var row1 = new TableRow();
            var cell1 = new TableCell(new Paragraph(new Run($"Invoice Date: {DateTime.Now:yyyy-MM-dd}")));
            var cell2 = new TableCell(new Paragraph(new Run($"Rental ID: {rental.RentalID}"))
            {
                TextAlignment = TextAlignment.Right
            });
            row1.Cells.Add(cell1);
            row1.Cells.Add(cell2);
            headerGroup.Rows.Add(row1);

            doc.Blocks.Add(headerTable);
            doc.Blocks.Add(new Paragraph(new Run("")) { Margin = new Thickness(0, 10, 0, 10) });

            var infoTable = new Table();
            infoTable.Columns.Add(new TableColumn { Width = new GridLength(150) });
            infoTable.Columns.Add(new TableColumn());
            var infoGroup = new TableRowGroup();
            infoTable.RowGroups.Add(infoGroup);

            AddTableRow(infoGroup, "Bill To:", rental.CustomerName, true);
            if (!string.IsNullOrWhiteSpace(rental.CustomerContact))
                AddTableRow(infoGroup, "Contact:", rental.CustomerContact);
            if (!string.IsNullOrWhiteSpace(rental.CustomerAddress))
                AddTableRow(infoGroup, "Address:", rental.CustomerAddress);
            if (!string.IsNullOrWhiteSpace(rental.CustomerEmail))
                AddTableRow(infoGroup, "Email:", rental.CustomerEmail);
            if (!string.IsNullOrWhiteSpace(rental.CustomerPhone))
                AddTableRow(infoGroup, "Phone:", rental.CustomerPhone);

            doc.Blocks.Add(infoTable);
            doc.Blocks.Add(new Paragraph(new Run("")) { Margin = new Thickness(0, 15, 0, 15) });

            var itemsTable = new Table
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            itemsTable.Columns.Add(new TableColumn());
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
            itemsTable.Columns.Add(new TableColumn { Width = new GridLength(80) });

            var itemsHeaderGroup = new TableRowGroup();
            itemsTable.RowGroups.Add(itemsHeaderGroup);
            var headerRow = new TableRow { Background = System.Windows.Media.Brushes.LightGray };
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Description")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Rental Period")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Rate")))));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Amount")))));
            itemsHeaderGroup.Rows.Add(headerRow);

            var itemsBodyGroup = new TableRowGroup();
            itemsTable.RowGroups.Add(itemsBodyGroup);

            var rentalDays = (rental.ReturnDate ?? DateTime.Today) - rental.RentalDate.Date;
            var days = Math.Max(1, rentalDays.Days + 1);
            var rentalAmount = dailyRate * days;

            var itemRow = new TableRow();
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"Item: {rental.ItemNumber}"))));
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"{days} day(s)"))));
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"${dailyRate:F2}/day"))));
            itemRow.Cells.Add(new TableCell(new Paragraph(new Run($"${rentalAmount:F2}"))
            {
                TextAlignment = TextAlignment.Right
            }));
            itemsBodyGroup.Rows.Add(itemRow);

            if (lateFee > 0)
            {
                var lateRow = new TableRow();
                lateRow.Cells.Add(new TableCell(new Paragraph(new Run("Late Fee"))));
                lateRow.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                lateRow.Cells.Add(new TableCell(new Paragraph(new Run(""))));
                lateRow.Cells.Add(new TableCell(new Paragraph(new Run($"${lateFee:F2}"))
                {
                    TextAlignment = TextAlignment.Right
                }));
                itemsBodyGroup.Rows.Add(lateRow);
            }

            var totalAmount = rentalAmount + lateFee;
            var totalRow = new TableRow { Background = System.Windows.Media.Brushes.LightGray };
            totalRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run("Total")))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(""))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(""))));
            totalRow.Cells.Add(new TableCell(new Paragraph(new Bold(new Run($"${totalAmount:F2}")))
            {
                TextAlignment = TextAlignment.Right
            }));
            itemsBodyGroup.Rows.Add(totalRow);

            doc.Blocks.Add(itemsTable);

            doc.Blocks.Add(new Paragraph(new Run(""))
            {
                Margin = new Thickness(0, 30, 0, 0)
            });

            doc.Blocks.Add(new Paragraph(new Run("Thank you for your business!"))
            {
                FontSize = 10,
                TextAlignment = TextAlignment.Center,
                FontStyle = FontStyles.Italic
            });

            return doc;
        }

        private void AddTableRow(TableRowGroup group, string label, string value, bool boldValue = false)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label))
            {
                FontWeight = FontWeights.Bold
            }));

            var valueRun = new Run(value ?? string.Empty);
            var valuePara = new Paragraph(boldValue ? new Bold(valueRun) : valueRun);
            row.Cells.Add(new TableCell(valuePara));
            
            group.Rows.Add(row);
        }
    }
}
