using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DapperDatabaseHelper
{
	public abstract class BaseRepository
	{
		private readonly string _connectionString;

		protected BaseRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		protected async Task<T> WithConnection<T>(Func<IDbConnection, Task<T>> executeAsync)
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync();
					return await executeAsync(connection);
				}
			}
			catch (TimeoutException ex)
			{
				throw new Exception(String.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
			}
			catch (SqlException ex)
			{
				throw new Exception(String.Format("{0}.WithConnection() experienced a SQL exception", GetType().FullName), ex);
			}
		}
	}
}
