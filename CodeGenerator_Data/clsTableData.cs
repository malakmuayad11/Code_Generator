using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Logger;

namespace CodeGenerator_Data
{
    public static class clsTableData
    {
        public static async Task<DataTable> GetTablesForDatabase(string DatabaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(clsSettingsData.ConnectionString)
            {
                InitialCatalog = DatabaseName
            };
            DataTable Parameters = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    string query = $@"
                            SELECT name AS TableName
                            FROM sys.tables
                            WHERE name <> 'sysdiagrams'
                            ORDER BY name;
                            ";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                                Parameters.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await clsLogger.Log(ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return Parameters;
        }
    }
}