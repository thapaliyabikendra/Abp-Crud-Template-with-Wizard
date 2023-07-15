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

namespace AbpCrudTemplate
{
    public class WizardImplementation : IWizard
    {
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
        public UserInputForm _inputForm;
        public bool AddPItem { get; set; } = true;
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            try
            {
                // Display a form to the user. The form collects 
                // input for the custom message.
                _inputForm = new UserInputForm();
                if (_inputForm.ShowDialog() != DialogResult.OK)
                {
                    AddPItem = false;
                    return;
                }

                ItemTemplate itemTemplate = new ItemTemplate()
                {
                    SolutionDirectory = replacementsDictionary["$solutiondirectory$"],
                    RootNamespace = replacementsDictionary["$rootnamespace$"],
                    SafeItemName = replacementsDictionary["$safeitemname$"],
                    ServiceName = _inputForm.MicroServiceName,
                };

                StringBuilder properties = new StringBuilder();
                StringBuilder createupdateproperties = new StringBuilder();
                StringBuilder dtoproperties = new StringBuilder();
                StringBuilder filterproperties = new StringBuilder();

                StringBuilder ctorPropParam = new StringBuilder();
                StringBuilder ctorPropParamAssign = new StringBuilder();
                StringBuilder getListDtoSelect = new StringBuilder();
                StringBuilder getListFilterCondition = new StringBuilder();
                string multiTenant = "";
                string getListOrderBy = "";

                if (_inputForm.IsMultiTenant)
                {
                    multiTenant = ", IMultiTenant";
                    var propType = "Guid?";
                    var propName = "TenantId";
                    var propNameCamelCase = ToCamelCase(propName);
                    properties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                    ctorPropParam.Append($", {propType} {propNameCamelCase}");
                    ctorPropParamAssign.AppendLine($"        {propName} = {propNameCamelCase};");

                    AddMultiTenantValidation(replacementsDictionary, itemTemplate);
                }

                if (!string.IsNullOrWhiteSpace(_inputForm.Properties))
                {
                    var propertiesData = _inputForm.Properties.Split(',');
                    foreach (var property in propertiesData)
                    {
                        var prop = property.Split(':');
                        bool isRequired = false;
                        if (prop.Length >= 2)
                        {
                            var propType = prop.LastOrDefault();
                            var propName = prop.FirstOrDefault();
                            if (prop.Length >= 3)
                            {
                                propName = prop[1];
                                isRequired = (prop[2].ToLower() == "R".ToLower());
                            }
                            var propNameCamelCase = ToCamelCase(propName);

                            properties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                            if (isRequired)
                            {
                                createupdateproperties.AppendLine($"   [Required]");
                            }
                            createupdateproperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                            dtoproperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");

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
                            filterproperties.AppendLine($"    public {propType} {propName} {{ get; set; }}");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(getListOrderBy))
                {
                    getListOrderBy = "Name";
                }

                // Add custom parameters.
                if (replacementsDictionary.ContainsKey("$entitycamelcase$"))
                {
                    replacementsDictionary["$entitycamelcase$"] = ToCamelCase(itemTemplate.SafeItemName);
                }
                if (replacementsDictionary.ContainsKey("$microservicename$"))
                {
                    replacementsDictionary["$microservicename$"] = itemTemplate.ServiceName;
                }
                if (replacementsDictionary.ContainsKey("$pluralentityname$"))
                {
                    replacementsDictionary["$pluralentityname$"] = _inputForm.PluralEntityName;
                }
                if (replacementsDictionary.ContainsKey("$properties$"))
                {
                    replacementsDictionary["$properties$"] = properties.ToString();
                }
                if (replacementsDictionary.ContainsKey("$createupdateproperties$"))
                {
                    replacementsDictionary["$createupdateproperties$"] = createupdateproperties.ToString();
                }
                if (replacementsDictionary.ContainsKey("$dtoproperties$"))
                {
                    replacementsDictionary["$dtoproperties$"] = dtoproperties.ToString();
                }
                if (replacementsDictionary.ContainsKey("$filterproperties$"))
                {
                    replacementsDictionary["$filterproperties$"] = filterproperties.ToString();
                }
                if (replacementsDictionary.ContainsKey("$ctorpropparam$"))
                {
                    replacementsDictionary["$ctorpropparam$"] = ctorPropParam.ToString();
                }
                if (replacementsDictionary.ContainsKey("$ctorpropparamassign$"))
                {
                    replacementsDictionary["$ctorpropparamassign$"] = ctorPropParamAssign.ToString();
                }
                if (replacementsDictionary.ContainsKey("$multitenant$"))
                {
                    replacementsDictionary["$multitenant$"] = multiTenant;
                }

                if (replacementsDictionary.ContainsKey("$getlistorderby$"))
                {
                    replacementsDictionary["$getlistorderby$"] = getListOrderBy;
                }
                if (replacementsDictionary.ContainsKey("$getlistdtoselect$"))
                {
                    replacementsDictionary["$getlistdtoselect$"] = getListDtoSelect.ToString();
                }
                if (replacementsDictionary.ContainsKey("$getlistfiltercondition$"))
                {
                    replacementsDictionary["$getlistfiltercondition$"] = getListFilterCondition.ToString();
                }

                //replacementsDictionary.Add("$entitycamelcase$", "");

                UpdateDbContext(itemTemplate);
                UpdateAutoMapperProfile(itemTemplate);
                UpdatePermissionDefinition(itemTemplate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UpdateDbContext(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetDbContextPath(itemTemplate.ServiceName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "#region App Entities";
                var newText = positionText + $"\r\npublic DbSet<{itemTemplate.SafeItemName}> {itemTemplate.SafeItemName}s {{ get; set;}} \r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdateAutoMapperProfile(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetAutoMapperProfilePath(itemTemplate.ServiceName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "* into multiple profile classes for a better organization. */";
                var newText = positionText
                    + $"\r\nCreateMap<CreateUpdate{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}>();\r\n"
                    + $"\r\nCreateMap<{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}Dto>();\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void UpdatePermissionDefinition(ItemTemplate itemTemplate)
        {
            var filePath = itemTemplate.SolutionDirectorySubPath + FilePath.GetAutoMapperProfilePath(itemTemplate.ServiceName);
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                var positionText = "#endregion Desktop Permissions";
                var newText = positionText
                    + $"\r\nCreateMap<CreateUpdate{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}>();\r\n"
                    + $"\r\nCreateMap<{itemTemplate.SafeItemName}Dto, {itemTemplate.SafeItemName}Dto>();\r\n";
                fileText = fileText.Replace(positionText, newText);
                File.WriteAllText(filePath, fileText);
            }
        }

        private void AddMultiTenantValidation(Dictionary<string, string> replacementsDictionary, ItemTemplate itemTemplate)
        {
            #region Multi Tenant Authorization
            StringBuilder multiTenantAuthorizationValidationStart = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationEnd = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationGet = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationGetList = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationCreate = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationUpdate = new StringBuilder();
            StringBuilder multiTenantAuthorizationValidationDelete = new StringBuilder();
            StringBuilder multitenantauthorizationimport = new StringBuilder();

            multiTenantAuthorizationValidationStart.AppendLine("            if (CurrentTenant.Id == null)");
            multiTenantAuthorizationValidationStart.AppendLine("            {");
            multiTenantAuthorizationValidationStart.AppendLine("                var msg = $\"Unauthorized Access\";");
            multiTenantAuthorizationValidationEnd.AppendLine("                throw new AbpAuthorizationException();");
            multiTenantAuthorizationValidationEnd.AppendLine("            }");

            multiTenantAuthorizationValidationGet.Append(multiTenantAuthorizationValidationStart);
            multiTenantAuthorizationValidationGet.AppendLine($"                this._commonDependencies._logger.LogInformation($\"::{itemTemplate.SafeItemName}AppService:: - GetAsync - ::{{msg}}::\");");
            multiTenantAuthorizationValidationGet.Append(multiTenantAuthorizationValidationEnd);

            multiTenantAuthorizationValidationGetList.Append(multiTenantAuthorizationValidationStart);
            multiTenantAuthorizationValidationGetList.AppendLine($"                this._commonDependencies._logger.LogInformation($\"::{itemTemplate.SafeItemName}AppService:: - GetListAsync - ::{{msg}}::\");");
            multiTenantAuthorizationValidationGetList.Append(multiTenantAuthorizationValidationEnd);

            multiTenantAuthorizationValidationCreate.Append(multiTenantAuthorizationValidationStart);
            multiTenantAuthorizationValidationCreate.AppendLine($"                this._commonDependencies._logger.LogInformation($\"::{itemTemplate.SafeItemName}AppService:: - CreateAsync - ::{{msg}}::\");");
            multiTenantAuthorizationValidationCreate.Append(multiTenantAuthorizationValidationEnd);

            multiTenantAuthorizationValidationUpdate.Append(multiTenantAuthorizationValidationStart);
            multiTenantAuthorizationValidationUpdate.AppendLine($"                this._commonDependencies._logger.LogInformation($\"::{itemTemplate.SafeItemName}AppService:: - UpdateAsync - ::{{msg}}::\");");
            multiTenantAuthorizationValidationUpdate.Append(multiTenantAuthorizationValidationEnd);

            multiTenantAuthorizationValidationDelete.Append(multiTenantAuthorizationValidationStart);
            multiTenantAuthorizationValidationDelete.AppendLine($"                this._commonDependencies._logger.LogInformation($\"::{itemTemplate.SafeItemName}AppService:: - DeleteAsync - ::{{msg}}::\");");
            multiTenantAuthorizationValidationDelete.Append(multiTenantAuthorizationValidationEnd);

            if (replacementsDictionary.ContainsKey("$multitenantauthorizationvalidationget$"))
            {
                replacementsDictionary["$multitenantauthorizationvalidationget$"] = multiTenantAuthorizationValidationGet.ToString();
            }

            if (replacementsDictionary.ContainsKey("$multitenantauthorizationvalidationgetlist$"))
            {
                replacementsDictionary["$multitenantauthorizationvalidationgetlist$"] = multiTenantAuthorizationValidationGetList.ToString();
            }

            if (replacementsDictionary.ContainsKey("$multitenantauthorizationvalidationcreate$"))
            {
                replacementsDictionary["$multitenantauthorizationvalidationcreate$"] = multiTenantAuthorizationValidationCreate.ToString();
            }

            if (replacementsDictionary.ContainsKey("$multitenantauthorizationvalidationupdate$"))
            {
                replacementsDictionary["$multitenantauthorizationvalidationupdate$"] = multiTenantAuthorizationValidationUpdate.ToString();
            }

            if (replacementsDictionary.ContainsKey("$multitenantauthorizationvalidationdelete$"))
            {
                replacementsDictionary["$multitenantauthorizationvalidationdelete$"] = multiTenantAuthorizationValidationDelete.ToString();
            }

            multitenantauthorizationimport.AppendLine($"using Volo.Abp.Authorization;");
            if (replacementsDictionary.ContainsKey("$multitenantauthorizationimport$"))
            {
                replacementsDictionary["$multitenantauthorizationimport$"] = multitenantauthorizationimport.ToString();
            }
            #endregion
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return AddPItem;
        }

        private string ToCamelCase(string name)
        {
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
