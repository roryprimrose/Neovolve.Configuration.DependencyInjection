namespace WebTestHost;

public interface IConfig
{
    string RootValue { get; }
}

public class Config : IConfig
{
    public IFirstConfig First
    {
        get;
    } = new FirstConfig();

    public string RootValue { get; set; }
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public ISecondConfig Second
    {
        get;
    } = new SecondConfig();

    public string FirstValue { get; set; }
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public IThirdConfig Third
    {
        get;
    } = new ThirdConfig();

    public string SecondValue
    {
        get;
        set;
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