using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator_Business
{
    public class clsGeneratorData : IGenerator
    {
        public string TableName { get; set; }
        public string ProjectName { get; set; }

        public clsGeneratorData(string tableName, string projectName)
        {
            TableName = tableName;
            ProjectName = projectName;
        }

        /// <summary>
        /// Generates C# code for the necessary imports and class declaration.
        /// </summary>
        /// <returns>C# code for the data access layer, that sets necessay imports and class declaration.</returns>
        private string _GenerateImportsAndClassName()
        {
            string result = $@"
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;

namespace {ProjectName}_Data
{{
public class cls{TableName.TrimEnd('s')}Data
{{
";
            return result;
        }

        /// <summary>
        /// Generates C# code that retrieves all records from the table.
        /// It assumes the stored procedure is named: SP_GetAll[TableName]s
        /// </summary>
        /// <returns>C# code for the data access layer, that retrieves all records
        /// from a table.</returns>
        private string _GenerateGetAll() =>
           "\n" + $@"public async static Task<DataTable> GetAll{TableName}Async()
{{
    DataTable dt = new DataTable();
    try
    {{
         using (SqlConnection connection = new SqlConnection(clsSettingData.ConnectionString))
         {{
             await connection.OpenAsync();
             using (SqlCommand command = new SqlCommand(""SP_GetAll{TableName}"", connection))
             {{
                 command.CommandType = CommandType.StoredProcedure;
                 using (SqlDataReader reader = await command.ExecuteReaderAsync())
                 {{
                     if (reader.HasRows)
                         dt.Load(reader);
                 }}
             }}
         }}
    }}
    catch (SqlException ex)
    {{
        clsLogger.Log(ex.Message, EventLogEntryType.Error);
    }}
    return dt;
}}";


        /// <summary>
        /// Generates C# code that adds a new record to the table.
        /// It assumes the stored procedure is named: SP_AddNew[TableName].
        /// </summary>
        /// <returns>C# code for the data access layer, that adds a new 
        /// record to the table.</returns>
        private async Task<string> _GenerateAddNewAsync()
        {
            string StoredProcedureName = $"SP_AddNew{TableName.TrimEnd('s')}";
            DataTable parameters = await clsParamater.GetParametersForStoredProcedure(StoredProcedureName);

            StringBuilder result =
                new StringBuilder($"\n\npublic static int {StoredProcedureName.Remove(0, 3).TrimEnd('s')}(");

            foreach (DataRow parameter in parameters.Rows)
            {
                if (parameter["IsOutput"].ToString() == "INOUT" || parameter["IsOutput"].ToString() == "OUT")
                    continue;
                result.Append($"{clsUtil.MapSqlTypeToCSharpType(parameter["DataType"].ToString())} " +
                    $"{parameter["ParameterName"].ToString().TrimStart('@')}, ");
            }

            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(','));
            result.Append($@")
{{
    int? {TableName.TrimEnd('s')}ID = null;

    try
    {{
        using (SqlConnection connection = new SqlConnection(clsSettingData.ConnectionString))
        {{
            connection.Open();
            using (SqlCommand command = new SqlCommand(""{StoredProcedureName}"", connection))
            {{
                command.CommandType = CommandType.StoredProcedure;");

            foreach (DataRow parameter in parameters.Rows)
            {
                if ((parameter["IsOutput"].ToString() == "INOUT" || parameter["IsOutput"].ToString() == "OUT"))
                    continue;
                result.AppendLine();
                result.Append($"\t\t\t\tcommand.Parameters.AddWithValue" +
                    $"(\"{parameter["ParameterName"]}\", {parameter["ParameterName"].ToString().TrimStart('@')});");
            }

            result.Append($@"
                SqlParameter outputIdParam = new SqlParameter(""@{TableName.Substring(0, TableName.Length - 1)}ID"", SqlDbType.Int)
                {{
                    Direction = ParameterDirection.Output
                }};
                command.Parameters.Add(outputIdParam);
                command.ExecuteNonQuery();
                {TableName.TrimEnd('s')}ID = (int)(outputIdParam.Value);
            }}
        }}
    }}
    catch (SqlException ex)
    {{
        clsLoggerData.Log(ex.Message, System.Diagnostics.EventLogEntryType.Error);
    }}
    return {TableName.TrimEnd('s')}ID ?? -1;
}}");
            return result.ToString();
        }

        /// <summary>
        /// Generates C# code that updates an existing record in the table.
        /// It assumes the stored procedure is named: SP_Update[TableName].
        /// </summary>
        /// <returns>C# code for the data access layer, that updates 
        /// an existing record in the table.</returns>
        private async Task<string> _GenerateUpdateAsync()
        {
            string StoredProcedureName = $"SP_Update{TableName.TrimEnd('s')}";
            DataTable parameters = await clsParamater.GetParametersForStoredProcedure(StoredProcedureName);

            StringBuilder result = new StringBuilder($"\n\npublic static bool Update{TableName.TrimEnd('s')}(");
            foreach (DataRow parameter in parameters.Rows)
            {
                result.Append($"{clsUtil.MapSqlTypeToCSharpType(parameter["DataType"].ToString())} " +
                    $"{parameter["ParameterName"].ToString().TrimStart('@')}, ");
            }

            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(',') + ")");

            result.Append($@"
{{
    int rowsEffected = 0;
    try
    {{
        using (SqlConnection connection = new SqlConnection(clsSettingData.ConnectionString))
        {{
            connection.Open();
            using (SqlCommand command = new SqlCommand(""{StoredProcedureName}"", connection))
            {{
                command.CommandType = CommandType.StoredProcedure;");

            foreach (DataRow parameter in parameters.Rows)
            {
                result.AppendLine();
                result.Append($"\t\t\t\tcommand.Parameters.AddWithValue" +
                    $"(\"{parameter["ParameterName"]}\", {parameter["ParameterName"].ToString().TrimStart('@')});");
            }
            result.Append(@"
                rowsEffected = command.ExecuteNonQuery();
            }
        }
    }
    catch (SqlException ex)
    {
        clsLoggerData.Log(ex.Message, EventLogEntryType.Error);
    }
    return rowsEffected > 0;
}");
            return result.ToString();
        }

        /// <summary>
        /// Generates C# code that deletes a record from the table. 
        /// It assumes the stored procedure is named: SP_Delete[TableName].
        /// </summary>
        /// <returns>C# code for the data access layer, that deletes 
        /// an existing record in the table.</returns>
        private async Task<string> _GenerateDeleteAsync()
        {
            string StoredProcedureName = $"SP_Delete{TableName.TrimEnd('s')}";
            DataTable parameters = await clsParamater.GetParametersForStoredProcedure(StoredProcedureName);

            StringBuilder result = new StringBuilder($"\n\npublic static bool Delete{TableName.TrimEnd('s')}(");

            foreach (DataRow parameter in parameters.Rows)
            {
                result.Append($"{clsUtil.MapSqlTypeToCSharpType(parameter["DataType"].ToString())} " +
                    $"{parameter["ParameterName"].ToString().TrimStart('@')}, ");
            }

            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(',') + ")");

            result.Append($@"
{{
    int Result = 0;
    try
    {{
        using (SqlConnection connection = new SqlConnection(clsSettingData.ConnectionString))
        {{
            connection.Open();
            using (SqlCommand command = new SqlCommand(""{StoredProcedureName}"", connection))
            {{
                command.CommandType = CommandType.StoredProcedure;
");
            foreach (DataRow parameter in parameters.Rows)
            {
                result.AppendLine();
                result.Append($"\t\t\t\tcommand.Parameters.AddWithValue" +
                    $"(\"{parameter["ParameterName"]}\", {parameter["ParameterName"].ToString().TrimStart('@')});");
            }
            result.Append($@"
                Result = (int)command.ExecuteScalar();
            }}
        }}
    }}
    catch(SqlException ex)
    {{
        clsLoggerData.Log(ex.Message, EventLogEntryType.Error);
    }}
    return Result > 0;
}}");
            return result.ToString();
        }

        /// <summary>
        /// Generates class CRUD code for data access layer.
        /// </summary>
        /// <returns>C# class CRUD code.</returns>
        public async Task<string> GenerateCRUDCodeForClass()
        {
            StringBuilder result = new StringBuilder();
            result.Append(_GenerateImportsAndClassName());
            result.Append(_GenerateGetAll());
            result.Append(await _GenerateAddNewAsync());
            result.Append(await _GenerateUpdateAsync());
            result.Append(await _GenerateDeleteAsync());
            return result.Append("\n}").ToString();
        }
    }
}