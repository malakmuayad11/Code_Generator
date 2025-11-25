namespace CodeGenerator_Business
{
    public interface IGeneratorBusiness : IGenerator
    {
        string EntityName { get; }
    }
}