using JetBrains.Annotations;
using Litium.FieldFramework;
using Litium.Runtime.DependencyInjection;

namespace Litium.Accelerator.Fields
{
    /// <summary>
    /// When implemented, formats the content of the field to its string representation.
    /// </summary>
    [Service(ServiceType = typeof(FieldFormatter), NamedService = true)]
    public abstract class FieldFormatter
    {
        /// <summary>
        /// Formats the specified field definition.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="item">The item.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public abstract string Format(IFieldDefinition fieldDefinition, object item, [NotNull] FieldFormatArgs args);
    }
}
