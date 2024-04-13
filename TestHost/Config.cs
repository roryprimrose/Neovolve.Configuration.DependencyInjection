namespace TestHost;

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
}

internal class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;

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

    TimeSpan Timeout { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public int TimeoutInSeconds { get; set; } = 123;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutInSeconds);

    public string ThirdValue { get; set; } = string.Empty;
}