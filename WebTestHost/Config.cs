namespace WebTestHost;

public interface IConfig
{
    string RootValue { get; }
}

public class Config : IConfig
{
    public FirstConfig First { get; } = new();

    public string RootValue { get; set; }
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; }

    public SecondConfig Second { get; } = new();
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; }

    public ThirdConfig Third { get; } = new();
}

public interface IThirdConfig
{
    string ThirdValue { get; }
}

public class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; }
}