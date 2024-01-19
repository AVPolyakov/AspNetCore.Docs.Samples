using Microsoft.Extensions.DependencyInjection;

namespace TestServices;

public abstract class ProxyBase<TService>
{
    private readonly Lazy<TService> _originalService;

    public ProxyBase(ServiceDescriptor descriptor, IServiceProvider serviceProvider)
    {
        _originalService = new Lazy<TService>(() => (TService)serviceProvider.CreateInstance(descriptor));
    }

    public TService Service => Service<TService>.Current ?? _originalService.Value;
}
