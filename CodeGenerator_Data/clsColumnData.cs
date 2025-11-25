using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Logger;

namespace CodeGenerator_Data
{
    public static class clsColumnData
    {
        public static async Task<DataTable> GetColumns(string TableName)
        {
            DataTable Columns = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(clsSettingsData.ConnectionString))
                {
                    string query = $@"
                        SELECT 
                        COLUMN_NAME as ColumnName, 
                        DATA_TYPE as DataType, 
                        IS_NULLABLE as IsNullable
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = '{TableName}'
                        ORDER BY ORDINAL_POSITION;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                                Columns.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               await clsLogger.Log(ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            return Columns;
        }
    }
}