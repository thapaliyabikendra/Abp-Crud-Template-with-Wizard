using System.IO;

namespace AbpCrudTemplate.Models
{
    public class ItemTemplate
    {
        public string SolutionDirectory { get; set; }
        public string ProjectDirectory { get { return Directory.GetParent(Directory.GetParent(SolutionDirectory)?.FullName)?.FullName; } }

        public string SolutionSourceDirectory
        {
            get { return Path.Combine(SolutionDirectory, "src"); }
        }
        public string RootNamespace { get; set; }

        public string SolutionDirectorySubPath
        {
            get
            {
                return Path.Combine(this.SolutionSourceDirectory, this.RootNamespace);
            }
        }

        public string AppName { get; set; }
        public string AppNameCamelCase { get; set; }
        public string SafeItemName { get; set; }
        public string EntityCamelCase { get; set; }
        public string PluralEntityName { get; set; }
        public string PluralEntityCamelCase { get; set; }
        public string TempFolderName { get; set; }
        public string ProjectName { get; set; }
    }
}
