
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
        var procedures=new List< Procedure>();
        var udts=new List<UTD>();

        using (var conn = new SqlConnection(ConnectionString))
        {
            using (var comm = new SqlCommand(Files.MetaSQL(), conn))
            {
                conn.Open();
                using (var dr = comm.ExecuteReader())
                {
                    string className = string.Empty;
                    string procName = string.Empty;

                    // Procedures
                    while (dr.Read())
                    {
                        var meta=new Procedure();
                        meta.Name = dr["Name"].ToString();
                        meta.DisplayName = dr["DisplayName"].ToString();
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
                        procedures.Add(meta);
                    }
                    dr.NextResult();
                    // UDTs
                    while (dr.Read())
                    {
                        var meta=new UTD();
                        meta.Id = dr["Id"].ToString();
                        meta.Name = dr["Name"].ToString();
                        meta.Schema = dr["Schema"].ToString();
                        meta.DisplayName = dr["DisplayName"].ToString();
                        if (dr["Fields"] != DBNull.Value)
                        {
                            meta.Fields = JsonSerializer.Deserialize<Field[]>(dr["Fields"].ToString());
                        }
                        if (dr["IsTableType"] != DBNull.Value)
                        {
                            meta.IsTableType = Boolean.Parse(dr["IsTableType"].ToString());
                        }
                        udts.Add(meta);
                    }

                }
            }
        }
        return new MetaData
        {
            Procedures = procedures.ToArray(),
            UserDefinedTypes = udts.ToArray()
        };
    }

}














