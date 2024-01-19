using Microsoft.Extensions.DependencyInjection;

namespace TestServices;

public class GenericServiceDescriptorsProvider
{
    public List<ServiceDescriptor> ServiceDescriptors { get; }

    public GenericServiceDescriptorsProvider(List<ServiceDescriptor> serviceDescriptors)
    {
        ServiceDescriptors = serviceDescriptors;
    }
}
