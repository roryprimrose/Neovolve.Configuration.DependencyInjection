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

    public SecondConfig Second { get; set; }
}

internal interface ISecondConfig
{
    string SecondValue { get; }
}

internal class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; }

    public ThirdConfig Third { get; set; }
}

internal interface IThirdConfig
{
    string ThirdValue { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; }
}