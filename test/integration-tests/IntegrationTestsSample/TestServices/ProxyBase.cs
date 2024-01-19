using Microsoft.Extensions.DependencyInjection;

namespace TestServices;

public abstract class ProxyBase<TService>
{
    private readonly Lazy<TService> _lazy;

    public ProxyBase(ServiceDescriptor descriptor, IServiceProvider serviceProvider)
    {
        _lazy = new Lazy<TService>(() => (TService)serviceProvider.CreateInstance(descriptor));
    }

    public TService Service => Service<TService>.Current ?? _lazy.Value;
}
