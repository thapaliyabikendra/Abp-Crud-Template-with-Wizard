using System.IO;

namespace AbpCrudTemplate.Constants
{
    public static class FilePath
    {
        public static string GetDbContextPath(string serviceName)
        {
            return Path.Combine(Project.EntityFrameworkCore, "EntityFrameworkCore", $"{serviceName}DbContext.cs");
        }
        public static string GetAutoMapperProfilePath(string serviceName)
        {
            return Path.Combine(Project.Application, $"{serviceName}ApplicationAutoMapperProfile.cs");
        }
    }
}
