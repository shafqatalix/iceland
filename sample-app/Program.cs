// See https://aka.ms/new-console-template for more information


using System;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using SampleDB;

var db=new DbConfig();

var result=new SampleDB.Procedures.dbo.Ten_Most_Expensive_Products(db, new Logger()).ExecuteAsync().GetAwaiter().GetResult();

Console.WriteLine(JsonSerializer.Serialize(result));

