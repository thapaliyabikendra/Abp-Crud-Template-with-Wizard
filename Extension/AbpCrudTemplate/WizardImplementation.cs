using AbpCrudTemplate.Constants;
using AbpCrudTemplate.Models;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        #region Public Methods

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
                if (_addProjectItem)
                {
                    // Move Model.cs and ModelAppService.cs files
                    MoveFiles();

                    // Migrations (Add migrations and Update database)
                    if (_inputForm.AddMigration)
                    {
                        ExecuteMigrationCommands();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

                UpdateDbContext();
                UpdateAutoMapperProfile();
                UpdatePermissions();
                UpdatePermissionDefinition();
                UpdateLocalization();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public bool ShouldAddProjectItem(string filePath)
        {
            return _addProjectItem;
        }
        #endregion

        #region Private Methods
        private void MoveFiles()
        {
            // Move AppService from ApplicationContracts to Application
            MoveFile($"{_itemTemplate.SafeItemName}AppService.cs", Project.ApplicationContracts, Project.Application);

            // Move Model from ApplicationContracts to Domain
            MoveFile($"{_itemTemplate.SafeItemName}.cs", Project.ApplicationContracts, Project.Domain);
        }
        private void MoveFile(string fileName, string sourceDirectory, string destinationDirectory)
        {
            var sourceFilePath = Path.Combine(_itemTemplate.SolutionDirectorySubPath + sourceDirectory, _itemTemplate.PluralEntityName, fileName);
            var destinationFilePath = Path.Combine(_itemTemplate.SolutionDirectorySubPath + destinationDirectory, _itemTemplate.PluralEntityName, fileName);

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
        private void InitializeItemTemplate(Dictionary<string, string> replacementsDictionary)
        {
            var safeItemName = replacementsDictionary["$safeitemname$"];
            var appName = replacementsDictionary["$defaultnamespace$"]?.Split('.')?.LastOrDefault();
            var pluralEntityName = string.IsNullOrWhiteSpace(_inputForm.PluralEntityName) ? $"{safeItemName}s" : _inputForm.PluralEntityName;

            _itemTemplate = new ItemTemplate
            {
                AppName = appName,
                AppNameCamelCase = ToCamelCase(appName),

                RootNamespace = replacementsDictionary["$rootnamespace$"],
                SolutionDirectory = replacementsDictionary["$solutiondirectory$"],

                SafeItemName = safeItemName,
                EntityCamelCase = ToCamelCase(safeItemName),
                PluralEntityName = pluralEntityName,
                PluralEntityCamelCase = ToCamelCase(pluralEntityName)
            };
        }
        private void UpdateDictionaries(Dictionary<string, string> replacementsDictionary)
        {
            replacementsDictionary["$appname$"] = _itemTemplate.AppName;
            replacementsDictionary["$appnamecamelcase$"] = _itemTemplate.AppNameCamelCase;
            replacementsDictionary["$entitycamelcase$"] = _itemTemplate.EntityCamelCase;
            replacementsDictionary["$pluralentityname$"] = _itemTemplate.PluralEntityName;
            replacementsDictionary["$pluralcamelcaseentityname$"] = _itemTemplate.PluralEntityCamelCase;
        }
        private void UpdateDbContext()
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetDbContextPath(_itemTemplate.AppName);
            UpdateFile(filePath, fileText =>
            {
                var newText = new StringBuilder();
                var importNamespace = new StringBuilder();
                var nameSpace = $@"namespace {_itemTemplate.RootNamespace}.EntityFrameworkCore;";
                var positionText = $@"public {_itemTemplate.AppName}DbContext(";

                importNamespace.AppendLine($"using {_itemTemplate.RootNamespace}.{_itemTemplate.PluralEntityName};")
                               .Append(nameSpace);
                newText.AppendLine($"public DbSet<{_itemTemplate.SafeItemName}> {_itemTemplate.PluralEntityName} {{ get; set;}}")
                       .Append("\t").Append(positionText);

                return fileText.Replace(nameSpace, importNamespace.ToString()).Replace(positionText, newText.ToString());
            });

        }
        private void UpdateAutoMapperProfile()
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetAutoMapperProfilePath(_itemTemplate.AppName);
            UpdateFile(filePath, fileText =>
            {
                var newText = new StringBuilder();
                var importNamespace = new StringBuilder();
                var nameSpace = $@"namespace {_itemTemplate.RootNamespace};";
                var positionText = @"* into multiple profile classes for a better organization. */";

                importNamespace.AppendLine($"using {_itemTemplate.RootNamespace}.{_itemTemplate.PluralEntityName};")
                               .Append(nameSpace);
                newText.AppendLine(positionText)
                       .Append("\t\t").AppendLine($"CreateMap<CreateUpdate{_itemTemplate.SafeItemName}Dto, {_itemTemplate.SafeItemName}>();")
                       .Append("\t\t").Append($"CreateMap<{_itemTemplate.SafeItemName}, {_itemTemplate.SafeItemName}Dto>();");

                return fileText.Replace(nameSpace, importNamespace.ToString()).Replace(positionText, newText.ToString());
            });
        }
        private void UpdatePermissions()
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionPath(_itemTemplate.AppName);
            UpdateFile(filePath, fileText =>
            {
                var updatedText = new StringBuilder();
                var positionText = @"//public const string MyPermission1 = GroupName + "".MyPermission1"";";

                updatedText.AppendLine(positionText)
                           .Append("\t").Append($@"public static class {_itemTemplate.PluralEntityName}")
                           .Append("\t").AppendLine("{")
                           .Append("\t\t").AppendLine($@"public const string Default = GroupName + "".{_itemTemplate.PluralEntityName}"";")
                           .Append("\t\t").AppendLine($@"public const string Create = Default + "".Create"";")
                           .Append("\t\t").AppendLine($@"public const string Edit = Default + "".Edit"";")
                           .Append("\t\t").AppendLine($@"public const string Delete = Default + "".Delete"";")
                           .Append("\t").Append("}");

                return fileText.ToString().Replace(positionText, updatedText.ToString());
            });
        }
        private void UpdatePermissionDefinition()
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionDefinitionPath(_itemTemplate.AppName);

            UpdateFile(filePath, fileText =>
            {
                var updatedText = new StringBuilder();
                var positionText = $@"//myGroup.AddPermission({_itemTemplate.AppName}Permissions.MyPermission1, L(""Permission:MyPermission1""));";

                updatedText.AppendLine(positionText)
                           .Append("\t\t").AppendLine($@"var {_itemTemplate.PluralEntityCamelCase}Permission = {_itemTemplate.AppNameCamelCase}Group.AddPermission({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Default, L(""Permission:{_itemTemplate.PluralEntityName}""));")
                           .Append("\t\t").AppendLine($@"{_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Create, L(""Permission:{_itemTemplate.PluralEntityName}.Create""));")
                           .Append("\t\t").AppendLine($@"{_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Edit, L(""Permission:{_itemTemplate.PluralEntityName}.Edit""));")
                           .Append("\t\t").Append($@"{_itemTemplate.PluralEntityCamelCase}Permission.AddChild({_itemTemplate.AppName}Permissions.{_itemTemplate.PluralEntityName}.Delete, L(""Permission:{_itemTemplate.PluralEntityName}.Delete""));");

                return fileText.ToString().Replace(positionText, updatedText.ToString());
            });
        }
        private void UpdateLocalization()
        {
            var filePath = _itemTemplate.SolutionDirectorySubPath + FilePath.GetLocalizationPath(_itemTemplate.AppName);

            UpdateFile(filePath, fileText =>
            {
                var updatedText = new StringBuilder();
                var positionText = @"""Welcome"": ""Welcome"",";

                updatedText.AppendLine(positionText)
                           .Append("\t\t").AppendLine($@"""Permission:{_itemTemplate.PluralEntityName}"": ""{_itemTemplate.PluralEntityName}"",")
                           .Append("\t\t").AppendLine($@"""Permission:{_itemTemplate.PluralEntityName}.Create"": ""Create"",")
                           .Append("\t\t").AppendLine($@"""Permission:{_itemTemplate.PluralEntityName}.Edit"": ""Edit"",")
                           .Append("\t\t").Append($@"""Permission:{_itemTemplate.PluralEntityName}.Delete"": ""Delete"",");

                return fileText.ToString().Replace(positionText, updatedText.ToString());
            });
        }
        private void UpdateFile(string filePath, Func<string, string> updateAction)
        {
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                fileText = updateAction(fileText);
                File.WriteAllText(filePath, fileText);
            }
        }
        private string ToCamelCase(string name)
        {
            return (name.Length < 1) ? "" : Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
        #endregion
    }
}
