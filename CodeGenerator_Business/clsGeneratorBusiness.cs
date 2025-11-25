using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator_Business
{
    public class clsGeneratorBusiness : IGeneratorBusiness
    {
        public string TableName { get; set; }
        public string ProjectName { get; set; }
        public string EntityName { get; }

        private DataTable _TableColumns;

        /// <summary>
        /// Asynchronously creates a new instance of <see cref="clsGeneratorBusiness"/> for the specified table and
        /// project.
        /// </summary>
        /// <remarks>This method initializes the <see cref="clsGeneratorBusiness"/> instance with the
        /// specified table and project names, and populates its table columns asynchronously.</remarks>
        /// <param name="tableName">The name of the table for which to generate business logic. Cannot be null or empty.</param>
        /// <param name="projectName">The name of the project associated with the business logic. Cannot be null or empty.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the newly created <see
        /// cref="clsGeneratorBusiness"/> instance.</returns>
        public static async Task<clsGeneratorBusiness> CreateAsync(string tableName, string projectName)
        {
            clsGeneratorBusiness instance = new clsGeneratorBusiness(tableName, projectName);
            instance._TableColumns = await clsColumn.GetColumnsForTable(tableName);
            return instance;
        }

        private clsGeneratorBusiness(string tableName, string projectName)
        {
            TableName = tableName;
            EntityName = TableName.TrimEnd('s');
            //_TableColumns = Task.Run(async () => await clsColumn.GetColumnsForTable(TableName)).Result;
            ProjectName = projectName;
        }

        /// <summary>
        /// Generates C# property definitions for each column in the specified table.
        /// </summary>
        /// <remarks>Each property corresponds to a column in the table, with the data type mapped from
        /// SQL to C#. Nullable columns are represented with nullable types. Additionally, an enumeration and a private
        /// field for mode management are included in the generated string.</remarks>
        /// <param name="TableName">The name of the table for which to generate property definitions.</param>
        /// <returns>A string containing C# property definitions for each column in the table, or an empty string if there are no
        /// columns.</returns>
        private string _GenerateProperties(string TableName)
        {
            if (_TableColumns.Rows.Count == 0)
                return string.Empty;

            StringBuilder result = new StringBuilder();

            foreach (DataRow TableColumn in _TableColumns.Rows)
            {
                result.AppendLine();
                result.Append("\npublic " + $"{clsUtil.MapSqlTypeToCSharpType(TableColumn["DataType"].ToString())}");
                result.Append(TableColumn["IsNullable"].ToString() == "YES" ? "?" : "");
                result.Append($" {TableColumn["ColumnName"]} {{ get; set; }}");
            }

            result.Append("\npublic enum enMode { AddNew, Update }");
            result.Append("\nprivate enMode _Mode;");
            return result.ToString();
        }

        /// <summary>
        /// Generates a public constructor for the class with default initialization for each property.
        /// </summary>
        /// <remarks>The constructor initializes each property of the class to its default value based on
        /// its data type. Nullable properties are set to <see langword="null"/>. The mode is set to
        /// <c>enMode.AddNew</c>.</remarks>
        /// <returns>A string containing the C# code for the public constructor of the class. Returns an empty string if there
        /// are no columns to initialize.</returns>
        private string _GeneratePublicConstructor()
        {
            if (_TableColumns.Rows.Count == 0)
                return string.Empty;

            StringBuilder result = new StringBuilder();
            result.Append("\n\npublic cls" + $"{EntityName}()" + "\n{");
            foreach (DataRow TableColumn in _TableColumns.Rows)
            {
                result.Append("\n\tthis." + $"{TableColumn["ColumnName"]} = ");
                result.Append(TableColumn["IsNullable"].ToString() == "YES" ? "null;"
                    : clsUtil.GetDefaultValueForCSharpType(
                        clsUtil.MapSqlTypeToCSharpType(TableColumn["DataType"].ToString())) + ";");
            }
            result.Append("\n\tthis._Mode = enMode.AddNew;\n}");
            return result.ToString();
        }

        /// <summary>
        /// Generates a private constructor for the class based on the table columns.
        /// </summary>
        /// <remarks>This method constructs a private constructor string for the class, initializing
        /// properties with the provided table column data types and names. The constructor sets the mode to
        /// update.</remarks>
        /// <returns>A string representing the private constructor of the class. Returns an empty string if there are no table
        /// columns.</returns>
        private string _GeneratePrivateConstructor()
        {
            if (_TableColumns.Rows.Count == 0)
                return string.Empty;

            StringBuilder result = new StringBuilder();

            result.Append("\n\nprivate cls" + $"{EntityName}(");
            foreach (DataRow TableColumn in _TableColumns.Rows)
            {
                result.Append($"{clsUtil.MapSqlTypeToCSharpType(TableColumn["DataType"].ToString())}");
                result.Append(TableColumn["IsNullable"].ToString() == "YES" ? "?" : "");
                result.Append($" {TableColumn["ColumnName"]}, ");
            }
            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(','));
            result.Append($")\n{{");
            foreach (DataRow TableColumn in _TableColumns.Rows)
            {
                result.Append("\n\tthis." + $"{TableColumn["ColumnName"]} = {TableColumn["ColumnName"]};");
            }
            result.Append("\n\tthis._Mode = enMode.Update;\n}");
            return result.ToString();
        }

        /// <summary>
        /// Generates a string containing the necessary import statements and the class declaration for the specified
        /// entity within the business namespace.
        /// </summary>
        /// <returns>A string that includes the import statements for the data and system namespaces, and the class declaration
        /// for the entity specified by <see cref="EntityName"/>.</returns>
        private string _GenerateImportsAndClassName()
        {
            string result = $@"
﻿using {ProjectName}_Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace {ProjectName}_Business
{{
public class cls{EntityName}
{{
";
            return result;
        }

        /// <summary>
        /// Generates C# code that retrieves all records from the table.
        /// </summary>
        /// <returns>C# code for the business layer, that retrives all records from 
        /// the table. It assumes that TableName is plural.</returns>
        private string _GenerateGetAll() =>
             $"\n\npublic static async Task<DataTable> GetAll{TableName}Async() => await cls{TableName.Substring(0, TableName.Length - 1)}Data.GetAll{TableName}Async();";

        /// <summary>
        /// Generates C# code that adds a new record to the table.
        /// </summary>
        /// <returns>C# code for the business layer, that adds a new
        /// record to the table. It assumes that the TableName is plural.</returns>
        private async Task<string> _GenerateAddNewAsync()
        {
            string StoredProcedureName = $"SP_AddNew{EntityName}";

            StringBuilder result = new StringBuilder(
               "\n\n" + $@"private bool _AddNew{EntityName}()
{{
    this.{EntityName}ID = cls{EntityName}Data.AddNew{EntityName}(");

            DataTable parameters = await clsParamater.GetParametersForStoredProcedure(StoredProcedureName);

            foreach (DataRow parameter in parameters.Rows)
            {
                if (parameter["IsOutput"].ToString() == "INOUT" || parameter["IsOutput"].ToString() == "OUT")
                    continue;
                result.Append($"this.{parameter["ParameterName"].ToString().TrimStart('@')}, ");
            }

            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(','));

            result.Append($");\n\treturn this.{EntityName}ID != -1;\n}}");
            return result.ToString();
        }

        /// <summary>
        /// Generates C# code that updates an existing record in the table.
        /// </summary>
        /// <returns>C# code for the business layer, that updates an
        /// existing record in the table. It assumes that the TableName is plural.</returns>
        private async Task<string> _GenerateUpdateAsync()
        {
            string StoredProcedureName = $"SP_Update{EntityName}";

            StringBuilder result = new StringBuilder(
               "\n\n" + $@"private bool _Update{EntityName}() => cls{EntityName}Data.Update{EntityName}(");

            DataTable parameters = await clsParamater.GetParametersForStoredProcedure(StoredProcedureName);

            foreach (DataRow parameter in parameters.Rows)
            {
                if (parameter["IsOutput"].ToString() == "INOUT" || parameter["IsOutput"].ToString() == "OUT")
                    continue;
                result.Append($"this.{parameter["ParameterName"].ToString().TrimStart('@')}, ");
            }

            result = new StringBuilder(result.ToString().TrimEnd(' ').TrimEnd(',')).Append(");");

            return result.ToString();
        }

        /// <summary>
        /// Generates C# code that saves the current entity.
        /// </summary>
        /// <returns>C# code for the business layer, that saves
        /// the current entity state in the table. It assumes that the TableName is plural.</returns>
        private string _GenerateSave()
        {
            string result = "\n\n" + $@"
public bool Save()
{{
    switch (this._Mode)
    {{
           case enMode.AddNew:
           {{
                  if (_AddNew{EntityName}())
                  {{
                        this._Mode = cls{EntityName}.enMode.Update;
                        return true;
                  }}
           }}
           break;
           case enMode.Update:
                return _Update{EntityName}();
    }}
    return false;
}}";
            return result;
        }

        /// <summary>
        /// Generates C# code that deletes the specified entity from the table.
        /// </summary>
        /// <returns><C# code for the business layer, that deletes the specified
        /// entity from the table. It assumes that the TableName is plural.</returns>
        private string _GenerateDelete() =>
            "\n\n" + $@"public static bool Delete{EntityName}(int {EntityName}ID) => cls{EntityName}Data.Delete{EntityName}({EntityName}ID);";

        /// <summary>
        /// Generates class CRUD code for business layer.
        /// </summary>
        /// <returns>C# class CRUD code.</returns>
        public async Task<string> GenerateCRUDCodeForClass()
        {
            StringBuilder result = new StringBuilder();
            result.Append(_GenerateImportsAndClassName());
            result.Append(_GenerateProperties(TableName));
            result.Append(_GeneratePublicConstructor());
            result.Append(_GeneratePrivateConstructor());
            result.Append(_GenerateGetAll());
            result.Append(await _GenerateAddNewAsync());
            result.Append(await _GenerateUpdateAsync());
            result.Append(_GenerateSave());
            result.Append(_GenerateDelete());
            return result.Append("\n}").ToString();
        }
    }
}