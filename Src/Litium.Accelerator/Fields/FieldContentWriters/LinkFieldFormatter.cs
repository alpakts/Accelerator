using System;
using System.Web;
using Litium.FieldFramework;
using Litium.FieldFramework.FieldTypes;
using Litium.Products;
using Litium.Runtime.AutoMapper;
using Litium.Runtime.DependencyInjection;
using Litium.Web;
using Litium.Web.Models;
using Litium.Web.Models.Products;
using Litium.Websites;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a link field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.Link)]
    internal class LinkFieldFormatter : FieldFormatter
    {
        private readonly UrlService _urlService;
        private readonly PageService _pageService;

        public LinkFieldFormatter(UrlService urlService, PageService pageService)
        {
            _urlService = urlService;
            _pageService = pageService;
        }

        /// <summary>
        ///     Converts the value of a specified object to an equivalent string representation using specified format and
        ///     culture-specific formatting information.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public override string Format(IFieldDefinition fieldDefinition, object item, FieldFormatArgs args)
        {
            args.ContentType = "text/plain";
            var linkItem = item as LinkItem;
            if (linkItem == null)
            {
                return null;
            }

            var linkArgs = args as LinkFieldFormatArgs;
            string result = GetLink(linkItem, linkArgs);
            linkArgs.Text = linkItem.Text;
            return (args.HtmlEncode && result != null) ? HttpUtility.HtmlEncode(result) : result;
        }

        private string GetLink(LinkItem linkItem, LinkFieldFormatArgs args)
        {
            switch (linkItem.EntityType)
            {
                case LinkTypeConstants.MediaFile:
                    return linkItem.EntitySystemId?.MapTo<FileModel>()?.Url;
                case LinkTypeConstants.ProductsBaseProduct:
                    return linkItem.EntitySystemId?.MapTo<BaseProduct>()?.GetUrl(Guid.Empty, channelSystemId: args.ChannelSystemId);
                case LinkTypeConstants.ProductsVariant:
                    return linkItem.EntitySystemId?.MapTo<Variant>()?.GetUrl(channelSystemId: args.ChannelSystemId);
                case LinkTypeConstants.ProductsCategory:
                    return linkItem.EntitySystemId?.MapTo<Category>()?.MapTo<CategoryModel>()?.Category.GetUrl(args.ChannelSystemId.GetValueOrDefault());
                case LinkTypeConstants.WebsitesPage:
                    var page = linkItem.EntitySystemId.HasValue ? _pageService.Get(linkItem.EntitySystemId.Value) : null;
                    if (page != null)
                    {
                        var channelSystemId = linkItem.ChannelSystemId != null && linkItem.ChannelSystemId != Guid.Empty ? linkItem.ChannelSystemId.Value : args.ChannelSystemId.GetValueOrDefault();
                        return _urlService.GetUrl(page, new PageUrlArgs(channelSystemId));
                    }
                    break;
                case LinkTypeConstants.Url:
                   args.IsExternalLink = true;
                   return linkItem.Url;
            }
            return string.Empty;
        }
    }
}
