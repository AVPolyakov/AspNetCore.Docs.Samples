using Microsoft.Extensions.Options;

namespace RazorPagesProject.Tests.IntegrationTests;

public class TestOptions<TOptions> : IOptions<TOptions> where TOptions : class
{
    public TestOptions(TOptions value) => Value = value;

    public TOptions Value { get; }
}
