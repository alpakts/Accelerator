using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Litium.Common;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Security;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Definitions
{
    public class AcceleratorDefaultPermissionSetup : IDefinitionInit
    {
        private readonly GroupService _groupService;
        private readonly DefaultAccessControlService _defaultAccessControlService;
        private readonly SettingService _settingService;
        private readonly SecurityContextService _securityContextService;
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly PersonService _personService;
        private readonly FieldFrameworkSetupLocalizationService _fieldFrameworkSetupLocalizationService;
        private readonly ILogger<AcceleratorDefaultPermissionSetup> _logger;

        public AcceleratorDefaultPermissionSetup(
            GroupService groupService,
            DefaultAccessControlService defaultAccessControlService,
            SettingService settingService,
            SecurityContextService securityContextService,
            FieldTemplateService fieldTemplateService,
            PersonService personService,
            FieldFrameworkSetupLocalizationService fieldFrameworkSetupLocalizationService,
            ILogger<AcceleratorDefaultPermissionSetup> logger)
        {
            _groupService = groupService;
            _defaultAccessControlService = defaultAccessControlService;
            _settingService = settingService;
            _securityContextService = securityContextService;
            _fieldTemplateService = fieldTemplateService;
            _personService = personService;
            _fieldFrameworkSetupLocalizationService = fieldFrameworkSetupLocalizationService;
            _logger = logger;
        }

        private Guid CreateVisitorGroup()
        {
            var defaultGroupTemplate = _fieldTemplateService.GetAll().OfType<GroupFieldTemplate>().FirstOrDefault();
            if (defaultGroupTemplate == null)
            {
                defaultGroupTemplate = new GroupFieldTemplate("DefaultGroupTemplate");
                _fieldFrameworkSetupLocalizationService.Localize(defaultGroupTemplate);
                _fieldTemplateService.Create(defaultGroupTemplate);
            }
            var visitorGroup = new StaticGroup(defaultGroupTemplate.SystemId, "Visitors")
            {
                Id = "Visitors"
            };

            var everyone = _personService.Get(SecurityContextService.Everyone.Id)?.MakeWritableClone();
            if (everyone != null)
            {
                everyone.GroupLinks.Add(new PersonToGroupLink(visitorGroup.SystemId));
            }

            _logger.LogTrace("Creating visitor group {@Group}", visitorGroup);
            _groupService.Create(visitorGroup);
            _logger.LogTrace("Created visitor group {@Group}", visitorGroup);

            _logger.LogTrace("Adding everyone to visitor group {@Everyone}", everyone);
            _personService.Update(everyone);
            _logger.LogTrace("Added everyone to visitor group {@Everyone}", everyone);

            return visitorGroup.SystemId;
        }

        private void SetPimDefaultPermission()
        {
            var visitorGroupSystemId = (_groupService.Get<Group>("Visitors") ?? _groupService.Get<Group>("Besökare"))?.SystemId ?? CreateVisitorGroup();
            if (visitorGroupSystemId == Guid.Empty)
            {
                return;
            }
            SetEntityDefaultPermission<BaseProduct>(visitorGroupSystemId);
            SetEntityDefaultPermission<ProductPriceList>(visitorGroupSystemId);
            SetEntityDefaultPermission<ProductList>(visitorGroupSystemId);
        }

        private void SetEntityDefaultPermission<TEntity>(Guid visitorGroupSystemId)
            where TEntity : IDefaultAccessControlSupport
        {
            var defaultAccessControl = _defaultAccessControlService.Get<TEntity>()?.MakeWritableClone();
            if (defaultAccessControl.AccessControlList.Any(x => x.Operation == Operations.Entity.Read && x.GroupSystemId == visitorGroupSystemId))
            {
                return;
            }
            defaultAccessControl.AccessControlList.Add(new AccessControlEntry(Operations.Entity.Read, visitorGroupSystemId));
            _defaultAccessControlService.Update(defaultAccessControl);
        }

        private bool IsAlreadyExecuted()
        {
            return _settingService.Get<bool>($"AcceleratorDefaultPermissionSetup");
        }

        private void SetAlreadyExecuted()
        {
            _settingService.Set($"AcceleratorDefaultPermissionSetup", true);
        }

        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            if (!IsAlreadyExecuted())
            {
                using (_securityContextService.ActAsSystem())
                {
                    SetPimDefaultPermission();
                    SetAlreadyExecuted();
                }
            }

            return ValueTask.CompletedTask;
        }
    }
}
