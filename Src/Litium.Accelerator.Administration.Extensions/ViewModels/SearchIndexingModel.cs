using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Administration.Extensions.ViewModel
{
    public class SearchIndexingModel
    {
        public List<TemplateGroup> GroupedTemplates { get; set; }

        public string Message { get; set; }

        public class Template
        {
            public string Title { get; set; }
            public List<FieldGroup> Fields { get; set; }
            public List<string> SelectedFields { get; set; } = new List<string>();
            public string TemplateId { get; set; }
            public Type AreaType { get; internal set; }
        }

        public class FieldGroup
        {
            public string Title { get; set; }
            public string FieldId { get; set; }
        }

        public class TemplateGroup
        {
            public string Title { get; set; }
            public List<Template> Templates { get; set; }
        }
    }
}
