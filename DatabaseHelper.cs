using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DapperDatabaseHelper
{
    public class DatabaseHelper<T> : BaseRepository, IDatabaseHelper<T>
    {
        SqlConnection conn;

        public DatabaseHelper(string connectionString) : base(connectionString) { }

        public async Task ExecuteNonQueryAsync(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<SqlDataReader> ExecuteReaderAsync(string sql, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(sql, conn);
            foreach(var item in parameters)
            {
                cmd.Parameters.Add(item);
            }
            SqlDataReader reader = await cmd.ExecuteReaderAsync();
            return reader;
        }

        public async Task<int> ExecuteCommandQueryAsync(string sql, params SqlParameter[] parameters)
        {
            return await WithConnection<int>(async c => {

                List<T> list = new List<T>();

                DynamicParameters dynamicParamters = new DynamicParameters();

                foreach (var item in parameters)
                {
                    dynamicParamters.Add(item.ParameterName, item.Value);
                }

                try
                {
                    var data = await conn.ExecuteAsync(sql, dynamicParamters, commandType: CommandType.Text);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }

        public async Task<T> ExecuteStoredProcedureQueryAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            return await WithConnection<T>(async c => {

                List<T> list = new List<T>();

                DynamicParameters dynamicParamters = new DynamicParameters();

                foreach (var item in parameters)
                {
                    dynamicParamters.Add(item.ParameterName, item.Value);
                }

                try
                {
                    var data = await conn.QueryAsync<T>(storedProcedure, dynamicParamters, commandType: CommandType.StoredProcedure);
                    return data.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }

        public async Task<List<IEnumerable<T>>> ExecuteStoredProcedureQueryMultipleAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            return await WithConnection<List<IEnumerable<T>>>(async c => {

                List<IEnumerable<IEnumerable<T>>> list = new List<IEnumerable<IEnumerable<T>>>();

                DynamicParameters TParamters = new DynamicParameters();

                foreach (var item in parameters)
                {
                    TParamters.Add(item.ParameterName, item.Value);
                }

                try
                {
                    var data = await conn.QueryMultipleAsync(storedProcedure, TParamters, commandType: CommandType.StoredProcedure);

                    List<IEnumerable<T>> d = new List<IEnumerable<T>>();
                    while (data.IsConsumed == false)
                    {
                        d.Add(await data.ReadAsync<T>());
                    }

                    return d;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }
    }
}
