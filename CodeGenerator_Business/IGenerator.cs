using System.Threading.Tasks;

namespace CodeGenerator_Business
{
    public interface IGenerator
    {
        Task<string> GenerateCRUDCodeForClass();
        string TableName { get; set; }
        string ProjectName { get; set; }
    }
}