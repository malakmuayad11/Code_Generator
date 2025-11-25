using System.Data;
using System.Threading.Tasks;
using CodeGenerator_Data;

namespace CodeGenerator_Business
{
    public class clsParamater
    {
        /// <summary>
        /// Gets all parameters of a stored procedure.
        /// </summary>
        /// <param name="StoredProcedureName">The name of the stored procedure to get its parameters.</param>
        /// <returns>Parameters of a stored procedure.</returns>
        public static async Task<DataTable> GetParametersForStoredProcedure(string StoredProcedureName) =>
          await clsParamaterData.GetParametersForStoredProcedure(StoredProcedureName);
    }
}