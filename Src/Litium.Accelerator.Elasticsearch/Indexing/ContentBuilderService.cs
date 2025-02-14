using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Services;
using Litium.Common;
using Litium.Common.Events;
using Litium.Events;
using Litium.FieldFramework;
using Litium.FieldFramework.Events;
using Litium.FieldFramework.FieldTypes;
using Litium.Runtime;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Search.Indexing
{
    [Service(ServiceType = typeof(ContentBuilderService), Lifetime = DependencyLifetime.Singleton)]
    public class ContentBuilderService
    {
        private readonly FieldDefinitionService _fieldDefinitionService;
        private readonly TemplateSettingService _templateSettingService;
        private readonly KeyLookupService _keyLookupService;

        private readonly ConcurrentDictionary<(Type, string), ICollection<FieldDefinition>> _fieldCache =
            new ConcurrentDictionary<(Type, string), ICollection<FieldDefinition>>();

        public ContentBuilderService(
            FieldDefinitionService fieldDefinitionService,
            TemplateSettingService templateSettingService,
            KeyLookupService keyLookupService,
            EventBroker eventBroker)
        {
            _fieldDefinitionService = fieldDefinitionService;
            _templateSettingService = templateSettingService;
            _keyLookupService = keyLookupService;

            eventBroker.Subscribe<ClearFieldCache>(_ => _fieldCache.Clear());
            eventBroker.Subscribe<FieldDefinitionCreated>(_ => eventBroker.PublishAsync(EventScope.LocalAndRemote, new ClearFieldCache()));
            eventBroker.Subscribe<FieldDefinitionUpdated>(_ => eventBroker.PublishAsync(EventScope.LocalAndRemote, new ClearFieldCache()));
            eventBroker.Subscribe<FieldDefinitionDeleted>(_ => eventBroker.PublishAsync(EventScope.LocalAndRemote, new ClearFieldCache()));
            eventBroker.Subscribe<SettingChanged>(async x =>
            {
                if (x.Key.StartsWith("IndexingTemplateFields:", StringComparison.InvariantCultureIgnoreCase))
                {
                    await eventBroker.PublishAsync(EventScope.LocalAndRemote, new ClearFieldCache());
                }
            });
        }

        public ISet<string> BuildContent<TTemplate, TArea>(Guid fieldTemplateSystemId, CultureInfo cultureInfo, MultiCultureFieldContainer fieldContainer)
            where TTemplate : FieldTemplate where TArea : IArea
        {
            ISet<string> content = new HashSet<string>();
            if (!_keyLookupService.TryGetId<TTemplate>(fieldTemplateSystemId, out var templateId))
            {
                return content;
            }

            var fields = _fieldCache.GetOrAdd((typeof(TArea), templateId), key => _templateSettingService.GetTemplateIndexingFields<TArea>(templateId)
                ?.Select(x => _fieldDefinitionService.Get<TArea>(x)).Where(x => x != null).ToList());

            if (fields == null || fields.Count == 0)
            {
                // Take all fields in case if a template doesn't have any selected fields or selected fields were deleted from the system.
                fields = _fieldCache.GetOrAdd((typeof(TArea), null), _ => _fieldDefinitionService.GetAll<TArea>().ToList());
            }

            if (fields.Count == 0)
            {
                return content;
            }

            foreach (var field in fields)
            {
                var value = field.MultiCulture
                    ? fieldContainer.GetValue<object>(field.Id, cultureInfo)
                    : fieldContainer.GetValue<object>(field.Id);

                if (value is null)
                {
                    continue;
                }

                PopulateContent(field, value);
            }

            void PopulateContent(FieldDefinition fieldDefinition, object value)
            {
                switch (fieldDefinition.FieldType)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    case SystemFieldTypeConstants.CustomerPointerOrganization:
#pragma warning restore CS0618 // Type or member is obsolete
                    case SystemFieldTypeConstants.Date:
                    case SystemFieldTypeConstants.DateTime:
                    case SystemFieldTypeConstants.MediaPointerFile:
                    case SystemFieldTypeConstants.MediaPointerImage:
                    case SystemFieldTypeConstants.Object:
                    case SystemFieldTypeConstants.Pointer:
                    case SystemFieldTypeConstants.Boolean:
                    case SystemFieldTypeConstants.Link:
                    case "FilterFields":
                    case "MediaPointerImageArray":
                        break;
                    case SystemFieldTypeConstants.MultiField:
                        var fieldOption = (MultiFieldOption)fieldDefinition.Option;
                        var multiFieldItems = ((IList<MultiFieldItem>)value);
                        foreach (var multiFieldItem in multiFieldItems)
                        {
                            foreach (var multiFieldItemField in fieldOption.Fields)
                            {
                                var multiFieldItemFieldDefinition = _fieldDefinitionService.Get<TArea>(multiFieldItemField);
                                if (multiFieldItemFieldDefinition is null)
                                {
                                    continue;
                                }

                                var multiFieldItemValue = multiFieldItemFieldDefinition.MultiCulture ? multiFieldItem.Fields.GetValue<object>(multiFieldItemField, cultureInfo) : multiFieldItem.Fields.GetValue<object>(multiFieldItemField);
                                if (multiFieldItemValue != null)
                                {
                                    PopulateContent(multiFieldItemFieldDefinition, multiFieldItemValue );
                                }
                            }
                        }
                        break;

                    case SystemFieldTypeConstants.Decimal:
                        content.Add(((decimal)value).ToString(cultureInfo));
                        break;

                    case SystemFieldTypeConstants.DecimalOption:
                        if (value is IList<decimal> decimalValues)
                        {
                            foreach (var item in OptionValueHelper.GetDecimalOptionValues((DecimalOption)fieldDefinition.Option, decimalValues, cultureInfo))
                            {
                                content.Add(item);
                            }
                        }
                        else
                        {
                            content.Add(OptionValueHelper.GetDecimalOptionValue((DecimalOption)fieldDefinition.Option, (decimal)value, cultureInfo));
                        }
                        break;

                    case SystemFieldTypeConstants.Int:
                        content.Add(((int)value).ToString(cultureInfo));
                        break;

                    case SystemFieldTypeConstants.IntOption:
                        if (value is IList<int> intValues)
                        {
                            foreach (var item in OptionValueHelper.GetIntOptionValues((IntOption)fieldDefinition.Option, intValues, cultureInfo))
                            {
                                content.Add(item);
                            }
                        }
                        else
                        {
                            content.Add(OptionValueHelper.GetIntOptionValue((IntOption)fieldDefinition.Option, (int)value, cultureInfo));
                        }
                        break;

                    case SystemFieldTypeConstants.Long:
                        content.Add(((long)value).ToString(cultureInfo));
                        break;

                    case SystemFieldTypeConstants.TextOption:
                        if (value is IList<string> textValues)
                        {
                            foreach (var item in OptionValueHelper.GetTextOptionValues((TextOption)fieldDefinition.Option, textValues, cultureInfo))
                            {
                                content.Add(item);
                            }
                        }
                        else
                        {
                            content.Add(OptionValueHelper.GetTextOptionValue((TextOption)fieldDefinition.Option, (string) value, cultureInfo));
                        }
                        break;

                    case SystemFieldTypeConstants.Editor:
                    case SystemFieldTypeConstants.MultirowText:
                    case SystemFieldTypeConstants.LimitedText:
                    case SystemFieldTypeConstants.Text:
                    default:
                        content.Add(value.ToString());
                        break;
                }

            }

            return content;
        }

        private class ClearFieldCache : IMessage
        {
        }
    }
}
