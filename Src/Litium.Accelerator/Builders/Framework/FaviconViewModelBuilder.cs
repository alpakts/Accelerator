using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Litium.Accelerator.ViewModels.Framework;
using Litium.Media;
using Litium.Web.Media;

namespace Litium.Accelerator.Builders.Framework
{
    public class FaviconViewModelBuilder : IViewModelBuilder<FaviconViewModel>
    {
        private readonly FileService _fileService;
        private readonly MediaLocationService _mediaLocationService;

        public FaviconViewModelBuilder(FileService fileService, MediaLocationService mediaLocationService)
        {
            _fileService = fileService;
            _mediaLocationService = mediaLocationService;
        }

        public List<FaviconViewModel> Build(Guid faviconId)
        {
            var favicons = new List<FaviconViewModel>
            {
                CreateFavicon(faviconId, FaviconViewModel.RelType.AppleTouchIcon, 180),
                CreateFavicon(faviconId, FaviconViewModel.RelType.Icon, 192),
                CreateFavicon(faviconId, FaviconViewModel.RelType.Icon, 96),
                CreateFavicon(faviconId, FaviconViewModel.RelType.Icon, 32),
                CreateFavicon(faviconId, FaviconViewModel.RelType.Icon, 16),
                CreateFavicon(faviconId, FaviconViewModel.RelType.ShortcutIcon, 16)
            };
            return favicons;
        }

        private FaviconViewModel CreateFavicon(Guid faviconId, FaviconViewModel.RelType relType, int size)
        {
            var icon = new FaviconViewModel(relType)
            {
                Href = LoadImage(faviconId, relType, size, out var newSize),
                Size = newSize,
                Rel = GetDisplayValue(relType)
            };
            return icon;
        }

        private string LoadImage(Guid imageId, FaviconViewModel.RelType relType, int size, out string newSize)
        {
            newSize = string.Empty;
            var href = string.Empty;
            var file = _fileService.Get(imageId);

            if (file == null)
            {
                return href;
            }
            
            var resizeToFormat = relType == FaviconViewModel.RelType.ShortcutIcon ? ImageFormat.Icon : ImageFormat.Png;
            var newSizeVal = file.GetCalculatedSize(new Size(size, size));
            href = _mediaLocationService.GetLocation<File>(file.SystemId, file.BlobUri, file.Name, resizeToFormat, newSizeVal, true, false);
            newSize = $"{newSizeVal.Width}x{newSizeVal.Height}";
            return href;
        }

        private string GetDisplayValue(FaviconViewModel.RelType value)
        {
            if (!(typeof(FaviconViewModel.RelType).GetField(value.ToString())
                .GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] displayAttributeArray))
            {
                return string.Empty;
            }
            return displayAttributeArray.Length == 0 ? value.ToString() : displayAttributeArray[0].Name;
        }
    }
}
