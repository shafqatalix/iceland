//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UserDefinedTypes;


namespace SampleDB.Procedures.dbo
{
  
  public class Ten Most Expensive ProductsReturnType
  {
    public string TenMostExpensiveProducts;
    public decimal UnitPrice;
  }
  public class Ten Most Expensive Products
  {
    private IDatabase _database;
    private ILogger _logger;
    private ActivitySource _activity;
    public Ten Most Expensive Products(IDatabase database, ILogger logger = null)
    {
      this._database = database;
      this._logger = logger;
      this._activity = new ActivitySource("StoredProcedure", Helpers.Version);
    }
    public virtual async Task<Ten Most Expensive ProductsReturnType> ExecuteAsync()
    {
var parameters=new SqlParameter[0];
var result = await Helpers.ExecuteStoredProcedureWithReaderAsync<Ten Most Expensive ProductsReturnType>(_database, _activity, _logger, "dbo.Ten Most Expensive Products", parameters);
      return result.FirstOrDefault();
    }
  }
}