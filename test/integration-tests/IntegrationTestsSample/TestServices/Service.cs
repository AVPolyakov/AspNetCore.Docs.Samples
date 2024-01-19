namespace TestServices;

public static class Service<T>
{
    private static readonly AsyncLocal<T?> _current = new();

    public static T? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
