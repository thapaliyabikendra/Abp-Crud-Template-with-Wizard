using AbpCrudTemplate.Constants;
using AbpCrudTemplate.Models;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AbpCrudTemplate
{
    public class WizardImplementation : IWizard
    {
        private UserInputForm _inputForm;
        private bool _addProjectItem = true;

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

                var safeItemName = replacementsDictionary["$safeitemname$"];
                if (string.IsNullOrWhiteSpace(_inputForm.PluralEntityName))
                {
                    _inputForm.PluralEntityName = $"{safeItemName}s";
                }

                var itemTemplate = new ItemTemplate
                {
                    SolutionDirectory = replacementsDictionary["$solutiondirectory$"],
                    RootNamespace = replacementsDictionary["$rootnamespace$"],
                    AppName = _inputForm.AppName,
                    AppNameCamelCase = ToCamelCase(_inputForm.AppName),
                    SafeItemName = safeItemName,
                    PluralEntityName = _inputForm.PluralEntityName,
                    EntityCamelCase = ToCamelCase(safeItemName)
                };

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
                        var propName = prop[0];
                        var propType = prop[1];
                        var isRequired = prop.Length > 2 && prop[2].Equals("R", StringComparison.OrdinalIgnoreCase);

                        var propNameCamelCase = ToCamelCase(propName);

                        properties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                        if (isRequired)
                        {
                            createUpdateProperties.AppendLine("   [Required]");
                        }
                        createUpdateProperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                        dtoProperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");

                        ctorPropParam.Append($", {propType} {propNameCamelCase}");
                        ctorPropParamAssign.AppendLine($"        {propName} = {propNameCamelCase};");

                        if (string.IsNullOrWhiteSpace(getListOrderBy))
                        {
                            getListOrderBy = propName;
                        }

                        getListDtoSelect.AppendLine($"                                 {propName} = s.{propName},");

                        if (propType == "string")
                        {
                            getListFilterCondition.AppendLine($"                               .WhereIf(!string.IsNullOrEmpty(filter.{propName}), s => s.{propName}.ToLower().Contains(filter.{propName}.ToLower()))");
                        }
                        else
                        {
                            getListFilterCondition.AppendLine($"                               .WhereIf(filter.{propName} != null, s => s.{propName} == filter.{propName})");
                            if (!propType.Contains("?"))
                            {
                                propType += "?";
                            }
                        }
                        filterProperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                    }
                }

                if (string.IsNullOrWhiteSpace(getListOrderBy))
                {
                    getListOrderBy = "Name";
                }

                // Add custom parameters.
                replacementsDictionary["appname"] = itemTemplate.AppName;
                replacementsDictionary["appnamecamelcase"] = itemTemplate.AppNameCamelCase;
                replacementsDictionary["$entitycamelcase$"] = itemTemplate.EntityCamelCase;
                replacementsDictionary["$pluralentityname$"] = _inputForm.PluralEntityName;
                replacementsDictionary["pluralcamelcaseentityname"] = ToCamelCase(_inputForm.PluralEntityName);
                replacementsDictionary["$properties$"] = properties.ToString();
                replacementsDictionary["$createupdateproperties$"] = createUpdateProperties.ToString();
                replacementsDictionary["$dtoproperties$"] = dtoProperties.ToString();
                replacementsDictionary["$filterproperties$"] = filterProperties.ToString();
                replacementsDictionary["$ctorpropparam$"] = ctorPropParam.ToString();
                replacementsDictionary["$ctorpropparamassign$"] = ctorPropParamAssign.ToString();
                replacementsDictionary["$getlistorderby$"] = getListOrderBy;
                replacementsDictionary["$getlistdtoselect$"] = getListDtoSelect.ToString();
                replacementsDictionary["$getlistfiltercondition$"] = getListFilterCondition.ToString();

                UpdateDbContext(itemTemplate);
                UpdateAutoMapperProfile(itemTemplate);
                UpdatePermissions(itemTemplate, replacementsDictionary);
                UpdatePermissionDefinition(itemTemplate);
                UpdateLocalization(itemTemplate);
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

        private void UpdateDbContext(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetDbContextPath(itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);

                // Import namespace
                var nameSpace = $"namespace {itemTemplate.RootNamespace}.EntityFrameworkCore;";
                var importNamespace = $"using {itemTemplate.RootNamespace}.{itemTemplate.PluralEntityName};\n" + nameSpace;
                fileText = fileText.Replace(nameSpace, importNamespace);

                // Add dbset
                var positionText = "#region App Entities";
                var newText = positionText + $"\r\n\rpublic DbSet<{itemTemplate.SafeItemName}> {itemTemplate.PluralEntityName} {{ get; set;}} \r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdateAutoMapperProfile(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetAutoMapperProfilePath(itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);

                // Import namespace
                var nameSpace = $"namespace {itemTemplate.RootNamespace};";
                var importNamespace = $"using {itemTemplate.RootNamespace}.{itemTemplate.PluralEntityName};\n" + nameSpace;
                fileText = fileText.Replace(nameSpace, importNamespace);

                var positionText = "* into multiple profile classes for a better organization. */";
                var newText = positionText
                    + $"\r\n\r\rCreateMap<CreateUpdate{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}>();"
                    + $"\r\n\r\rCreateMap<{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}Dto>();\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdatePermissions(ItemTemplate itemTemplate, Dictionary<string, string> replacementsDictionary)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionPath(itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "//public const string MyPermission1 = GroupName + \".MyPermission1\";";
                var newText = positionText
                    + $"\r\n\r\n    public static class {itemTemplate.PluralEntityName}\r\n"
                    + "    {\r\n"
                    + $"        public const string Default = GroupName + \".{itemTemplate.PluralEntityName}\";\r\n"
                    + $"        public const string Create = Default + \".Create\";\r\n"
                    + $"        public const string Edit = Default + \".Edit\";\r\n"
                    + $"        public const string Delete = Default + \".Delete\";\r\n"
                    + "    }\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdatePermissionDefinition(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetPermissionDefinitionPath(itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = $"//myGroup.AddPermission({itemTemplate.AppName}Permissions.MyPermission1, L(\"Permission:MyPermission1\"));";
                var newText = positionText
                      + $"\r\n\n\r\rvar {itemTemplate.EntityCamelCase}Permission = {itemTemplate.AppNameCamelCase}Group.AddPermission({itemTemplate.AppName}Permissions.{itemTemplate.PluralEntityName}.Default, L(\"Permission:{itemTemplate.PluralEntityName}\"));\r\n"
                      + $"        {itemTemplate.EntityCamelCase}Permission.AddChild({itemTemplate.AppName}Permissions.{itemTemplate.SafeItemName}.Create, L(\"Permission:{itemTemplate.PluralEntityName}.Create\"));\r\n"
                      + $"        {itemTemplate.EntityCamelCase}Permission.AddChild({itemTemplate.AppName}Permissions.{itemTemplate.SafeItemName}.Edit, L(\"Permission:{itemTemplate.PluralEntityName}.Edit\"));\r\n"
                      + $"        {itemTemplate.EntityCamelCase}Permission.AddChild({itemTemplate.AppName}Permissions.{itemTemplate.SafeItemName}.Delete, L(\"Permission:{itemTemplate.PluralEntityName}.Delete\"));\r";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdateLocalization(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetLocalizationPath(itemTemplate.AppName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "\"Welcome\": \"Welcome\",";
                var newText = positionText
                    + $"\r\n    \"Permission:{itemTemplate.PluralEntityName}\": \"{itemTemplate.PluralEntityName}\",\r\n"
                    + $"    \"Permission:{itemTemplate.PluralEntityName}.Create\": \"Create\",\r\n"
                    + $"    \"Permission:{itemTemplate.PluralEntityName}.Edit\": \"Edit\",\r\n"
                    + $"    \"Permission:{itemTemplate.PluralEntityName}.Delete\": \"Delete,\"\r\n";
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
