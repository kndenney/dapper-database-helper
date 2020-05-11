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
        Task<SqlDataReader> ExecuteReaderSqlAsync(string sql, params SqlParameter[] parameters);
        Task<SqlDataReader> ExecuteReaderStoredProcedureAsync(string sql, params SqlParameter[] parameters);
        Task<int> ExecuteCommandQueryAsync(string sql, params SqlParameter[] parameters);
        IEnumerator<SqlDataReader> ExecuteDataReaderSqlReturnDataReader(string sql, params SqlParameter[] parameters);
        Task<IEnumerable<U>> ExecuteStoredProcedureQueryAsync<U>(string storedProcedure, params SqlParameter[] parameters);
        Task<List<IEnumerable<T>>> ExecuteStoredProcedureQueryMultipleAsync(string storedProcedure, params SqlParameter[] parameters);
    }
}
