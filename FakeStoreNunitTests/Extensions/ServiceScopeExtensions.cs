using Microsoft.Extensions.DependencyInjection;

namespace Api.XUnit.Tests.Extensions;

public static class ServiceScopeExtensions
{
    public static T GetService<T>(this IServiceScope scope)
        where T : class
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
