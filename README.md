# dapper-database-helper
This repository is a helper class for basic read/write operations for the Dapper ORM

For configuration purposes:

Add the following line in the Startup.cs of your C# .Net Core Project (if that is what you are using)

services.AddScoped(typeof(IDatabaseHelper<>), typeof(DatabaseHelper<>));

Also to create a new instance of the class you can use dependency injection like so:

private readonly IDatabaseHelper<dynamic> _databaseHelper;

Where dynamic is the type that you want to use for the return methods of the classes within the database helper (you can change this per your requirement).

To use the functions as an example:

List<SqlParameter> paramters = new List<SqlParameter> {
  new SqlParameter("@ParameterName", SqlDbType.VarChar), { Value = "SomePropertyValue" }
}

var example = await _databaseHelper.ExecuteReaderAsync("SELECT or Stored Procedure Goes Here", parameters.ToArray());

Because we are using the 'dynamic' type we can type cast as needed OR we can create new instances of the DatabaseHelper class to fit the needs that we have per 
method call needed. 
