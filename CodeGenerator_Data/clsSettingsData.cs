using System.Configuration;

namespace CodeGenerator_Data
{
    public static class clsSettingsData
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
    }
}