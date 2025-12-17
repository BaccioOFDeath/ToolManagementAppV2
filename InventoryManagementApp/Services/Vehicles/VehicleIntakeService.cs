using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.DTOs;
using InventoryManagementApp.Services.Core;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Services.Vehicles
{
    /// <summary>
    /// Provides intake-to-dismantle workflow management for SDAutoOS vehicle processing.
    /// Tracks VIN intake, compliance holds, and part-level dismantling tasks.
    /// </summary>
    public class VehicleIntakeService
    {
        private readonly DatabaseService _databaseService;
        private readonly IUserContext _userContext;

        public VehicleIntakeService(DatabaseService databaseService, IUserContext userContext)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<int> CreateVehicleAsync(VehicleIntake vehicle)
        {
            if (vehicle is null)
                throw new ArgumentNullException(nameof(vehicle));
            if (string.IsNullOrWhiteSpace(vehicle.Vin))
                throw new ArgumentException("VIN is required for vehicle intake", nameof(vehicle));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    INSERT INTO Vehicles
                    (Vin, StockNumber, Year, Make, Model, Trim, IntakeDate, Status, Location, Mileage, FuelType,
                     DriveTrain, Disposition, Notes, ComplianceHoldReason, CreatedByUserID, CreatedAt, UpdatedAt)
                    VALUES
                    (@Vin, @StockNumber, @Year, @Make, @Model, @Trim, @IntakeDate, @Status, @Location, @Mileage,
                     @FuelType, @DriveTrain, @Disposition, @Notes, @ComplianceHoldReason, @CreatedByUserID, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, conn);
                var now = DateTime.Now;
                var createdBy = _userContext.CurrentUser?.UserID ?? 0;
                cmd.Parameters.AddWithValue("@Vin", vehicle.Vin.Trim());
                cmd.Parameters.AddWithValue("@StockNumber", (object?)vehicle.StockNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Year", vehicle.Year.HasValue ? vehicle.Year.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Make", vehicle.Make);
                cmd.Parameters.AddWithValue("@Model", vehicle.Model);
                cmd.Parameters.AddWithValue("@Trim", vehicle.Trim);
                cmd.Parameters.AddWithValue("@IntakeDate", vehicle.IntakeDate);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(vehicle.Status) ? "Received" : vehicle.Status);
                cmd.Parameters.AddWithValue("@Location", vehicle.Location);
                cmd.Parameters.AddWithValue("@Mileage", vehicle.Mileage.HasValue ? vehicle.Mileage.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FuelType", vehicle.FuelType);
                cmd.Parameters.AddWithValue("@DriveTrain", vehicle.DriveTrain);
                cmd.Parameters.AddWithValue("@Disposition", vehicle.Disposition);
                cmd.Parameters.AddWithValue("@Notes", vehicle.Notes);
                cmd.Parameters.AddWithValue("@ComplianceHoldReason", vehicle.ComplianceHoldReason);
                cmd.Parameters.AddWithValue("@CreatedByUserID", createdBy);
                cmd.Parameters.AddWithValue("@CreatedAt", now);
                cmd.Parameters.AddWithValue("@UpdatedAt", now);

                var id = Convert.ToInt32(cmd.ExecuteScalar());
                vehicle.VehicleID = id;
                vehicle.CreatedByUserID = createdBy;
                vehicle.CreatedAt = now;
                vehicle.UpdatedAt = now;
                return id;
            });
        }

        public async Task<bool> UpdateVehicleAsync(VehicleIntake vehicle)
        {
            if (vehicle is null)
                throw new ArgumentNullException(nameof(vehicle));
            if (vehicle.VehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(vehicle.VehicleID));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    UPDATE Vehicles
                    SET Vin = @Vin,
                        StockNumber = @StockNumber,
                        Year = @Year,
                        Make = @Make,
                        Model = @Model,
                        Trim = @Trim,
                        IntakeDate = @IntakeDate,
                        Status = @Status,
                        Location = @Location,
                        Mileage = @Mileage,
                        FuelType = @FuelType,
                        DriveTrain = @DriveTrain,
                        Disposition = @Disposition,
                        Notes = @Notes,
                        ComplianceHoldReason = @ComplianceHoldReason,
                        UpdatedAt = @UpdatedAt
                    WHERE VehicleID = @VehicleID";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Vin", vehicle.Vin.Trim());
                cmd.Parameters.AddWithValue("@StockNumber", (object?)vehicle.StockNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Year", vehicle.Year.HasValue ? vehicle.Year.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Make", vehicle.Make);
                cmd.Parameters.AddWithValue("@Model", vehicle.Model);
                cmd.Parameters.AddWithValue("@Trim", vehicle.Trim);
                cmd.Parameters.AddWithValue("@IntakeDate", vehicle.IntakeDate);
                cmd.Parameters.AddWithValue("@Status", vehicle.Status);
                cmd.Parameters.AddWithValue("@Location", vehicle.Location);
                cmd.Parameters.AddWithValue("@Mileage", vehicle.Mileage.HasValue ? vehicle.Mileage.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FuelType", vehicle.FuelType);
                cmd.Parameters.AddWithValue("@DriveTrain", vehicle.DriveTrain);
                cmd.Parameters.AddWithValue("@Disposition", vehicle.Disposition);
                cmd.Parameters.AddWithValue("@Notes", vehicle.Notes);
                cmd.Parameters.AddWithValue("@ComplianceHoldReason", vehicle.ComplianceHoldReason);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@VehicleID", vehicle.VehicleID);

                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> UpdateStatusAsync(int vehicleID, string status, string? complianceHoldReason = null)
        {
            if (vehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(vehicleID));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required", nameof(status));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var holdReason = status.Equals("OnHold", StringComparison.OrdinalIgnoreCase)
                    ? complianceHoldReason ?? string.Empty
                    : string.Empty;
                var sql = @"
                    UPDATE Vehicles
                    SET Status = @Status,
                        ComplianceHoldReason = @ComplianceHoldReason,
                        UpdatedAt = @UpdatedAt
                    WHERE VehicleID = @VehicleID";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@ComplianceHoldReason", holdReason);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@VehicleID", vehicleID);

                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> DeleteVehicleAsync(int vehicleID)
        {
            if (vehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(vehicleID));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                using var tx = conn.BeginTransaction();
                using var deleteTasks = new SqliteCommand("DELETE FROM DismantlingTasks WHERE VehicleID = @VehicleID", conn, tx);
                deleteTasks.Parameters.AddWithValue("@VehicleID", vehicleID);
                deleteTasks.ExecuteNonQuery();

                using var deleteVehicle = new SqliteCommand("DELETE FROM Vehicles WHERE VehicleID = @VehicleID", conn, tx);
                deleteVehicle.Parameters.AddWithValue("@VehicleID", vehicleID);
                var affected = deleteVehicle.ExecuteNonQuery();
                tx.Commit();
                return affected > 0;
            });
        }

        public async Task<List<VehicleIntake>> GetAllVehiclesAsync()
        {
            return await Task.Run(() =>
            {
                var vehicles = new List<VehicleIntake>();
                using var conn = _databaseService.CreateConnection();
                const string sql = "SELECT * FROM Vehicles ORDER BY IntakeDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vehicles.Add(MapVehicle(reader));
                }
                return vehicles;
            });
        }

        public async Task<List<VehicleIntake>> GetVehiclesByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required", nameof(status));

            return await Task.Run(() =>
            {
                var vehicles = new List<VehicleIntake>();
                using var conn = _databaseService.CreateConnection();
                const string sql = "SELECT * FROM Vehicles WHERE Status = @Status ORDER BY IntakeDate DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vehicles.Add(MapVehicle(reader));
                }
                return vehicles;
            });
        }

        public async Task<VehicleIntake?> GetVehicleByIdAsync(int vehicleID)
        {
            if (vehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(vehicleID));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                const string sql = "SELECT * FROM Vehicles WHERE VehicleID = @VehicleID";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapVehicle(reader);
                }
                return null;
            });
        }

        public async Task<VehiclePipelineSummary> GetPipelineSummaryAsync()
        {
            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                const string sql = "SELECT Status, COUNT(*) as Count FROM Vehicles GROUP BY Status";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                var summary = new VehiclePipelineSummary();
                while (reader.Read())
                {
                    var status = reader["Status"]?.ToString() ?? string.Empty;
                    var count = Convert.ToInt32(reader["Count"]);
                    switch (status)
                    {
                        case "Received":
                            summary.Received = count;
                            break;
                        case "OnHold":
                            summary.OnHold = count;
                            break;
                        case "Dismantling":
                            summary.Dismantling = count;
                            break;
                        case "Completed":
                            summary.Completed = count;
                            break;
                    }
                }
                return summary;
            });
        }

        public async Task<int> CreateDismantlingTaskAsync(DismantlingTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));
            if (task.VehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(task.VehicleID));
            if (string.IsNullOrWhiteSpace(task.PartName))
                throw new ArgumentException("Part name is required", nameof(task));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    INSERT INTO DismantlingTasks
                    (VehicleID, PartName, PartTag, ConditionGrade, Technician, StartedAt, CompletedAt, Status, Notes, ContainsHazmat, CreatedAt)
                    VALUES
                    (@VehicleID, @PartName, @PartTag, @ConditionGrade, @Technician, @StartedAt, @CompletedAt, @Status, @Notes, @ContainsHazmat, @CreatedAt);
                    SELECT last_insert_rowid();";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@VehicleID", task.VehicleID);
                cmd.Parameters.AddWithValue("@PartName", task.PartName);
                cmd.Parameters.AddWithValue("@PartTag", string.IsNullOrWhiteSpace(task.PartTag) ? DBNull.Value : task.PartTag);
                cmd.Parameters.AddWithValue("@ConditionGrade", task.ConditionGrade);
                cmd.Parameters.AddWithValue("@Technician", task.Technician);
                cmd.Parameters.AddWithValue("@StartedAt", task.StartedAt.HasValue ? task.StartedAt.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@CompletedAt", task.CompletedAt.HasValue ? task.CompletedAt.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(task.Status) ? "Pending" : task.Status);
                cmd.Parameters.AddWithValue("@Notes", task.Notes);
                cmd.Parameters.AddWithValue("@ContainsHazmat", task.ContainsHazmat ? 1 : 0);
                var now = DateTime.Now;
                cmd.Parameters.AddWithValue("@CreatedAt", now);

                var id = Convert.ToInt32(cmd.ExecuteScalar());
                task.TaskID = id;
                task.CreatedAt = now;
                return id;
            });
        }

        public async Task<bool> StartTaskAsync(int taskID, string technician)
        {
            if (taskID < 1)
                throw new ArgumentOutOfRangeException(nameof(taskID));
            if (string.IsNullOrWhiteSpace(technician))
                throw new ArgumentException("Technician is required", nameof(technician));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    UPDATE DismantlingTasks
                    SET Status = 'InProgress',
                        Technician = @Technician,
                        StartedAt = @StartedAt
                    WHERE TaskID = @TaskID";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Technician", technician);
                cmd.Parameters.AddWithValue("@StartedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<bool> CompleteTaskAsync(int taskID, string conditionGrade, string? notes = null)
        {
            if (taskID < 1)
                throw new ArgumentOutOfRangeException(nameof(taskID));

            return await Task.Run(() =>
            {
                using var conn = _databaseService.CreateConnection();
                var sql = @"
                    UPDATE DismantlingTasks
                    SET Status = 'Completed',
                        ConditionGrade = @ConditionGrade,
                        Notes = @Notes,
                        CompletedAt = @CompletedAt
                    WHERE TaskID = @TaskID";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ConditionGrade", conditionGrade ?? string.Empty);
                cmd.Parameters.AddWithValue("@Notes", notes ?? string.Empty);
                cmd.Parameters.AddWithValue("@CompletedAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                return cmd.ExecuteNonQuery() > 0;
            });
        }

        public async Task<List<DismantlingTask>> GetTasksForVehicleAsync(int vehicleID)
        {
            if (vehicleID < 1)
                throw new ArgumentOutOfRangeException(nameof(vehicleID));

            return await Task.Run(() =>
            {
                var tasks = new List<DismantlingTask>();
                using var conn = _databaseService.CreateConnection();
                const string sql = "SELECT * FROM DismantlingTasks WHERE VehicleID = @VehicleID ORDER BY CreatedAt DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tasks.Add(MapTask(reader));
                }
                return tasks;
            });
        }

        private static VehicleIntake MapVehicle(SqliteDataReader reader)
        {
            return new VehicleIntake
            {
                VehicleID = Convert.ToInt32(reader[nameof(VehicleIntake.VehicleID)]),
                Vin = reader[nameof(VehicleIntake.Vin)]?.ToString() ?? string.Empty,
                StockNumber = reader[nameof(VehicleIntake.StockNumber)]?.ToString() ?? string.Empty,
                Year = reader[nameof(VehicleIntake.Year)] != DBNull.Value ? Convert.ToInt32(reader[nameof(VehicleIntake.Year)]) : null,
                Make = reader[nameof(VehicleIntake.Make)]?.ToString() ?? string.Empty,
                Model = reader[nameof(VehicleIntake.Model)]?.ToString() ?? string.Empty,
                Trim = reader[nameof(VehicleIntake.Trim)]?.ToString() ?? string.Empty,
                IntakeDate = reader[nameof(VehicleIntake.IntakeDate)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(VehicleIntake.IntakeDate)]) : DateTime.MinValue,
                Status = reader[nameof(VehicleIntake.Status)]?.ToString() ?? string.Empty,
                Location = reader[nameof(VehicleIntake.Location)]?.ToString() ?? string.Empty,
                Mileage = reader[nameof(VehicleIntake.Mileage)] != DBNull.Value ? Convert.ToInt32(reader[nameof(VehicleIntake.Mileage)]) : null,
                FuelType = reader[nameof(VehicleIntake.FuelType)]?.ToString() ?? string.Empty,
                DriveTrain = reader[nameof(VehicleIntake.DriveTrain)]?.ToString() ?? string.Empty,
                Disposition = reader[nameof(VehicleIntake.Disposition)]?.ToString() ?? string.Empty,
                Notes = reader[nameof(VehicleIntake.Notes)]?.ToString() ?? string.Empty,
                ComplianceHoldReason = reader[nameof(VehicleIntake.ComplianceHoldReason)]?.ToString() ?? string.Empty,
                CreatedByUserID = reader[nameof(VehicleIntake.CreatedByUserID)] != DBNull.Value ? Convert.ToInt32(reader[nameof(VehicleIntake.CreatedByUserID)]) : 0,
                CreatedAt = reader[nameof(VehicleIntake.CreatedAt)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(VehicleIntake.CreatedAt)]) : DateTime.MinValue,
                UpdatedAt = reader[nameof(VehicleIntake.UpdatedAt)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(VehicleIntake.UpdatedAt)]) : DateTime.MinValue
            };
        }

        private static DismantlingTask MapTask(SqliteDataReader reader)
        {
            return new DismantlingTask
            {
                TaskID = Convert.ToInt32(reader[nameof(DismantlingTask.TaskID)]),
                VehicleID = Convert.ToInt32(reader[nameof(DismantlingTask.VehicleID)]),
                PartName = reader[nameof(DismantlingTask.PartName)]?.ToString() ?? string.Empty,
                PartTag = reader[nameof(DismantlingTask.PartTag)]?.ToString() ?? string.Empty,
                ConditionGrade = reader[nameof(DismantlingTask.ConditionGrade)]?.ToString() ?? string.Empty,
                Technician = reader[nameof(DismantlingTask.Technician)]?.ToString() ?? string.Empty,
                StartedAt = reader[nameof(DismantlingTask.StartedAt)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(DismantlingTask.StartedAt)]) : null,
                CompletedAt = reader[nameof(DismantlingTask.CompletedAt)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(DismantlingTask.CompletedAt)]) : null,
                Status = reader[nameof(DismantlingTask.Status)]?.ToString() ?? string.Empty,
                Notes = reader[nameof(DismantlingTask.Notes)]?.ToString() ?? string.Empty,
                ContainsHazmat = reader[nameof(DismantlingTask.ContainsHazmat)] != DBNull.Value && Convert.ToInt32(reader[nameof(DismantlingTask.ContainsHazmat)]) == 1,
                CreatedAt = reader[nameof(DismantlingTask.CreatedAt)] != DBNull.Value ? Convert.ToDateTime(reader[nameof(DismantlingTask.CreatedAt)]) : DateTime.MinValue
            };
        }
    }
}
