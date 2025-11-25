using CodeGenerator_Data;
using System.Data;
using System.Threading.Tasks;

namespace CodeGenerator_Business
{
    public class clsColumn
    {
        /// <summary>
        /// Get all columns for a table in the database.
        /// </summary>
        /// <param name="TableName">The table's name to get its columns</param>
        /// <returns>Columns for a table in the database.</returns>
        public static async Task<DataTable> GetColumnsForTable(string TableName) =>
            await clsColumnData.GetColumns(TableName);
    }
}