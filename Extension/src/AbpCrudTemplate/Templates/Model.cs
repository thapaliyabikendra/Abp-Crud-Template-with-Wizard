using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace $rootnamespace$.$pluralentityname$;
public class $safeitemname$: FullAuditedAggregateRoot<Guid>
{
    public $safeitemname$()
    {
    }
    public $safeitemname$(Guid id$ctorpropparam$) : base(id)
    {$ctorpropparamassign$
    }$properties$
}