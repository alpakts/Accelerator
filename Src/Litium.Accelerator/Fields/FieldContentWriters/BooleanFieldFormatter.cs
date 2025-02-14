using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Fields.FieldContentWriters
{
    /// <summary>
    ///     Writes contents of a boolean field.
    /// </summary>
    [Service(Name = SystemFieldTypeConstants.Boolean)]
    internal class BooleanFieldFormatter : FieldFormatter
    {
        /// <summary>
        /// Converts the value of a specified object to an equivalent string representation using specified format and
        /// culture-specific formatting information.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public override string Format(IFieldDefinition fieldDefinition, object item, FieldFormatArgs args)
        {
            args.ContentType = "text/boolean";

            if (!(item is bool))
                return string.Empty;

            return (args.Culture != null) ? ((bool)item).ToString(args.Culture) : ((bool)item).ToString();
        }
    }
}
