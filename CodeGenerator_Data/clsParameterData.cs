using System;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Logger;

namespace CodeGenerator_Data
{
    public static class clsParamaterData
    {
        public static async Task<DataTable> GetParametersForStoredProcedure(string StoredProcedureName)
        {
            DataTable Parameters = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(clsSettingsData.ConnectionString))
                {
                    string query = $@"
                            SELECT 
                                PARAMETER_NAME as ParameterName,
                                DATA_TYPE as DataType,
                                PARAMETER_MODE as IsOutput
                            FROM INFORMATION_SCHEMA.PARAMETERS
                            WHERE SPECIFIC_NAME = '{StoredProcedureName}';";

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