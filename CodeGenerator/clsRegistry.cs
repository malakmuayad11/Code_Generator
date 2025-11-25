using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Logger;

namespace CodeGenerator
{
    public static class clsRegistry
    {
        internal static string KeyPath = @"HKEY_CURRENT_USER\SOFTWARE\CodeGenerator";
        public async static Task WriteInRegistry(string ValueName, string ValueData)
        {
            try
            {
                Registry.SetValue(KeyPath, ValueName, ValueData, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                await clsLogger.Log(ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public async static Task<string> ReadFromRegistry(string ValueName)
        {
            try
            {
                return Registry.GetValue(KeyPath, ValueName, null) as string;
            }
            catch (Exception ex)
            {
                await clsLogger.Log(ex.Message, EventLogEntryType.Error);
            }
            return null;
        }
    }
}