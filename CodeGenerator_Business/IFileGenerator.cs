namespace CodeGenerator_Business
{
    public interface IFileGenerator
    {
        string TableName { get; set; }
        string ProjectName { get; set; }
        string DataLayerClass { get; set; }
        string BusinessLayerClass { get; set; }
        void GenerateLayersFolders();
        void GenerateClassFiles();

    }
}