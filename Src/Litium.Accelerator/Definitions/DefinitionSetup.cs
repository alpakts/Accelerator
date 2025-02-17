using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Litium.Accelerator.Search.Filtering;
using Litium.Accelerator.Services;
using Litium.Common;
using Litium.Customers;
using Litium.FieldFramework;
using Litium.Products;
using Litium.Runtime;
using Litium.Security;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Block = Litium.Blocks;

namespace Litium.Accelerator.Definitions
{
    [Autostart]
    public class DefinitionSetup : IAsyncAutostart
    {
        private readonly FieldTemplateService _fieldTemplateService;
        private readonly SettingService _settingService;
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly SecurityContextService _securityContextService;
        private readonly RelationshipTypeService _relationshipTypeService;
        private readonly Block.CategoryService _categoryService;
        private readonly AddressTypeService _addressTypeService;
        private readonly RoleService _roleService;
        private readonly TemplateSettingService _templateSettingService;
        private readonly IEnumerable<FieldDefinitionSetup> _definitionSetups;
        private readonly IEnumerable<FieldTemplateSetup> _fieldTemplateSetups;
        private readonly IEnumerable<RelationshipTypeSetup> _relationshipTypeSetups;
        private readonly IEnumerable<BlockCategorySetup> _categorySetups;
        private readonly IEnumerable<AddressTypeSetup> _addressTypeSetups;
        private readonly IEnumerable<RoleSetup> _roleSetups;
        private readonly FieldFrameworkSetupLocalizationService _fieldFrameworkSetupLocalizationService;
        private readonly FilterService _filterService;
        private readonly ILogger<DefinitionSetup> _logger;
        private readonly IEnumerable<IDefinitionSetup> _extraSetup;
        private readonly IEnumerable<IDefinitionInit> _extraInit;
        private readonly IStringLocalizer<FieldFrameworkSetupLocalizationService> _localizer;

        public DefinitionSetup(
            FieldDefinitionService fieldDefinitionService,
            FieldTemplateService fieldTemplateService,
            SettingService settingService,
            SecurityContextService securityContextService,
            RelationshipTypeService relationshipTypeService,
            Block.CategoryService categoryService,
            AddressTypeService addressTypeService,
            RoleService roleService,
            TemplateSettingService templateSettingService,
            IEnumerable<FieldDefinitionSetup> fieldDefinitonSetups,
            IEnumerable<FieldTemplateSetup> fieldTemplateSetups,
            IEnumerable<RelationshipTypeSetup> relationshipTypeSetups,
            IEnumerable<BlockCategorySetup> categorySetup,
            IEnumerable<AddressTypeSetup> addressTypeSetup,
            IEnumerable<RoleSetup> roleSetups,
            FieldFrameworkSetupLocalizationService fieldFrameworkSetupLocalizationService,
            FilterService filterService,
            ILogger<DefinitionSetup> logger,
            IEnumerable<IDefinitionSetup> extraSetup,
            IEnumerable<IDefinitionInit> extraInit,
            IStringLocalizer<FieldFrameworkSetupLocalizationService> localizer)
        {
            _fieldDefinitionService = fieldDefinitionService;
            _fieldTemplateService = fieldTemplateService;
            _settingService = settingService;
            _definitionSetups = fieldDefinitonSetups;
            _fieldTemplateSetups = fieldTemplateSetups;
            _securityContextService = securityContextService;
            _relationshipTypeService = relationshipTypeService;
            _categoryService = categoryService;
            _templateSettingService = templateSettingService;
            _relationshipTypeSetups = relationshipTypeSetups;
            _categorySetups = categorySetup;
            _addressTypeService = addressTypeService;
            _addressTypeSetups = addressTypeSetup;
            _roleSetups = roleSetups;
            _roleService = roleService;
            _fieldFrameworkSetupLocalizationService = fieldFrameworkSetupLocalizationService;
            _filterService = filterService;
            _logger = logger;
            _extraSetup = extraSetup;
            _extraInit = extraInit;
            _localizer = localizer;
        }

