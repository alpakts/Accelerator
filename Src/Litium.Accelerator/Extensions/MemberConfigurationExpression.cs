using AutoMapper;
using Litium.Web.Models;

namespace Litium.Accelerator.Extensions
{
    public static class MemberConfigurationExpression
    {
        public static void MapFromField<TSource, TDestination, TMember>(this IMemberConfigurationExpression<TSource, TDestination, TMember> config, string fieldId) where TSource : FieldFrameworkModel
        {
            config.MapFrom(model => model.GetValue<TMember>(fieldId));
        }
    }
}
