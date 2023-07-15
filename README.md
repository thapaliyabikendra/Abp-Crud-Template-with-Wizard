# Using the "Abp Crud Template with Wizard" Extension to Generate Abp CrudAppService Code

This guide provides step-by-step instructions on how to use the "Abp Crud Template with Wizard" extension to generate Abp CrudAppService code. The extension requires the following fields: `AppName`, `PluralEntityName`, and `Properties`. Follow the instructions below to effectively utilize this extension:

## Instructions

1. Download and install the "Abp Crud Template with Wizard" extension.
2. Launch Visual Studio.
3. Locate the `{AppName}DbContext.cs` file in your project.
4. Open the `{AppName}DbContext.cs` file.
5. Identify the region in which you want to insert the code snippet and position your cursor accordingly.

6. Insert the following code snippet at the desired location:
```csharp
#region App Entities \r\n #endregion
```
7. Save the file to apply the changes. This code snippet will be added to the `{AppName}DbContext.cs` file, creating a region for your app entities.

8. Create a new item in Visual Studio.
9. Select the "Abp Crud Template with Wizard" item and enter the entity name, then Click the "Add" button to continue.
10. Fill in the required fields (`AppName`, `PluralEntityName`, `Properties`) in the form provided.
11. Enter multiple properties in the `Properties` field using the format: `propertyName:propertyType[:isRequired]`. Separate each property with a comma (`,`).
12. Click the "ok" button to generate the Abp CrudAppService code based on the provided information.
13. The generated code will be automatically created and added to your project.

## Conclusion

By following these instructions, you can effectively utilize the "Abp Crud Template with Wizard" extension to generate Abp CrudAppService code snippets, enhancing your development workflow.

Note: Ensure that you replace `{AppName}` with the actual name of your application and provide the necessary information in the form fields (`PluralEntityName`, `Properties`) according to your application requirements.