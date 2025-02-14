using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Litium.Accelerator.Mailing
{
    /// <summary>
    /// Html mail definition
    /// </summary>
    public abstract class HtmlMailDefinition : MailDefinition
    {
        /// <summary>
        ///     Gets the body.
        /// </summary>
        /// <returns></returns>
        public abstract string Body { get; }
    }

    public abstract class HtmlMailDefinition<TModel> : HtmlMailDefinition where TModel : class
    {
        private readonly TModel _model;
        private readonly Dictionary<string, string> _dynamicValues;

        protected HtmlMailDefinition(Guid channelSystemId, TModel model, string toEmail)
        {
            ChannelSystemId = channelSystemId;
            _model = model;
            ToEmail = toEmail;

            _dynamicValues = GetDynamicValues();
        }

        public override string Body => ReplaceKeyByValue(RawBodyText, _dynamicValues);

        public override string Subject => ReplaceKeyByValue(RawSubjectText, _dynamicValues);

        public override Guid ChannelSystemId { get; }

        public override string ToEmail { get; }

        protected abstract string RawBodyText { get; } 

        protected abstract string RawSubjectText { get; }

        private Dictionary<string, string> GetDynamicValues()
        {
            var result = new Dictionary<string, string>();
            var type = typeof(TModel);
            var props = type.GetProperties().ToList();
            foreach (var propertyInfo in props)
            {
                var name = propertyInfo.Name;
                var value = Convert.ToString(propertyInfo.GetValue(_model, null));
                result.Add(BuildKeyName(name), value);
            }

            return result;
        }

        private static string BuildKeyName(string name)
        {
            return $"{{{name.ToUpper()}}}";
        }

        private static string ReplaceKeyByValue(string rawText, Dictionary<string, string> dynamicValues)
        {
            var strBuilder = new StringBuilder(rawText);

            foreach (var dynamicValue in dynamicValues)
            {
                strBuilder.Replace(dynamicValue.Key, dynamicValue.Value);
            }

            return strBuilder.ToString();
        }
    }
}
