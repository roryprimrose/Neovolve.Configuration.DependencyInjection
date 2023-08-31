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
    string SecondValue { get; }
}

internal class SecondConfig : ISecondConfig
{
    private string _secondValue;

    public ThirdConfig Third
    {
        get;
        set;
    }

    public string SecondValue
    {
        get => _secondValue;
        set
        {
            //if (_secondValue != null)
            //{
            //    throw new InvalidOperationException("haha");
            //}

            _secondValue = value;
        }
    }
}

internal interface IThirdConfig
{
    string ThirdValue { get; }
}

internal class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; }
}