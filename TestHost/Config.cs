namespace TestHost;

internal interface IConfig
{
    FirstConfig First { get; set; }

    string RootValue { get; set; }
}

internal class Config : IConfig
{
    public FirstConfig First
    {
        get;
        set;
    }

    public string RootValue { get; set; }
}

internal interface IFirstConfig
{
    SecondConfig Second { get; }

    string FirstValue { get; }
}

internal class FirstConfig : IFirstConfig
{
    public SecondConfig Second
    {
        get;
        set;
    }

    public string FirstValue { get; set; }
}

internal interface ISecondConfig
{
    ThirdConfig Third { get; }

    string SecondValue { get; }
}

internal class SecondConfig : ISecondConfig
{
    public ThirdConfig Third
    {
        get;
        set;
    }

    public string SecondValue { get; set; }
}

internal interface IThirdConfig
{
    string ThirdValue { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; }
}