        private void Start()
        {
            using (_securityContextService.ActAsSystem("setup"))
            {
                foreach (var item in _definitionSetups)
                {
                    InitFields(item.GetFieldDefinitions());
                }

                foreach (var item in _relationshipTypeSetups)
                {
                    InitRelationshipTypes(item.GetRelationshipTypes());
                }

                foreach (var item in _addressTypeSetups)
                {
                    InitAddressType(item.GetAddressTypes());
                }

                foreach (var roleSetup in _roleSetups)
                {
                    InitRoles(roleSetup.GetRoles());
                }

                foreach (var categorySetup in _categorySetups)
                {
                    InitCategories(categorySetup.GetCategories());
                }

                foreach (var item in _fieldTemplateSetups)
                {
                    InitTemplates(item.GetTemplates());
                }

                if (!IsAlreadyExecuted<FieldTemplate>("ProductWithVariants:Settings"))
                {
                    _templateSettingService.SetTemplateGroupings("ProductWithVariants", "color");
                    SetAlreadyExecuted<FieldTemplate>("ProductWithVariants:Settings");
                }

                if (!IsAlreadyExecuted<FieldTemplate>("ProductFilters:Settings"))
                {
                    var filters = new List<String>
                    {
                        "Size",
                        "#Price",
                        "Color",
                        "Brand",
                        "#News"
                    };
                    _filterService.SaveProductFilteringFields(filters);
                    SetAlreadyExecuted<FieldTemplate>("ProductFilters:Settings");
                }
            }
        }

        private void InitRoles(IEnumerable<Role> roles)
        {
            foreach (var item in roles)
            {
                if (IsAlreadyExecuted<Role>(item.Id))
                {
                    continue;
                }
                var currentField = _roleService.Get(item.Id);
                if (currentField != null)
                {
                    continue;
                }

                foreach (var culture in new[] { "sv-SE", "en-US" })
                {
                    item.Localizations[culture].Name = GetString(new CultureInfo(culture), $"roles.{item.Id.ToLowerInvariant()}.name");
                }

                _roleService.Create(item);
                SetAlreadyExecuted<Role>(item.Id);
            }
        }

        private void InitCategories(IEnumerable<Block.Category> categories)
        {
            foreach (var category in categories)
            {
                if (IsAlreadyExecuted<Category>(category.Id))
                {
                    continue;
                }
                var currentCategory = _categoryService.Get(category.Id);
                if (currentCategory != null)
                {
                    continue;
                }
                _categoryService.Create(category);
                SetAlreadyExecuted<Category>(category.Id);
            }
        }

        private void InitFields(IEnumerable<FieldDefinition> fields)
        {
            foreach (var item in fields)
            {
                if (IsAlreadyExecuted<FieldDefinition>(item.Id, item.AreaType.Name))
                {
                    continue;
                }

                var currentField = _fieldDefinitionService.Get(item.AreaType, item.Id);
                if (currentField == null)
                {
                    _fieldFrameworkSetupLocalizationService.Localize(item);
                    _fieldDefinitionService.Create(item);
                    SetAlreadyExecuted<FieldDefinition>(item.Id, item.AreaType.Name);
                    continue;
                }

                if (item.FieldType != currentField.FieldType)
                {
                    _logger.LogError("Accelerator \"{Id}\" field with \"{FieldType}\" type can't be created. The system already has the \"{CurrentId}\" field with \"{CurrentFieldType}\" type. Accelerator deployment would fail.", item.Id, item.FieldType, currentField.Id, currentField.FieldType);
                    _settingService.Set<bool?>("Accelerator.DefinitionsError", true);
                    continue;
                }
                if (item.MultiCulture != currentField.MultiCulture)
                {
                    _logger.LogError("Accelerator \"{Id}\" field with \"MultiCulture\" setting and \"{MultiCulture}\" value can't be created. The system already has the \"{CurrentId}\" field with \"MultiCulture\" setting and \"{CurrentMultiCulture}\" value. Accelerator deployment would fail.", item.Id, item.MultiCulture, currentField.Id, currentField.MultiCulture);
                    _settingService.Set<bool?>("Accelerator.DefinitionsError", true);
                    continue;
                }

                SetAlreadyExecuted<FieldDefinition>(item.Id, item.AreaType.Name);
            }
        }

