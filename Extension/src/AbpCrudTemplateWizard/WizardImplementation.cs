using AbpCrudTemplate.Constants;
using AbpCrudTemplate.Models;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AbpCrudTemplate
{
    public class WizardImplementation : IWizard
    {
        private UserInputForm _inputForm;
        private bool _addProjectItem = true;
        private ItemTemplate _itemTemplate;
        private string[] _angularTemplateFiles = new string[] {
                    "model-routing.module.ts",
                    "model.component.scss",
                    "model.component.html",
                    "model.component.ts",
                    "model.module.ts"
                };
        private bool _shouldGenerateAngularFiles = false;

        #region Public Methods
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }
        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            _itemTemplate.ProjectName = projectItem.ContainingProject.Name;
        }
        public void RunFinished()
        {
            try
            {
                if (_addProjectItem)
                {
                    MoveFiles();

                    // Remove generated dir
                    RemoveGeneratedDir();

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

                if (string.IsNullOrWhiteSpace(_inputForm.Properties))
                {
                    MessageBox.Show("Enter Properties");
                    return;
                }

                InitializeItemTemplate(replacementsDictionary);
                UpdateDictionaries(replacementsDictionary);

                _shouldGenerateAngularFiles = _inputForm.GenerateAngularFiles;

                var properties = new StringBuilder();
                var createUpdateProperties = new StringBuilder();
                var dtoProperties = new StringBuilder();
                var filterProperties = new StringBuilder();

                var getListDtoSelect = new StringBuilder();
                var getListFilterCondition = new StringBuilder();
                var getListOrderBy = "";

                // angular
                var buildFormProperties = new StringBuilder();
                var saveFormProperties = new StringBuilder();
                var filterInputs = new StringBuilder();
                var gridColumns = new StringBuilder();
                var formInputs = new StringBuilder();

                // Properties input format: PropertyName:PropertyType[:IsRequired]
                // ex. DisplayName:string:R
                var propertiesData = _inputForm.Properties.Split(',');
                var isFirstItem = true;
                var requiredAttribute = "\t[Required]";
                foreach (var prop in propertiesData.Select((item, index) => (item, index)))
                {
                    if (string.IsNullOrWhiteSpace(prop.item) || !prop.item.Contains(':')) {
                        continue;
                    }

                    var propData = prop.item.Split(':');
                    var propName = propData[0]?.Trim();
                    var propType = propData[1]?.Trim();
                    var propNameCamelCase = ToCamelCase(propName);

                    if (propType is null)
                    {
                        continue;
                    }

                    var isRequired = propData.Length > 2 && propData[2].Equals("R", StringComparison.OrdinalIgnoreCase);
                    var isLastItem = (prop.index == (propertiesData.Count() - 1));

                    var property = $"\tpublic {propType} {propName} {{ get; set; }}";
                    var nullablePropType = (!propType.Equals("string") && !propType.Contains("?")) ? $"{propType}?" : propType;
                    var propertyFilter = $"\tpublic {nullablePropType} {propName} {{ get; set; }}";
                    var getListDtoSelectField = $"\t\t\t\t\t\t\t\t{propName} = s.{propName},";
                    var filterCondition = (propType == "string")
                       ? $"\t\t\t\t\t\t.WhereIf(!string.IsNullOrEmpty(filter.{propName}), s => s.{propName}.Contains(filter.{propName}, StringComparison.OrdinalIgnoreCase))"
                       : $"\t\t\t\t\t\t.WhereIf(filter.{propName} != null, s => s.{propName} == filter.{propName})";

                    if (string.IsNullOrWhiteSpace(getListOrderBy))
                    {
                        getListOrderBy = propName;
                    }

                    if (isFirstItem)
                    {
                        filterCondition = Environment.NewLine + filterCondition;
                    }

                    if (!isLastItem && !isFirstItem) {
                        filterCondition += Environment.NewLine;
                    }

                    properties.Append(property);
                    if (isRequired)
                    {
                        createUpdateProperties.Append(requiredAttribute + Environment.NewLine);
                    }
                    createUpdateProperties.Append(property);
                    dtoProperties.Append(property);
                    filterProperties.Append(propertyFilter);
                    getListDtoSelect.Append(getListDtoSelectField);
                    getListFilterCondition.Append(filterCondition);

                    if (_shouldGenerateAngularFiles) {
                        buildFormProperties.AppendLine($"\t\t\t{propNameCamelCase}: [this.selected.{propNameCamelCase}{(isRequired ? ", Validators.required" : "")}],");
                        saveFormProperties.AppendLine($"\t\t\t{propNameCamelCase}: this.form.get(\"{propNameCamelCase}\").value,");

                        var filterInput = $"\t\t\t<div class=\"col-md-2 form-group mb-1\">\r\n" +
                                          $"\t\t\t\t<label for=\"filter-{propNameCamelCase}\">{{ '::{propName}' | abpLocalization }}</label>\r\n" +
                                          $"\t\t\t\t<input type=\"text\" class=\"form-control\" [(ngModel)]=\"filter.{propNameCamelCase}\" id=\"filter-{propNameCamelCase}\" />\r\n" +
                                          $"\t\t\t</div>";
                        var gridColumn = $"\t\t\t<ngx-datatable-column [name]=\"'::{propName}' | abpLocalization\" prop=\"{propNameCamelCase}\"></ngx-datatable-column>";
                        var formInput = $"<div class=\"form-group\">\r\n" +
                            $"\t\t\t\t<label for=\"{propNameCamelCase}\">{{'::{propName}' | abpLocalization}}</label>{(isRequired ? "<span> * </span>" : "")}\r\n" +
                            $"\t\t\t\t<input type=\"text\" id=\"{propNameCamelCase}\" class=\"form-control\" formControlName=\"{propNameCamelCase}\" />\r\n" +
                            $"\t\t\t</div>";

                        filterInputs.AppendLine(filterInput);
                        gridColumns.AppendLine(gridColumn);
                        formInputs.AppendLine(formInput);
                    }
                    isFirstItem = false;
                }

                // Add custom parameters.
                replacementsDictionary["$properties$"] = properties.ToString();
                replacementsDictionary["$createupdateproperties$"] = createUpdateProperties.ToString();
                replacementsDictionary["$dtoproperties$"] = dtoProperties.ToString();
                replacementsDictionary["$filterproperties$"] = filterProperties.ToString();

                replacementsDictionary["$getlistorderby$"] = getListOrderBy;
                replacementsDictionary["$getlistdtoselect$"] = getListDtoSelect.ToString();
                replacementsDictionary["$getlistfiltercondition$"] = getListFilterCondition.ToString();

                replacementsDictionary["$buildFormProperties$"] = buildFormProperties.ToString();
                replacementsDictionary["$saveFormProperties$"] = saveFormProperties.ToString();
                replacementsDictionary["$filterInputs$"] = filterInputs.ToString();
                replacementsDictionary["$gridColumns$"] = gridColumns.ToString();
                replacementsDictionary["$formInputs$"] = formInputs.ToString();

                UpdateDbContext();
                UpdateAutoMapperProfile();
                UpdatePermissions();
                UpdatePermissionDefinition();
                UpdateLocalization();

                UpdateUIAppRouting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public bool ShouldAddProjectItem(string filePath)
        {
            if (!_shouldGenerateAngularFiles && _angularTemplateFiles.Contains(filePath)) {
                return false;
            }

            return _addProjectItem;
        }
        #endregion

        #region Private Methods
        private void RemoveGeneratedDir()
        {
            var directoryPath = Path.Combine(_itemTemplate.SolutionSourceDirectory, _itemTemplate.ProjectName, _itemTemplate.TempFolderName);
            if (Directory.Exists(directoryPath))
            {
                bool hasFiles = Directory.GetFiles(directoryPath).Length > 0;
                if (!hasFiles)
                {
                    Directory.Delete(directoryPath);
                }
            }
        }
        private void MoveFiles()
        {
            MoveAPIFiles();
            MoveUIFiles();
        }
        private void MoveAPIFiles()
        {
            // Move to Domain
            MoveFile($"{_itemTemplate.SafeItemName}.cs", Constants.Project.Domain);

            // Move to Application Contracts
            MoveFile($"{_itemTemplate.SafeItemName}Dto.cs", Constants.Project.ApplicationContracts);
            MoveFile($"{_itemTemplate.SafeItemName}Filter.cs", Constants.Project.ApplicationContracts);
            MoveFile($"CreateUpdate{_itemTemplate.SafeItemName}Dto.cs", Constants.Project.ApplicationContracts);
            MoveFile($"I{_itemTemplate.SafeItemName}AppService.cs", Constants.Project.ApplicationContracts);

            // Move to Application
            MoveFile($"{_itemTemplate.SafeItemName}AppService.cs", Constants.Project.Application);
        }
        private void MoveUIFiles()
        {
            // Move to angular
            var angularAppPath = Path.Combine(Constants.Project.Angular, "src", "app");
            MoveFile($"{_itemTemplate.EntityCamelCase}-routing.module.ts", angularAppPath, isUI: true);
            MoveFile($"{_itemTemplate.EntityCamelCase}.component.scss", angularAppPath, isUI: true);
            MoveFile($"{_itemTemplate.EntityCamelCase}.component.html", angularAppPath, isUI: true);
            MoveFile($"{_itemTemplate.EntityCamelCase}.component.ts", angularAppPath, isUI: true);
            MoveFile($"{_itemTemplate.EntityCamelCase}.module.ts", angularAppPath, isUI: true);
        }
        private void MoveFile(string fileName, string destinationDirectory, bool isUI = false)
        {
            var sourceFilePath = Path.Combine(_itemTemplate.SolutionSourceDirectory, _itemTemplate.ProjectName, _itemTemplate.TempFolderName, fileName);
            var destinationFilePath = isUI ? Path.Combine(_itemTemplate.ProjectDirectory, destinationDirectory, fileName)
                                           : Path.Combine(_itemTemplate.SolutionDirectorySubPath + destinationDirectory, _itemTemplate.PluralEntityName, fileName);

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
            var appName = replacementsDictionary["$defaultnamespace$"]?.Split('.')?.LastOrDefault();
            var safeItemName = replacementsDictionary["$safeitemname$"];
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
                PluralEntityCamelCase = ToCamelCase(pluralEntityName),
                TempFolderName = replacementsDictionary["$guid1$"]
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
                       .Append("\t")
                       .Append(positionText);

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
        private void UpdateUIAppRouting()
        {
            var filePath = Path.Combine(_itemTemplate.ProjectDirectory, Constants.Project.Angular, "src", "app", $"app-routing.module.ts");

            UpdateFile(filePath, fileText =>
            {
                var updatedText = new StringBuilder();
                var positionText = @"];";

                updatedText.AppendLine(", {path: routeUrl." + _itemTemplate.SafeItemName.ToUpper() + ", loadChildren: () => import('./" + _itemTemplate.EntityCamelCase + "/" + _itemTemplate.EntityCamelCase + ".module').then(m => m." + _itemTemplate.SafeItemName + "Module)}")
                           .Append("\t")
                           .AppendLine(positionText);

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
