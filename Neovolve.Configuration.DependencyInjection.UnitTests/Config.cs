namespace Neovolve.Configuration.DependencyInjection.UnitTests;

internal interface IConfig
{
    FirstConfig First { get; set; }

    string RootValue { get; set; }
}

internal class Config : IConfig
{
    public FirstConfig First { get; set; } = new();

    public string RootValue { get; set; } = string.Empty;
}

internal interface IFirstConfig
{
    string FirstValue { get; }
    SecondConfig Second { get; }
}

internal class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;

    public SecondConfig Second { get; set; } = new();
}

internal interface ISecondConfig
{
    string SecondValue { get; }
    ThirdConfig Third { get; }
}

internal class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; } = string.Empty;

    public ThirdConfig Third { get; set; } = new();
}

internal interface IThirdConfig
{
    string ThirdValue { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; } = string.Empty;
}