        private void InitRelationshipTypes(IEnumerable<RelationshipType> relationshipTypes)
        {
            foreach (var item in relationshipTypes)
            {
                if (IsAlreadyExecuted<RelationshipType>(item.Id))
                {
                    continue;
                }
                var currentItem = _relationshipTypeService.Get(item.Id);
                if (currentItem != null)
                {
                    continue;
                }
                _relationshipTypeService.Create(item);
                SetAlreadyExecuted<RelationshipType>(item.Id);
            }
        }

        private void InitAddressType(IEnumerable<AddressType> addressTypes)
        {
            foreach (var type in addressTypes)
            {
                if (IsAlreadyExecuted<AddressType>(type.Id))
                {
                    continue;
                }
                var currentField = _addressTypeService.Get(type.Id);
                if (currentField != null)
                {
                    continue;
                }
                _addressTypeService.Create(type);
                SetAlreadyExecuted<AddressType>(type.Id);
            }
        }

        private void InitTemplates(IEnumerable<FieldTemplate> templates)
        {
            foreach (var item in templates)
            {
                if (IsAlreadyExecuted<FieldTemplate>(item.Id, item.AreaType.Name))
                {
                    //continue;
                }

                var currentField = _fieldTemplateService.Get<FieldTemplate>(item.AreaType, item.Id);
                try
                {
                    if (item is ProductFieldTemplate template)
                    {
                        _fieldFrameworkSetupLocalizationService.Localize(item, template.ProductFieldGroups.Concat(template.VariantFieldGroups).ToList());
                    }
                    else
                    {
                        _fieldFrameworkSetupLocalizationService.Localize(item, ((dynamic)item).FieldGroups as ICollection<FieldTemplateFieldGroup>);
                    }
                }
                catch
                {
                    _fieldFrameworkSetupLocalizationService.Localize(item);
                }

                if (currentField == null)
                {
                    _fieldTemplateService.Create(item);
                }
                else
                {
                    item.SystemId = currentField.SystemId;
                    _fieldTemplateService.Update(item);
                }

                SetAlreadyExecuted<FieldTemplate>(item.Id, item.AreaType.Name);
            }
        }

        private bool IsAlreadyExecuted<T>(string id, string area = null)
        {
            if (area == null)
            {
                return _settingService.Get<bool>($"AcceleratorBuilder:{typeof(T).FullName}:{id}");
            }
            return _settingService.Get<bool>($"AcceleratorBuilder:{typeof(T).FullName}:{area}:{id}");
        }

        private void SetAlreadyExecuted<T>(string id, string area = null)
        {
            if (area == null)
            {
                _settingService.Set($"AcceleratorBuilder:{typeof(T).FullName}:{id}", true);
            }

            _settingService.Set($"AcceleratorBuilder:{typeof(T).FullName}:{area}:{id}", true);
        }

        private string GetString(CultureInfo x, string s)
        {
            var oldCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = x;
            try
            {
                var str = _localizer.GetString(s);
                return str is null || str.ResourceNotFound ? null : str.Value;
            }
            finally
            {
                CultureInfo.CurrentUICulture = oldCulture;
            }
        }

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            foreach (var item in _extraInit)
            {
                await item.StartAsync(cancellationToken).ConfigureAwait(false);
            }

            Start();

            foreach (var item in _extraSetup)
            {
                await item.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
