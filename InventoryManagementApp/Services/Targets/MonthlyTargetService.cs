using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.Services.Targets
{
    public class MonthlyTargetService : IMonthlyTargetService
    {
        readonly DatabaseService _databaseService;
        readonly ILogger<MonthlyTargetService> _logger;

        const string SelectSql = @"SELECT TargetId, FinancialYearStart, MonthOffset, Year, Month, TargetAmount
                                   FROM MonthlyTargets
                                   WHERE FinancialYearStart = @FinancialYearStart
                                   ORDER BY MonthOffset";

        const string InsertSql = @"INSERT INTO MonthlyTargets (FinancialYearStart, MonthOffset, Year, Month, TargetAmount)
                                   VALUES (@FinancialYearStart, @MonthOffset, @Year, @Month, @TargetAmount)";

        const string DeleteSql = @"DELETE FROM MonthlyTargets WHERE FinancialYearStart = @FinancialYearStart";

        public MonthlyTargetService(DatabaseService databaseService, ILogger<MonthlyTargetService>? logger = null)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _logger = logger ?? NullLogger<MonthlyTargetService>.Instance;
        }

        public async Task<IReadOnlyList<MonthlyTarget>> GetTargetsAsync(int financialYearStart, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _databaseService.CreateConnection();
                var parameter = new SqliteParameter("@FinancialYearStart", DbType.Int32) { Value = financialYearStart };
                var result = new List<MonthlyTarget>();
                using var command = new SqliteCommand(SelectSql, connection);
                command.Parameters.Add(parameter);
                using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var target = new MonthlyTarget
                    {
                        TargetId = reader.GetInt32(0),
                        FinancialYearStart = reader.GetInt32(1),
                        MonthOffset = reader.GetInt32(2),
                        Year = reader.GetInt32(3),
                        Month = reader.GetInt32(4),
                        TargetAmount = reader.IsDBNull(5) ? 0m : Convert.ToDecimal(reader.GetValue(5))
                    };
                    result.Add(target);
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load monthly targets for financial year {FinancialYearStart}", financialYearStart);
                throw new InvalidOperationException($"Failed to load monthly targets for financial year {financialYearStart}.", ex);
            }
        }

        public async Task SaveTargetsAsync(int financialYearStart, IEnumerable<MonthlyTarget> targets, CancellationToken cancellationToken = default)
        {
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            try
            {
                using var connection = _databaseService.CreateConnection();
                using var transaction = connection.BeginTransaction();

                using (var deleteCommand = new SqliteCommand(DeleteSql, connection, transaction))
                {
                    deleteCommand.Parameters.Add(new SqliteParameter("@FinancialYearStart", DbType.Int32) { Value = financialYearStart });
                    await deleteCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                foreach (var target in targets)
                {
                    using var command = new SqliteCommand(InsertSql, connection, transaction);
                    command.Parameters.AddRange(new[]
                    {
                        new SqliteParameter("@FinancialYearStart", DbType.Int32) { Value = target.FinancialYearStart },
                        new SqliteParameter("@MonthOffset", DbType.Int32) { Value = target.MonthOffset },
                        new SqliteParameter("@Year", DbType.Int32) { Value = target.Year },
                        new SqliteParameter("@Month", DbType.Int32) { Value = target.Month },
                        new SqliteParameter("@TargetAmount", DbType.Decimal) { Value = target.TargetAmount }
                    });
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                transaction.Commit();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save monthly targets for financial year {FinancialYearStart}", financialYearStart);
                throw new InvalidOperationException($"Failed to save monthly targets for financial year {financialYearStart}.", ex);
            }
        }
    }
}
