
using System.Text.Json;
using Microsoft.Data.SqlClient;


internal static class DbExecute
{

	public static string ConnectionString { get; set; }

	public static string GetDBName()
	{
		using (var conn = new SqlConnection(ConnectionString))
		{
			return conn.Database;
		}
	}

	public static MetaData GetMeta()
	{
		var rows=new List< Procedure>();
		using (var conn = new SqlConnection(ConnectionString))
		{
			using (var comm = new SqlCommand(Files.ProceduresMetaSQL(), conn))
			{
				conn.Open();
				using (var dr = comm.ExecuteReader())
				{
					string className = string.Empty;
					string procName = string.Empty;

					while (dr.Read())
					{
						var meta=new Procedure();
						meta.Name = dr["Name"].ToString();
						meta.Schema = dr["Schema"].ToString();
						if (dr["Parameters"] != DBNull.Value)
						{
							meta.Parameters = JsonSerializer.Deserialize<Parameter[]>(dr["Parameters"].ToString());
						}
						if (dr["ReturnType"] != DBNull.Value)
						{
							meta.ReturnType = JsonSerializer.Deserialize<ReturnType[]>(dr["ReturnType"].ToString());
						}
						if (dr["Dependencies"] != DBNull.Value)
						{
							meta.Dependencies = JsonSerializer.Deserialize<Dependency[]>(dr["Dependencies"].ToString());
						}
						rows.Add(meta);
					}
				}
			}
		}
		return new MetaData
		{
			Procedures = rows.ToArray()
		};
	}

}














