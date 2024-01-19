using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TestServices;

public static class ServiceCollectionExtensions
{
    public static void DecorateByTestServices(this IServiceCollection services)
    {
        var genericServiceDescriptors = new List<ServiceDescriptor>();
        
        var newServices = services.Select(descriptor =>
            {
                if (descriptor.ServiceType.IsInterface)
                {
                    if (descriptor.ServiceType.IsGenericType)
                    {
                        genericServiceDescriptors.Add(descriptor);
                        //TODO: использовать GenericProxyBase<TService>
                        return descriptor;
                    }
                    else
                    {
                        if (IsImplemented(descriptor))
                        {
                            //TODO: рассмотреть возможно использования готовых proxy DispatchProxy и DynamicProxy , см. по ссылке
                            //https://devblogs.microsoft.com/dotnet/migrating-realproxy-usage-to-dispatchproxy/
                            return new ServiceDescriptor(descriptor.ServiceType,
                                serviceProvider => CreateProxy(descriptor, serviceProvider),
                                descriptor.Lifetime);
                        }
                    }
                }
                    
                return descriptor;
            })
            .ToList();
            
        services.Clear();
        services.AddSingleton(new GenericServiceDescriptorsProvider(genericServiceDescriptors));
        services.Add(newServices);
    }

    private static bool IsImplemented(ServiceDescriptor descriptor)
    {
        //TODO: убрать эти ограничения
        var serviceType = descriptor.ServiceType;
        return serviceType.GetInterfaces().Length == 0 &&
            serviceType.IsPublic &&
            serviceType.GetProperties(AllBindingFlags).Any() == false &&
            serviceType.GetEvents(AllBindingFlags).Any() == false &&
            serviceType.GetMethods(AllBindingFlags).All(methodInfo => methodInfo.GetParameters().Length == 0);
    }

    private static readonly ConcurrentDictionary<Type, Func<ServiceDescriptor, IServiceProvider, object>> _proxyFuncsByType = new();
    
    private static object CreateProxy(ServiceDescriptor descriptor, IServiceProvider serviceProvider)
    {
        var proxyFunc = _proxyFuncsByType.GetOrAdd(descriptor.ServiceType, GetProxyFunc);
        return proxyFunc(descriptor, serviceProvider);
    }

    private static Func<ServiceDescriptor, IServiceProvider, object> GetProxyFunc(Type serviceType)
    {
        var parent = typeof(ProxyBase<>).MakeGenericType(serviceType);
        
        var typeBuilder = _moduleBuilder.DefineType(
            name: $"{serviceType.Name}_{Guid.NewGuid()}",
            attr: TypeAttributes.Public,
            parent: parent,
            interfaces: new[] { serviceType });

        var parameterTypes = new[] { typeof(ServiceDescriptor), typeof(IServiceProvider) };
        
        var constructorBuilder = typeBuilder.DefineConstructor(
            attributes: MethodAttributes.Public,
            callingConvention: CallingConventions.Standard,
            parameterTypes: parameterTypes);

        {
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Call, parent.GetConstructor(parameterTypes)!);
            ilGenerator.Emit(OpCodes.Ret);
        }
        
        foreach (var methodInfo in serviceType.GetMethods())
        {
            const MethodAttributes getSetMethodAttributes =
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName;
            
            var parameterInfos = methodInfo.GetParameters();
            var getterMethodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                getSetMethodAttributes, methodInfo.ReturnType,
                parameterInfos.Select(x => x.ParameterType).ToArray());
            
            var ilGenerator = getterMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, parent.GetProperty("Service")!.GetGetMethod()!);
            ilGenerator.Emit(OpCodes.Callvirt, methodInfo);
            ilGenerator.Emit(OpCodes.Ret);
        }

        var type = typeBuilder.CreateType();
        
        var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(),
            returnType: typeof(object),
            parameterTypes: parameterTypes,
            restrictedSkipVisibility: true);

        {
            var ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Newobj, type.GetConstructors()[0]);
            ilGenerator.Emit(OpCodes.Ret);
        }

        return (Func<ServiceDescriptor, IServiceProvider, object>)
            dynamicMethod.CreateDelegate(typeof(Func<ServiceDescriptor, IServiceProvider, object>));
    }

    private static readonly ModuleBuilder _moduleBuilder;

    static ServiceCollectionExtensions()
    {
        var assemblyName = new AssemblyName { Name = "cae932c33f324144958b3201f81eb165" };
        _moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
    }

    private const BindingFlags AllBindingFlags = BindingFlags.Default |
        BindingFlags.IgnoreCase |
        BindingFlags.DeclaredOnly |
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.FlattenHierarchy |
        BindingFlags.InvokeMethod |
        BindingFlags.CreateInstance |
        BindingFlags.GetField |
        BindingFlags.SetField |
        BindingFlags.GetProperty |
        BindingFlags.SetProperty |
        BindingFlags.PutDispProperty |
        BindingFlags.PutRefDispProperty |
        BindingFlags.ExactBinding |
        BindingFlags.SuppressChangeType |
        BindingFlags.OptionalParamBinding |
        BindingFlags.IgnoreReturn;
}
