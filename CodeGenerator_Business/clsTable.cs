using CodeGenerator_Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace CodeGenerator_Business
{
    public class clsTable
    {
        public string Name { get; }
        public List<clsColumn> Columns { get; }

        private IFileGenerator _FileGenerator;
        private IGenerator _GeneratorData;
        private IGeneratorBusiness _GeneratorBusiness;

        public clsTable(string ProjectName, string Name, IFileGenerator FileGenerator,
            IGenerator Generator, IGeneratorBusiness GeneratorBusiness)
        {
            this.Name = Name;
            _FileGenerator = FileGenerator;
            FileGenerator.GenerateLayersFolders();
            FileGenerator.GenerateClassFiles();
            _GeneratorData = Generator;
            _GeneratorBusiness = GeneratorBusiness;
        }

        /// <summary>
        /// Generates CRUD code for data access and business layer classes of the database table.
        /// </summary>
        public async Task GenerateCRUDCode()
        {
            File.AppendAllText(_FileGenerator.DataLayerClass, await _GeneratorData.GenerateCRUDCodeForClass());
            File.AppendAllText(_FileGenerator.BusinessLayerClass, await _GeneratorBusiness.GenerateCRUDCodeForClass());
        }

        /// <summary>
        /// Gets the list of tables for a specified database.
        /// </summary>
        /// <param name="DatabaseName">The name of the database to get its tables.</param>
        /// <returns>A list of tables for a specific database.</returns>
        public static async Task<DataTable> GetTablesForDatabase(string DatabaseName) =>
            await clsTableData.GetTablesForDatabase(DatabaseName);
    }
}