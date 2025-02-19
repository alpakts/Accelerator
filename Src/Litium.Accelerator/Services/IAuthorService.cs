using System.Collections.Generic;
using System;

namespace Litium.Accelerator.Services;

[Litium.Runtime.DependencyInjection.Service(
    ServiceType = typeof(IAuthorService),
    Lifetime = Litium.Runtime.DependencyInjection.DependencyLifetime.Scoped)]
public interface IAuthorService
{
    List<string> GetBooksByAuthor(Guid authorPageId);
}

