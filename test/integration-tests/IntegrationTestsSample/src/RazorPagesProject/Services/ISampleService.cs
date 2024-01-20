namespace RazorPagesProject.Services;

// <snippet1>
public interface ISampleService
{
    Task<string> GetSampleValue();
}
// </snippet1>

public class PositionOptions
{
    public const string Position = "Position";

    public string Title { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
}
