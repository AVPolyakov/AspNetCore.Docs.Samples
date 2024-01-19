#nullable enable
using NSubstitute;
using TestServices;

namespace RazorPagesProject.Tests;

public static class NSubstituteService
{
    public static T SetCurrentFor<T>() where T : class => Service<T>.Current = Substitute.For<T>();
}
