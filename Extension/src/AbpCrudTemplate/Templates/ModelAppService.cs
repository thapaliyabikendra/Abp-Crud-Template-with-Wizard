using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;
using $rootnamespace$.Permissions;

namespace $rootnamespace$.$pluralentityname$;
[Authorize($appname$Permissions.$pluralentityname$.Default)]
public class $safeitemname$:     
    CrudAppService<
            $fileinputname$,
            $fileinputname$Dto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdate$fileinputname$Dto
        >,
    I$fileinputname$AppService
{
    #region Properties
    #endregion

    #region Constructor
    public $fileinputname$AppService(
        IRepository<$fileinputname$, Guid> repository
    ): base(repository)
    {
        GetPolicyName = $appname$Permissions.$pluralentityname$.Default;
        GetListPolicyName = $appname$Permissions.$pluralentityname$.Default;
        CreatePolicyName = $appname$Permissions.$pluralentityname$.Create;
        UpdatePolicyName = $appname$Permissions.$pluralentityname$.Edit;
        DeletePolicyName = $appname$Permissions.$pluralentityname$.Delete;
    }
    #endregion

    #region Public Methods
    public override async Task<$fileinputname$Dto> GetAsync(Guid id)
    {
          try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Started::");

            var response = await base.GetAsync(id);
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Ended::");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }

    public override async Task<PagedResultDto<$fileinputname$Dto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - GetListAsync - ::Started::");

            var response = await base.GetListAsync(input);
            Logger.LogInformation($"::$fileinputname$AppService:: - GetListAsync - ::Ended::");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - GetListAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }

    public async Task<PagedResultDto<$fileinputname$Dto>> GetListByFilterAsync(PagedAndSortedResultRequestDto input, $fileinputname$Filter filter)
    {
        try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - GetListByFilterAsync - ::Started::");
            
            if (input.Sorting.IsNullOrWhiteSpace())
            {
                input.Sorting = $"{nameof($fileinputname$Filter.$getlistorderby$)}";
            }
            var _$pluralcamelcaseentityname$ = await Repository.GetQueryableAsync();

            var queryable = (from s in _$pluralcamelcaseentityname$
                             select new $fileinputname$Dto()
                             {
                Id = s.Id,$getlistdtoselect$
                             })$getlistfiltercondition$;

            var dtos = await AsyncExecuter.ToListAsync(
                 queryable
                 .OrderBy(input.Sorting)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
             );
            var totalCount = queryable.Count();

            Logger.LogInformation($"::$fileinputname$AppService:: - GetListByFilterAsync - ::Ended::");
            return new PagedResultDto<$fileinputname$Dto>(
                totalCount,
                dtos
            );
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - GetListByFilterAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }

    public override async Task<$fileinputname$Dto> CreateAsync(CreateUpdate$fileinputname$Dto input)
    {
          try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Started::");

            var response = await base.CreateAsync(input);
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Ended::");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - CreateAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }

    public override async Task<$fileinputname$Dto> UpdateAsync(Guid id, CreateUpdate$fileinputname$Dto input)
    {
        try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - UpdateAsync - ::Started::");

            var $entitycamelcase$ = await Repository.FirstOrDefaultAsync(x => x.Id == id);
            if ($entitycamelcase$ == null)
            {
                var msg = "$fileinputname$ Not Found.";
                Logger.LogInformation($"::$fileinputname$AppService:: - UpdateAsync - ::{msg}::");
                throw new AbpValidationException(msg,
                    new List<ValidationResult>
                    {
                        new ValidationResult(msg, new []{ "id" })
                    }
                );
            }

            var response = await base.UpdateAsync(id, input);
            Logger.LogInformation($"::$fileinputname$AppService:: - UpdateAsync - ::Ended::");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - UpdateAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }

    public override async Task DeleteAsync(Guid id)
    {
        try
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - DeleteAsync - ::Started::");

            var $entitycamelcase$ = await Repository.FirstOrDefaultAsync(s => s.Id == id);
            if ($entitycamelcase$ == null)
            {
                var msg = "$fileinputname$ Not Found.";
                Logger.LogInformation($"::$fileinputname$AppService:: - DeleteAsync - ::{msg}::");
                throw new AbpValidationException(msg,
                    new List<ValidationResult>
                    {
                        new ValidationResult(msg, new []{ "id" })
                    }
                );
            }

            await base.DeleteAsync(id);
            Logger.LogInformation($"::$fileinputname$AppService:: - DeleteAsync - ::Ended::");
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"::$fileinputname$AppService:: - DeleteAsync - ::Exception:: - {ex.Message}");
            throw new UserFriendlyException(ex.Message);
        }
    }
    #endregion
}
