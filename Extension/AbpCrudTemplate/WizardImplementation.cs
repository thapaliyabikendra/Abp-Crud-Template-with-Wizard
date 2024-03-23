using AbpCrudTemplate.Constants;
using AbpCrudTemplate.Models;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Project = AbpCrudTemplate.Constants.Project;

namespace AbpCrudTemplate
{
    public class WizardImplementation : IWizard
    {
        private UserInputForm _inputForm;
        private bool _addProjectItem = true;
        private ItemTemplate _itemTemplate;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
            try
            {
                // Move Model.cs and ModelAppService.cs files
                MoveFiles();

                // Migrations (Add migrations and Update database)
                if (_inputForm.AddMigration)
                {
                    ExecuteMigrationCommands();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MoveFiles()
        {
            // Move AppService from ApplicationContracts to Application
            MoveFile($"{_itemTemplate.SafeItemName}AppService.cs", Project.ApplicationContracts, Project.Application);

            // Move Model from ApplicationContracts to Domain
            MoveFile($"{_itemTemplate.SafeItemName}.cs", Project.ApplicationContracts, Project.Domain);
        }

        private void MoveFile(string fileName, string sourceDirectory, string destinationDirectory)
        {
            var sourceFilePath = Path.Combine(_itemTemplate.SolutionDirectorySubPath, sourceDirectory, _itemTemplate.PluralEntityName, fileName);
            var destinationFilePath = Path.Combine(_itemTemplate.SolutionDirectorySubPath, destinationDirectory, _itemTemplate.PluralEntityName, fileName);

            if (File.Exists(sourceFilePath))
            {
                string directoryPath = Path.GetDirectoryName(destinationFilePath);
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                if (!File.Exists(destinationFilePath))
                    File.Move(sourceFilePath, destinationFilePath);
            }
        }

        private void ExecuteMigrationCommands()
        {
            string migrationCommand = $"dotnet ef migrations add \"added {_itemTemplate.PluralEntityName}\" --context {_itemTemplate.AppName}DbContext --project src/{_itemTemplate.RootNamespace}.EntityFrameworkCore --startup-project src/{_itemTemplate.RootNamespace}.DbMigrator";
            string updateCommand = $"dotnet ef database update --context {_itemTemplate.AppName}DbContext --project src/{_itemTemplate.RootNamespace}.EntityFrameworkCore --startup-project src/{_itemTemplate.RootNamespace}.DbMigrator";

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.RedirectStandardInput = true;
                //process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                process.StandardInput.WriteLine(migrationCommand);
                process.StandardInput.Flush();

                if (_inputForm.UpdateDatabase)
                {
                    process.StandardInput.WriteLine(updateCommand);
                    process.StandardInput.Flush();
                }

                process.StandardInput.Close();
                process.WaitForExit();
                //string output = process.StandardOutput.ReadToEnd();
            }
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                // Display a form to the user. The form collects input for the custom message.
                _inputForm = new UserInputForm();
                if (_inputForm.ShowDialog() != DialogResult.OK)
                {
                    _addProjectItem = false;
                    return;
                }

                InitializeItemTemplate(replacementsDictionary);
                UpdateDictionaries(replacementsDictionary);

                var properties = new StringBuilder();
                var createUpdateProperties = new StringBuilder();
                var dtoProperties = new StringBuilder();
                var filterProperties = new StringBuilder();

                var ctorPropParam = new StringBuilder();
                var ctorPropParamAssign = new StringBuilder();
                var getListDtoSelect = new StringBuilder();
                var getListFilterCondition = new StringBuilder();
                var getListOrderBy = "";

                // Properties input format: propertyName:propertyType[:isRequired]
                if (!string.IsNullOrWhiteSpace(_inputForm.Properties))
                {
                    var propertiesData = _inputForm.Properties.Split(',');
                    foreach (var property in propertiesData)
                    {
                        var prop = property.Split(':');
                        var propName = prop[0].Trim();
                        var propType = prop[1].Trim();
                        var isRequired = prop.Length > 2 && prop[2].Equals("R", StringComparison.OrdinalIgnoreCase);

                        var propNameCamelCase = ToCamelCase(propName);

                        properties.AppendLine($"\tpublic {propType} {propName} {{ get; set; }}");
                        if (isRequired)
                        {
                            createUpdateProperties.AppendLine("\t[Required]");
                        }
                        createUpdateProperties.AppendLine($"\tpublic {propType} {propName} {{ get; set; }}");
                        dtoProperties.AppendLine($"\tpublic {propType} {propName} {{ get; set; }}");

                        ctorPropParam.Append($", {propType} {propNameCamelCase}");
                        ctorPropParamAssign.AppendLine($"\t\t{propName} = {propNameCamelCase};");

                        if (string.IsNullOrWhiteSpace(getListOrderBy))
                        {
                            getListOrderBy = propName;
                        }

                        getListDtoSelect.AppendLine($"\t\t\t\t\t\t\t\t{propName} = s.{propName},");

                        if (propType == "string")
                        {
                            getListFilterCondition.AppendLine($"\t\t\t\t\t\t\t\t.WhereIf(!string.IsNullOrEmpty(filter.{propName}), s => s.{propName}.Contains(filter.{propName}, StringComparison.OrdinalIgnoreCase))");
                        }
                        else
                        {
                            getListFilterCondition.AppendLine($"\t\t\t\t\t\t\t\t.WhereIf(filter.{propName} != null, s => s.{propName} == filter.{propName})");
                            if (!propType.Contains("?"))
                            {
                                propType += "?";
                            }
                        }
                        filterProperties.AppendLine($"\tpublic {propType} {propName} {{ get; set; }}");
                    }
                }

                if (string.IsNullOrWhiteSpace(getListOrderBy))
                {
                    getListOrderBy = "Name";
                }

                // Add custom parameters.
                replacementsDictionary["$properties$"] = properties.ToString();
                replacementsDictionary["$createupdateproperties$"] = createUpdateProperties.ToString();
                replacementsDictionary["$dtoproperties$"] = dtoProperties.ToString();
                replacementsDictionary["$filterproperties$"] = filterProperties.ToString();
                replacementsDictionary["$ctorpropparam$"] = ctorPropParam.ToString();
                replacementsDictionary["$ctorpropparamassign$"] = ctorPropParamAssign.ToString();
                replacementsDictionary["$getlistorderby$"] = getListOrderBy;
                replacementsDictionary["$getlistdtoselect$"] = getListDtoSelect.ToString();
                replacementsDictionary["$getlistfiltercondition$"] = getListFilterCondition.ToString();

                UpdateDbContext(_itemTemplate);
                UpdateAutoMapperProfile(_itemTemplate);
                UpdatePermissions(_itemTemplate, replacementsDictionary);
                UpdatePermissionDefinition(_itemTemplate);
                UpdateLocalization(_itemTemplate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void InitializeItemTemplate(Dictionary<string, string> replacementsDictionary)
        {
            var safeItemName = replacementsDictionary["$safeitemname$"];
            _itemTemplate = new ItemTemplate
            {
                SolutionDirectory = replacementsDictionary["$solutiondirectory$"],
                RootNamespace = replacementsDictionary["$rootnamespace$"],
                AppName = _inputForm.AppName,
                AppNameCamelCase = ToCamelCase(_inputForm.AppName),
                SafeItemName = safeItemName,
                PluralEntityName = string.IsNullOrWhiteSpace(_inputForm.PluralEntityName) ? $"{safeItemName}s" : _inputForm.PluralEntityName,
                EntityCamelCase = ToCamelCase(safeItemName),
                PluralEntityCamelCase = ToCamelCase(_inputForm.PluralEntityName)
            };
        }

        private void UpdateDictionaries(Dictionary<string, string> replacementsDictionary)
        {
            replacementsDictionary["$appname$"] = _itemTemplate.AppName;
            replacementsDictionary["$appnamecamelcase$"] = _itemTemplate.AppNameCamelCase;
            replacementsDictionary["$entitycamelcase$"] = _itemTemplate.EntityCamelCase;
            replacementsDictionary["$pluralentityname$"] = _itemTemplate.PluralEntityName;
            replacementsDictionary["$pluralcamelcaseentityname$"] = ToCamelCase(_itemTemplate.PluralEntityName);
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return _addProjectItem;
        }

        private void UpdateDbContext(ItemTemplate _itemTemplate)
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetDbContextPath(_itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);

                // Import namespace
                var nameSpace = $"\nnamespace {_itemTemplate.RootNamespace}.EntityFrameworkCore;";
                var importNamespace = $"using {_itemTemplate.RootNamespace}.{_itemTemplate.PluralEntityName};\r" + nameSpace;
                fileText = fileText.Replace(nameSpace, importNamespace);

                // Add dbset
                var positionText = "#region App Entities";
                var newText = positionText + $"\r\n\tpublic DbSet<{_itemTemplate.SafeItemName}> {_itemTemplate.PluralEntityName} {{ get; set;}} \r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdateAutoMapperProfile(ItemTemplate _itemTemplate)
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetAutoMapperProfilePath(_itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);

                // Import namespace
                var nameSpace = $"\nnamespace {_itemTemplate.RootNamespace};";
                var importNamespace = $"using {_itemTemplate.RootNamespace}.{_itemTemplate.PluralEntityName};\r\n" + nameSpace;
                fileText = fileText.Replace(nameSpace, importNamespace);

                var positionText = "* into multiple profile classes for a better organization. */";
                var newText = positionText
                    + $"\r\n\t\tCreateMap<CreateUpdate{_itemTemplate.SafeItemName}Dto, {_itemTemplate.SafeItemName}>();"
                    + $"\r\n\t\tCreateMap<{_itemTemplate.SafeItemName}, {_itemTemplate.SafeItemName}Dto>();\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdatePermissions(ItemTemplate _itemTemplate, Dictionary<string, string> replacementsDictionary)
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionPath(_itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "//public const string MyPermission1 = GroupName + \".MyPermission1\";";
                var newText = positionText
                    + $"\r\n    public static class {_itemTemplate.PluralEntityName}\r\n"
                    + "    {\r\n"
                    + $"        public const string Default = GroupName + \".{_itemTemplate.PluralEntityName}\";\r\n"
                    + $"        public const string Create = Default + \".Create\";\r\n"
                    + $"        public const string Edit = Default + \".Edit\";\r\n"
                    + $"        public const string Delete = Default + \".Delete\";\r\n"
                    + "    }\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdatePermissionDefinition(ItemTemplate _itemTemplate)
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionDefinitionPath(_itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = $"//myGroup.AddPermission({_itemTemplate.AppName}Permissions.MyPermission1, L(\"Permission:MyPermission1\"));";
                var newText = positionText
                      + $"\r\n\t\tvar {_itemTemplate.PluralEntityCamelCase}Permission = {_itemTemplate.AppNameCamelCase}Group.AddPermission({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Default, L(\"Permission:{_itemTemplate.PluralEntityName}\"));\r\n"
                      + $"        {_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Create, L(\"Permission:{_itemTemplate.PluralEntityName}.Create\"));\r\n"
                      + $"        {_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Edit, L(\"Permission:{_itemTemplate.PluralEntityName}.Edit\"));\r\n"
                      + $"        {_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Delete, L(\"Permission:{_itemTemplate.PluralEntityName}.Delete\"));\r";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdateLocalization(ItemTemplate _itemTemplate)
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetLocalizationPath(_itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "\"Welcome\": \"Welcome\",";
                var newText = positionText
                    + $"\r\n    \"Permission:{_itemTemplate.PluralEntityName}\": \"{_itemTemplate.PluralEntityName}\",\r\n"
                    + $"    \"Permission:{_itemTemplate.PluralEntityName}.Create\": \"Create\",\r\n"
                    + $"    \"Permission:{_itemTemplate.PluralEntityName}.Edit\": \"Edit\",\r\n"
                    + $"    \"Permission:{_itemTemplate.PluralEntityName}.Delete\": \"Delete\",\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private string ToCamelCase(string name)
        {
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
