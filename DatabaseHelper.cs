using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
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
        SqlConnection conn = null;
        string _connectionString = "";

        public DatabaseHelper(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExecuteNonQueryAsync(string sql)
        {
            await WithConnection<Task>(async c =>
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    await cmd.ExecuteNonQueryAsync();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, sql), ex);
                }
            });
        }

        public async Task ExecuteNonQueryStoredProcedureAsync(string storedProcedure, SqlParameter[] parameters)
        {
            await WithConnection<Task>(async c =>
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(storedProcedure, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }

                    await cmd.ExecuteNonQueryAsync();
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        //We need to figure out a way to return an object from this reader rather than the sqlDataareder itself
        //https://www.codeproject.com/Articles/827984/Generically-Populate-List-of-Objects-from-SqlDataR
        //This might be enough too
        //https://codereview.stackexchange.com/questions/58251/transform-datareader-to-listt-using-reflections

        //https://stackoverflow.com/questions/10252531/returning-a-sqldatareader
        public IEnumerator<SqlDataReader> ExecuteDataReaderSqlReturnDataReader(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                foreach (var item in parameters)
                {
                    cmd.Parameters.Add(item);
                }
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    yield return reader;
                }
                conn.Close();
            }
        }

        public async Task<SqlDataReader> ExecuteReaderSqlAsync(string sql, params SqlParameter[] parameters)
        {
            return await WithConnection<SqlDataReader>(async c =>
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    return reader;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task<SqlDataReader> ExecuteReaderStoredProcedureAsync(string sql, params SqlParameter[] parameters)
        {
            return await WithConnection<SqlDataReader>(async c =>
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                    cmd.CommandType = CommandType.StoredProcedure;

                    //Keeps the connection open until reader is closed
                    SqlDataReader reader = await cmd.ExecuteReaderAsync(); // CommandBehavior.CloseConnection);

                    if (await reader.ReadAsync())
                    {
                        return reader;
                    }
                    return reader;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task<int> ExecuteCommandQueryAsync(string sql, params SqlParameter[] parameters)
        {
            return await WithConnection<int>(async c =>
            {
                try
                {
                    List<T> list = new List<T>();

                    DynamicParameters dynamicParamters = new DynamicParameters();

                    foreach (var item in parameters)
                    {
                        dynamicParamters.Add(item.ParameterName, item.Value);
                    }


                    var data = await c.ExecuteAsync(sql, dynamicParamters, commandType: CommandType.Text);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task<IEnumerable<U>> ExecuteStoredProcedureQueryAsync<U>(string storedProcedure, params SqlParameter[] parameters)
        {
            return await WithConnection<IEnumerable<U>>(async c =>
            {
                try
                {
                    List<T> list = new List<T>();

                    DynamicParameters dynamicParamters = new DynamicParameters();

                    foreach (var item in parameters)
                    {
                        dynamicParamters.Add(item.ParameterName, item.Value);
                    }


                    var data = await c.QueryAsync<U>(storedProcedure, dynamicParamters, commandType: CommandType.StoredProcedure);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task<List<IEnumerable<T>>> ExecuteStoredProcedureQueryMultipleAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            return await WithConnection<List<IEnumerable<T>>>(async c =>
            {
                try
                {
                    List<IEnumerable<IEnumerable<T>>> list = new List<IEnumerable<IEnumerable<T>>>();

                    DynamicParameters TParamters = new DynamicParameters();

                    foreach (var item in parameters)
                    {
                        TParamters.Add(item.ParameterName, item.Value);
                    }


                    var data = await c.QueryMultipleAsync(storedProcedure, TParamters, commandType: CommandType.StoredProcedure);

                    List<dynamic> d = new List<dynamic>();
                   /* while (data.IsConsumed == false)
                    {
                        d.Add(await data.ReadAsync<T>());
                    } */
                    
                    while (data.IsConsumed == false) {
                        foreach (await var item in data.ReadAsync())
                        {
                            d.Add(item);
                        }
                    }
                    return d;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task ExecuteStoredProcedureTransactionQueryAsync(string storedProcedure, params SqlParameter[] parameters)
        {
            await WithConnection<Task>(async c =>
            {
                try
                {
                    List<T> list = new List<T>();

                    DynamicParameters dynamicParamters = new DynamicParameters();

                    foreach (var item in parameters)
                    {
                        dynamicParamters.Add(item.ParameterName, item.Value);
                    }


                    using (var transaction = c.BeginTransaction())
                    {
                        await c.ExecuteAsync(storedProcedure, dynamicParamters, commandType: CommandType.StoredProcedure);

                        transaction.Commit();
                        return Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public async Task ExecuteSqlTransactionQueryAsync(string sql, params SqlParameter[] parameters)
        {
            await WithConnection<Task>(async c =>
            {
                try
                {
                    List<T> list = new List<T>();

                    DynamicParameters dynamicParamters = new DynamicParameters();

                    foreach (var item in parameters)
                    {
                        dynamicParamters.Add(item.ParameterName, item.Value);
                    }


                    using (var transaction = c.BeginTransaction())
                    {
                        await c.ExecuteAsync(sql, dynamicParamters, commandType: CommandType.Text);

                        transaction.Commit();
                        return Task.CompletedTask;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<SqlParameter>(",", parameters)), ex);
                }
            });
        }

        public DataTable ConvertObjectToDataTable<U>(IEnumerable<U> data)
        {
            try
            {
                PropertyDescriptorCollection props =
                    TypeDescriptor.GetProperties(typeof(U));

                DataTable dt = new DataTable();

                foreach (PropertyDescriptor p in props)
                {
                    dt.Columns.Add(p.Name, p.PropertyType);
                }

                object[] values = new object[props.Count];
                foreach (U item in data)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }
                    dt.Rows.Add(values);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("{0} experienced an exception with parameters {1}", GetType().FullName, String.Join<U>(",", data)), ex);
            }
        }

        public async Task<IEnumerable<U>> ExecuteQueryTableValueParameterStoredProcedure<U>(string storedProcedure, IEnumerable<U> dataToConvert)
        {
            return await WithConnection<IEnumerable<U>>(async c =>
            {

                List<T> list = new List<T>();

                DataTable dt = new DataTable();

                dt = ConvertObjectToDataTable(dataToConvert);

                try
                {
                    var data = await c.QueryAsync<U>(storedProcedure, dt.AsTableValuedParameter(), commandType: CommandType.StoredProcedure);
                    return data;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("{0}.WithConnection() experienced an exception", GetType().FullName), ex);
                }
            });
        }
    }
}
