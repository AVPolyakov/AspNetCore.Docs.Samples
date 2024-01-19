using Microsoft.Extensions.DependencyInjection;

namespace TestServices;

public static class GenericProxy
{
    public const int IndexOffset = 1000;
}

public abstract class GenericProxyBase<TService>
{
    private readonly Lazy<TService> _originalService;

    protected GenericProxyBase(IServiceProvider serviceProvider, int index)
    {
        _originalService = new Lazy<TService>(() =>
        {
            var serviceDescriptorsProvider = serviceProvider.GetRequiredService<GenericServiceDescriptorsProvider>();
            var serviceDescriptor = serviceDescriptorsProvider.ServiceDescriptors[index - GenericProxy.IndexOffset];
            var type = serviceDescriptor.ImplementationType!.MakeGenericType(typeof(TService).GenericTypeArguments);
            return (TService)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type);
        });
    }

    protected TService Service => Service<TService>.Current ?? _originalService.Value;
}
