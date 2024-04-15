using System.IO;

namespace AbpCrudTemplate.Wizard.Constants
{
    public static class FilePath
    {
        public static string GetDbContextPath(string appName)
        {
            return Path.Combine(Project.EntityFrameworkCore, "EntityFrameworkCore", $"{appName}DbContext.cs");
        }
        public static string GetAutoMapperProfilePath(string appName)
        {
            return Path.Combine(Project.Application, $"{appName}ApplicationAutoMapperProfile.cs");
        }
        public static string GetPermissionPath(string appName)
        {
            return Path.Combine(Project.ApplicationContracts, "Permissions", $"{appName}Permissions.cs");
        }
        public static string GetPermissionDefinitionPath(string appName)
        {
            return Path.Combine(Project.ApplicationContracts, "Permissions", $"{appName}PermissionDefinitionProvider.cs");
        }
        public static string GetLocalizationPath(string appName)
        {
            return Path.Combine(Project.DomainShared, "Localization", appName, $"en.json");
        }
    }
}
