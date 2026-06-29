using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.IO;

namespace InventoryManagementApp.Services.ImportExport
{
    /// <summary>
    /// Exports customers to CSV format using the existing CsvHelperUtil.
    /// </summary>
    public class CustomerCsvExporter : IDataExporter<Customer>
    {
        public string FileExtension => ".csv";
        public string FileFilter => "CSV Files|*.csv";
        public string FormatName => "CSV";

        public async Task ExportAsync(string filePath, IEnumerable<Customer> data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            // Convert Customer to CustomerModel for CSV export
            var customers = data.Select(c => new CustomerModel
            {
                CustomerID = c.CustomerID,
                Company = c.Company,
                Contact = c.Contact,
                Email = c.Email,
                Phone = c.Phone,
                Mobile = c.Mobile,
                Address = c.Address
            }).ToList();

            cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                CsvHelperUtil.ExportCustomersToCsv(filePath, customers);
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}