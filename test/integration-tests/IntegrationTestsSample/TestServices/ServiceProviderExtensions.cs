using Microsoft.Extensions.DependencyInjection;

namespace TestServices;

public static class ServiceProviderExtensions
{
    public static object CreateInstance(this IServiceProvider serviceProvider, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance != null)
            return descriptor.ImplementationInstance;

        if (descriptor.ImplementationFactory != null)
            return descriptor.ImplementationFactory(serviceProvider);

        return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, descriptor.ImplementationType!);
    }
}
