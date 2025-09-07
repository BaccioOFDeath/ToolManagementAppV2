using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp.Services
{
    public class StaffService : IStaffService
    {
        private readonly DatabaseService _db;

        public StaffService(DatabaseService db)
        {
            _db = db;
        }

        public async Task<IEnumerable<KeyValuePair<int, string>>> GetStaffAsync(CancellationToken cancellationToken = default)
        {
            var staff = new List<KeyValuePair<int, string>>();
            using var conn = _db.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserID, UserName FROM Users ORDER BY UserName";
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var id = reader.GetInt32(0);
                var name = reader.IsDBNull(1) ? id.ToString() : reader.GetString(1);
                staff.Add(new KeyValuePair<int, string>(id, name));
            }
            return staff;
        }
    }
}
