public static class $pluralentityname$
{
    public const string Default = GroupName + ".$pluralentityname$";
    public const string Create = Default + ".Create";
    public const string Edit = Default + ".Edit";
    public const string Delete = Default + ".Delete";
}

var $entitycamelcase$Permission = $appnamecamelcase$Group.AddPermission($appname$Permissions.$pluralentityname$.Default, L("Permission:$pluralentityname$"));
$entitycamelcase$Permission.AddChild($appname$Permissions.$fileinputname$.Create, L("Permission:$pluralentityname$.Create"));
$entitycamelcase$Permission.AddChild($appname$Permissions.$fileinputname$.Edit, L("Permission:$pluralentityname$.Edit"));
$entitycamelcase$Permission.AddChild($appname$Permissions.$fileinputname$.Delete, L("Permission:$pluralentityname$.Delete"));

CreateMap<CreateUpdate$fileinputname$Dto, $fileinputname$>();
CreateMap<$fileinputname$, $fileinputname$Dto>();

public DbSet<$fileinputname$> $pluralentityname$ { get; set; }


{
  "Permission:$pluralentityname$": "$pluralentityname$",
  "Permission:$pluralentityname$.Create": "Create",
  "Permission:$pluralentityname$.Edit": "Edit",
  "Permission:$pluralentityname$.Delete": "Delete",
}