using Litium.Accelerator.Builders.Block;
using Litium.Web.Models.Blocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Litium.Accelerator.Constants;  // BlockFieldNameConstants için
using Litium.FieldFramework.FieldTypes;  // PointerPageItem için
using Litium.Web.Models;  // LinkModel için

namespace Litium.Accelerator.Mvc.Controllers.Blocks
{
    public class AuthorBlockController : ViewComponent
    {
        private readonly AuthorBlockViewModelBuilder _builder;
        private readonly ILogger<AuthorBlockController> _logger;

        public AuthorBlockController(
            AuthorBlockViewModelBuilder builder,
            ILogger<AuthorBlockController> logger)
        {
            _builder = builder;
            _logger = logger;
        }

        public IViewComponentResult Invoke(BlockModel currentBlockModel)
        {
            _logger.LogInformation("Field values:");
            _logger.LogInformation($"Title: {currentBlockModel.Fields.GetValue<string>(BlockFieldNameConstants.BlockTitle)}");
            _logger.LogInformation($"Text: {currentBlockModel.Fields.GetValue<string>(BlockFieldNameConstants.BlockText)}");
            _logger.LogInformation($"Image: {currentBlockModel.Fields.GetValue<Guid>(BlockFieldNameConstants.BlockImagePointer)}");
            _logger.LogInformation($"LinkText: {currentBlockModel.Fields.GetValue<string>(BlockFieldNameConstants.LinkText)}");
            _logger.LogInformation($"Link: {currentBlockModel.Fields.GetValue<PointerPageItem>(BlockFieldNameConstants.Link)}");

            var model = _builder.Build(currentBlockModel);
            
            // Log model values
            _logger.LogInformation("Author block model values:");
            _logger.LogInformation($"- Title: '{model.Title}'");
            _logger.LogInformation($"- Text: '{model.Text}'");
            _logger.LogInformation($"- HasImage: {model.Image != null}");
            _logger.LogInformation($"- LinkText: '{model.LinkText}'");
            _logger.LogInformation($"- LinkUrl: '{model.LinkUrl}'");
            
            return View("~/Views/Blocks/Author.cshtml", model);
        }
    }
} 