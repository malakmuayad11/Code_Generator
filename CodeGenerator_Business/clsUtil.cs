namespace CodeGenerator_Business
{
    public static class clsUtil
    {
        /// <summary>
        /// Maps SQL data types to their corresponding C# data types.
        /// </summary>
        /// <param name="SqlType">SQL data type to map it to C# data type.</param>
        /// <returns>C# data type corresponds to the appropriate SQL data type.</returns>
        public static string MapSqlTypeToCSharpType(string SqlType)
        {
            switch (SqlType)
            {
                case "int":
                    return "int";
                case "bigint":
                    return "long";
                case "smallint":
                    return "short";
                case "tinyint":
                    return "byte";
                case "bit":
                    return "bool";
                case "float":
                    return "double";
                case "real":
                    return "float";
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                    return "decimal";
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    return "string";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "DateTime";
                case "time":
                    return "TimeSpan";
                case "uniqueidentifier":
                    return "Guid";
                default:
                    return "object";
            }

        }

        /// <summary>
        /// Gets the default value for a given C# data type.
        /// </summary>
        /// <param name="CSharpType">The C# data type to get its default value.</param>
        /// <returns>The default value for a C# data type.</returns>
        public static string GetDefaultValueForCSharpType(string CSharpType)
        {
            switch (CSharpType)
            {
                case "int":
                    return "0";
                case "long":
                    return "0L";
                case "short":
                    return "0";
                case "byte":
                    return "0";
                case "bool":
                    return "false";
                case "double":
                    return "0.0";
                case "float":
                    return "0.0f";
                case "decimal":
                    return "0.0m";
                case "string":
                    return "\"\"";
                case "DateTime":
                    return "DateTime.MinValue";
                case "TimeSpan":
                    return "TimeSpan.Zero";
                case "Guid":
                    return "Guid.Empty";
                default:
                    return "null";
            }
        }
    }
}