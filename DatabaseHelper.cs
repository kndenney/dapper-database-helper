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
    /*
     * if you inject this guy in the Startup here is an example of usage:
     * services.AddScoped<IDatabaseHelper<dynamic>(c => new DatabaseHelper<dynamic>("connection string goes here");
     * and if you need to get it from the config then use the ServiceProvider.GetServices() approach or the IOptions route
       for the connection string (so you don't have to hard code it 
     * 
     * */
    public class DatabaseHelper<T> : BaseRepository, IDatabaseHelper<T>
    {
        SqlConnection conn;

        public DatabaseHelper(string connectionString) : base(connectionString) { }

        public async Task ExecuteNonQueryAsync(string sql)
        {
            await WithConnection<Task>(async c =>
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync();
                return Task.CompletedTask;
            });
        }

        public async Task ExecuteNonQueryStoredProcedureAsync(string storedProcedure, SqlParameter[] parameters)
        {
            await WithConnection<Task>(async c =>
            {
                SqlCommand cmd = new SqlCommand(storedProcedure, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (var item in parameters)
                {
                    cmd.Parameters.Add(item);
                }

                await cmd.ExecuteNonQueryAsync();
                return Task.CompletedTask;
            });
        }

        public async Task<SqlDataReader> ExecuteReaderAsync(string sql, params SqlParameter[] parameters)
        {
            return await WithConnection<SqlDataReader>(async c =>
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                foreach (var item in parameters)
                {
                    cmd.Parameters.Add(item);
                }
                SqlDataReader reader = await cmd.ExecuteReaderAsync();
                return reader;
            }
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

        public async Task<T> ExecuteStoredProcedureTransactionQueryAsync(string storedProcedure, params SqlParameter[] parameters)
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
                    using (var transaction = conn.BeginTransaction())
                    {
                        await conn.ExecuteAsync(storedProcedure, dynamicParamters, commandType: CommandType.StoredProcedure);

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }

        public async Task<T> ExecuteSqlTransactionQueryAsync(string sql, params SqlParameter[] parameters)
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
                    using (var transaction = conn.BeginTransaction())
                    {
                        await conn.ExecuteAsync(sql, dynamicParamters, commandType: CommandType.Text);

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }
    }
}
