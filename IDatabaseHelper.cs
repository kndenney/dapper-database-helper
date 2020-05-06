using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DapperDatabaseHelper
{
    public interface IDatabaseHelper<T>
    {
        Task ExecuteNonQueryAsync(string sql);
        Task<SqlDataReader> ExecuteReaderAsync(string sql, params SqlParameter[] parameters);
        Task<int> ExecuteCommandQueryAsync(string sql, params SqlParameter[] parameters);
        Task<T> ExecuteStoredProcedureQueryAsync(string storedProcedure, params SqlParameter[] parameters);
        Task<List<IEnumerable<T>>> ExecuteStoredProcedureQueryMultipleAsync(string storedProcedure, params SqlParameter[] parameters);
    }
}
