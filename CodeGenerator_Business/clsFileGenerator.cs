using System.IO;

namespace CodeGenerator_Business
{
    public class clsFileGenerator : IFileGenerator
    {
        public string TableName { get; set; }
        public string ProjectName { get; set; }
        public string DataLayerClass { get; set; }
        public string BusinessLayerClass { get; set; }

        private string _BusinessLayerFolder;

        private string _DataLayerFolder;

        public clsFileGenerator(string tableName, string projectName)
        {
            TableName = tableName;
            ProjectName = projectName;
            _BusinessLayerFolder = $"C:/{ProjectName}/{ProjectName}_Business";
            _DataLayerFolder = $"C:/{ProjectName}/{ProjectName}_Data";
        }

        /// <summary>
        /// Creates business and data access layers folders for the project.
        /// </summary>
        public void GenerateLayersFolders()
        {
            // Create a folder for business layer of the project
            if (!Directory.Exists(_BusinessLayerFolder))
                Directory.CreateDirectory(_BusinessLayerFolder);

            // Create a folder for data access layer of the project
            if (!Directory.Exists(_DataLayerFolder))
                Directory.CreateDirectory(_DataLayerFolder);
        }

        /// <summary>
        /// Creates data access and business layer classes for the specfied table (class).
        /// </summary>
        public void GenerateClassFiles()
        {
            DataLayerClass = Path.Combine(_DataLayerFolder, $"cls{TableName.TrimEnd('s')}Data.cs");
            BusinessLayerClass = Path.Combine(_BusinessLayerFolder, $"cls{TableName.TrimEnd('s')}.cs");

            if (!File.Exists(DataLayerClass))
                File.Create(DataLayerClass).Dispose();

            if (!File.Exists(BusinessLayerClass))
                File.Create(BusinessLayerClass).Dispose();
        }
    }
}