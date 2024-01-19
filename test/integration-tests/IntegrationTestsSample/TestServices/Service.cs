namespace TestServices;

public static class Service
{
    public static TService SetCurrent<TService>(TService service) => Service<TService>.Current = service;
}

public static class Service<TService>
{
    private static readonly AsyncLocal<TService?> _current = new();

    public static TService? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
