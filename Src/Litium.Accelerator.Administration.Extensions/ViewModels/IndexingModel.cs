using System.Collections.Generic;

namespace Litium.Accelerator.Administration.Extensions.ViewModel
{
    public class IndexingModel
    {
        public List<Template> Templates { get; set; }

        public string Message { get; set; }

        public class Template
        {
            public string Title { get; set; }
            public string GroupingFieldId { get; set; }
            public List<FieldGroup> Fields { get; set; }
            public string TemplateId { get; set; }
        }

        public class FieldGroup
        {
            public string Title { get; set; }
            public string FieldId { get; set; }
        }
    }
}
