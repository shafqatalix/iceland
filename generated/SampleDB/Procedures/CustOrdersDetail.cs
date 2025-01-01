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
  
  public class CustOrdersDetailParameters
  {
    public int OrderID;
  }
  public class CustOrdersDetailReturnType
  {
    public int Discount;
    public decimal ExtendedPrice;
    public string ProductName;
    public short Quantity;
    public decimal UnitPrice;
  }
  public class CustOrdersDetail
  {
    private IDatabase _database;
    private ILogger _logger;
    private ActivitySource _activity;
    public CustOrdersDetail(IDatabase database, ILogger logger = null)
    {
      this._database = database;
      this._logger = logger;
      this._activity = new ActivitySource("StoredProcedure", Helpers.Version);
    }
    public virtual async Task<CustOrdersDetailReturnType> ExecuteAsync(CustOrdersDetailParameters args)
    {
var parameters=new SqlParameter[1];
var OrderID = new SqlParameter("@OrderID", args.OrderID);
parameters[0]= OrderID;

var result = await Helpers.ExecuteStoredProcedureWithReaderAsync<CustOrdersDetailReturnType>(_database, _activity, _logger, "dbo.CustOrdersDetail", parameters);
      return result.FirstOrDefault();
    }
  }
}