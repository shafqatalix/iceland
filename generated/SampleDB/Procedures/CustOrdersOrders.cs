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
  
  public class CustOrdersOrdersParameters
  {
public System.String CustomerID {get;set;}
  }
  public class CustOrdersOrdersReturnType
  {
public System.DateTime OrderDate {get;set;}public System.Int32 OrderID {get;set;}public System.DateTime RequiredDate {get;set;}public System.DateTime ShippedDate {get;set;}
  }
  public class CustOrdersOrders
  {
    private IDatabase _database;
    private ILogger _logger;
    private ActivitySource _activity;
    public CustOrdersOrders(IDatabase database, ILogger logger = null)
    {
      this._database = database;
      this._logger = logger;
      this._activity = new ActivitySource("StoredProcedure", Helpers.Version);
    }
    public virtual async Task<CustOrdersOrdersReturnType> ExecuteAsync(CustOrdersOrdersParameters args)
    {
var parameters=new SqlParameter[1];
var CustomerID = new SqlParameter("@CustomerID", args.CustomerID);
parameters[0]= CustomerID;

var result = await Helpers.ExecuteStoredProcedureWithReaderAsync<CustOrdersOrdersReturnType>(_database, _activity, _logger, "[dbo].[CustOrdersOrders]", parameters);
      return result.FirstOrDefault();
    }
  }
}
