﻿using System.IO;

namespace AbpCrudTemplate.Models
{
    public class ItemTemplate
    {
        private string _solutionDirectory;

        public string SolutionDirectory
        {
            get { return Path.Combine(_solutionDirectory, "src"); }
            set { _solutionDirectory = value; }
        }

        public string SolutionDirectorySubPath
        {
            get
            {
                return Path.Combine(this.SolutionDirectory, this.RootNamespace);
            }
        }
        public string RootNamespace { get; set; }
        public string SafeItemName { get; set; }
        public string ServiceName { get; set; }
    }
}
