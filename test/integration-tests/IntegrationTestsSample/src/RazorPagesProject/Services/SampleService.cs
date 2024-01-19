using Microsoft.Extensions.Options;

namespace RazorPagesProject.Services;

// <snippet1>
// Quote ©1975 BBC: The Doctor (Tom Baker); Dr. Who: Planet of Evil
// https://www.bbc.co.uk/programmes/p00pyrx6
public class SampleService : ISampleService
{
    private readonly IOptions<PositionOptions> _options;

    public SampleService(IOptions<PositionOptions> options)
    {
        _options = options;
    }

    public Task<string> GenerateQuote()
    {
        return Task.FromResult(_options.Value.Name);
    }
}
// </snippet1>
