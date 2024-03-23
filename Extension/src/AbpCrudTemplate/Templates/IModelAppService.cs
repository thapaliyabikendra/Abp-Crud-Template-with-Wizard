using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace $rootnamespace$.$pluralentityname$;
public interface $safeitemname$:
    ICrudAppService<
            $fileinputname$Dto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdate$fileinputname$Dto
        >
{
    public Task<PagedResultDto<$fileinputname$Dto>> GetListByFilterAsync(PagedAndSortedResultRequestDto input, $fileinputname$Filter filter);
}
