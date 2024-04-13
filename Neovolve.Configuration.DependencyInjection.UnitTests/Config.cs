namespace Neovolve.Configuration.DependencyInjection.UnitTests;

internal interface IConfig
{
    string RootValue { get; set; }
}

internal class Config : IConfig
{
    public FirstConfig First { get; } = new();

    public string RootValue { get; set; } = string.Empty;
}

internal interface IFirstConfig
{
    string FirstValue { get; }

    Guid Id { get; }
}

internal class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public SecondConfig Second { get; } = new();
}

internal interface ISecondConfig
{
    string SecondValue { get; }
}

internal class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; } = string.Empty;

    public ThirdConfig Third { get; } = new();
}

internal interface IThirdConfig
{
    string ThirdValue { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; } = string.Empty;
}