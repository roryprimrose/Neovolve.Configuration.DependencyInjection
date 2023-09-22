namespace WebTestHost;

public interface IConfig
{
    FirstConfig First { get; set; }

    string RootValue { get; set; }
}

public class Config : IConfig
{
    public FirstConfig First
    {
        get;
        set;
    }

    public string RootValue { get; set; }
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public SecondConfig Second
    {
        get;
        set;
    }

    public string FirstValue { get; set; }
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
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

public interface IThirdConfig
{
    string ThirdValue { get; }
}

public class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; }
}