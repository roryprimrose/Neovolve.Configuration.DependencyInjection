namespace TestHost;

internal interface IConfig
{
    FirstConfig First { get; set; }

    string RootValue { get; set; }
}

internal class Config : IConfig
{
    public FirstConfig First { get; set; }

    public string RootValue { get; set; }
}

internal interface IFirstConfig
{
    string FirstValue { get; }
}

internal class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; }

    public SecondConfig Second { get; set; } = new SecondConfig();
}

internal interface ISecondConfig
{
    string SecondValue { get; }
}

internal class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; }

    public ThirdConfig Third { get; set; } = new();
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

    public string ThirdValue { get; set; }
}