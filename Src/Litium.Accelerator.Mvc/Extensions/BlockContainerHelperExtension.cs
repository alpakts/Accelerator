using Litium.Accelerator.Mvc.Controllers.Framework;
using Litium.Blocks;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Litium.Accelerator.Mvc.Extensions
{
    public static class BlockContainerHelperExtension
    {
        private static readonly LazyService<FieldTemplateService> _fieldTemplateService = new LazyService<FieldTemplateService>();

        public static async Task<IHtmlContent> BlockContainerAsync(this IViewComponentHelper component, Dictionary<string, List<BlockModel>> containers, string containerId)
        {
            var builder = new HtmlContentBuilder();
            if (containers == null || !containers.TryGetValue(containerId, out var blocks) || blocks.Count == 0)
            {
                return builder;
            }

            foreach (var blockModel in blocks)
            {
                var fieldTemplate = _fieldTemplateService.Value.Get<BlockFieldTemplate>(blockModel.FieldTemplateSystemId);
                if (string.IsNullOrWhiteSpace(fieldTemplate?.TemplatePath) || fieldTemplate.TemplatePath.IndexOf("MVC:", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    throw new InvalidOperationException("Could not find template for block.");
                }

                var templateDefaults = fieldTemplate.TemplatePath.Split(':').Skip(1).ToArray();
                if (templateDefaults.Length < 1)
                {
                    throw new InvalidOperationException("Could not find template for block.");
                }

                var controllerType = Type.GetType(templateDefaults[0], true, true);
                if (controllerType == null)
                {
                    throw new InvalidOperationException("Could not find template for block.");
                }

                var tagBuilder = new TagBuilder("section");
                tagBuilder.Attributes["data-litium-block-id"] = blockModel.SystemId.ToString();
                tagBuilder.InnerHtml.AppendHtml(await component.InvokeAsync(controllerType, blockModel));

                builder.AppendHtml(tagBuilder);
                builder.AppendLine();
            }

            return builder;
        }
    }
}