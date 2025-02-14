using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Litium.Accelerator.Constants;
using Litium.Blocks;
using Litium.Common;
using Litium.FieldFramework;
using Litium.Globalization;
using Litium.Search;
using Litium.Search.Indexing;
using Litium.Websites;

namespace Litium.Accelerator.Search.Indexing.Pages
{
    public class PageIndexDocumentBuilder : MultilingualIndexDocumentBuilderBase<PageDocument>
    {
        private readonly ChannelService _channelService;
        private readonly LanguageService _languageService;
        private readonly PageService _pageService;
        private readonly SearchPermissionService _searchPermissionService;
        private readonly ContentBuilderService _contentBuilderService;
        private readonly BlockService _blockService;
        private readonly KeyLookupService _keyLookupService;
        private readonly FieldTemplateService _fieldTemplateService;

        public PageIndexDocumentBuilder(
            IndexDocumentBuilderDependencies dependencies,
            ChannelService channelService,
            LanguageService languageService,
            PageService pageService,
            SearchPermissionService searchPermissionService,
            ContentBuilderService contentBuilderService,
            BlockService blockService,
            KeyLookupService keyLookupService,
            FieldTemplateService fieldTemplateService)
            : base(dependencies)
        {
            _channelService = channelService;
            _languageService = languageService;
            _pageService = pageService;
            _searchPermissionService = searchPermissionService;
            _contentBuilderService = contentBuilderService;
            _blockService = blockService;
            _keyLookupService = keyLookupService;
            _fieldTemplateService = fieldTemplateService;
        }

        public override IEnumerable<(CultureInfo, IDocument)> BuildIndexDocuments(IndexQueueItem item)
        {
            var page = _pageService.Get(item.SystemId);
            if (page == null)
            {
                yield break;
            }

            var channels = _channelService.GetAll()
                .Where(x => x.WebsiteLanguageSystemId.HasValue && x.WebsiteLanguageSystemId.Value != Guid.Empty)
                .ToDictionary(x => x.SystemId, x => x.WebsiteLanguageSystemId.Value);

            var cultureContentCache = new ConcurrentDictionary<CultureInfo, ISet<string>>();
            var permissions = _searchPermissionService.GetPermissions(page);
            var blocks = _blockService.Get(page.Blocks.Items
                .SelectMany(x => x.Items)
                .OfType<BlockItemLink>()
                .Select(x => x.BlockSystemId))
                .ToList();

            var isBrand = false;
            var isNews = false;
            if (_keyLookupService.TryGetId<PageFieldTemplate>(page.FieldTemplateSystemId, out var templateId))
            {
                isBrand = PageTemplateNameConstants.Brand.Equals(templateId, StringComparison.OrdinalIgnoreCase);
                isNews = PageTemplateNameConstants.News.Equals(templateId, StringComparison.OrdinalIgnoreCase);
            }

            foreach (var channelLink in page.ChannelLinks)
            {
                channels.Remove(channelLink.ChannelSystemId);

                var channel = _channelService.Get(channelLink.ChannelSystemId);
                if (channel is null)
                {
                    // Orphaned category link exists, skip to create new index document.
                    continue;
                }

                var cultureInfo = _languageService.Get(channel.WebsiteLanguageSystemId.GetValueOrDefault())?.CultureInfo;
                if (cultureInfo is null)
                {
                    // Channel does not have a culture.
                    continue;
                }

                var pageFieldTemplate = _fieldTemplateService.Get<PageFieldTemplate>(page.FieldTemplateSystemId);
                if (page.Status != ContentStatus.Published || !pageFieldTemplate.IndexThePage || !page.Fields.GetValue<bool>(SystemFieldDefinitionConstants.IndexThePage))
                {
                    yield return (cultureInfo, new RemoveDocument(new PageDocument { PageSystemId = page.SystemId, ChannelSystemId = channel.SystemId }));
                    continue;
                }

                var localization = page.Localizations[cultureInfo];
                yield return (cultureInfo, new PageDocument
                {
                    PageSystemId = page.SystemId,
                    Content = cultureContentCache.GetOrAdd(cultureInfo, _ => _contentBuilderService.BuildContent<PageFieldTemplate, WebsiteArea>(page.FieldTemplateSystemId, cultureInfo, page.Fields)),
                    PublishDateTime = page.PublishedAtUtc ?? DateTimeOffset.MinValue,
                    Name = localization.Name,
                    ChannelSystemId = channel.SystemId,
                    WebsiteSystemId = page.WebsiteSystemId,
                    ChannelStartDateTime = channelLink.StartDateTimeUtc ?? DateTimeOffset.MinValue,
                    ChannelEndDateTime = channelLink.EndDateTimeUtc ?? DateTimeOffset.MaxValue,
                    Permissions = permissions,
                    Blocks = GetBlocks(blocks, cultureInfo, channel.SystemId),
                    IsBrand = isBrand,
                    IsNews = isNews,
                    NewsDate = isNews ? page.Fields.GetValue<DateTimeOffset>(PageFieldNameConstants.NewsDate) : DateTimeOffset.MinValue,
                    ParentPages = GetParenPages(page)
                });
            }

            foreach(var channelData in channels)
            {
                var cultureInfo = _languageService.Get(channelData.Value)?.CultureInfo;
                if (cultureInfo is null)
                {
                    // Channel does not have a culture.
                    continue;
                }
                yield return (cultureInfo, new RemoveDocument(new PageDocument { PageSystemId = page.SystemId, ChannelSystemId = channelData.Key }));
            }
        }

        private List<PageDocument.Block> GetBlocks(List<Block> blocks, CultureInfo cultureInfo, Guid channelSystemId)
        {
            var pageBlocks = new List<PageDocument.Block>();
            foreach (var block in blocks)
            {
                var blockToChannelLink = block.ChannelLinks.FirstOrDefault(x => x.ChannelSystemId == channelSystemId);
                if (blockToChannelLink != null)
                {
                    var pageBlock = new PageDocument.Block
                    {
                        Content = _contentBuilderService.BuildContent<BlockFieldTemplate, BlockArea>(block.FieldTemplateSystemId, cultureInfo, block.Fields),
                        ChannelStartDateTime = blockToChannelLink.StartDateTimeUtc ?? DateTimeOffset.MinValue,
                        ChannelEndDateTime = blockToChannelLink.EndDateTimeUtc ?? DateTimeOffset.MaxValue,
                        Permissions = _searchPermissionService.GetPermissions(block)
                    };
                    pageBlocks.Add(pageBlock);
                }
            }

            return pageBlocks;
        }

        private List<Guid> GetParenPages(Page page)
        {
            var pageIds = new List<Guid>();
            var currentPage = page;

            while (currentPage != null && currentPage.ParentPageSystemId != Guid.Empty)
            {
                currentPage = _pageService.Get(currentPage.ParentPageSystemId);
                if (currentPage != null)
                {
                    pageIds.Add(currentPage.SystemId);
                }
            }

            return pageIds;
        }

        public override IEnumerable<IDocument> BuildRemoveIndexDocuments(IndexQueueItem item)
        {
            yield return RemoveByFieldDocument.Create<PageDocument, Guid>(x => x.PageSystemId, item.SystemId);
        }
    }
}
