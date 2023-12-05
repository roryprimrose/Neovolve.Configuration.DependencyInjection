namespace WebTestHost;

public interface IConfig
{
    string RootValue { get; }
}

public class Config : IConfig
{
    public FirstConfig First { get; set; } = new();

    public string RootValue { get; set; }
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; }

    public SecondConfig Second { get; set; } = new();
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; }

    public ThirdConfig Third { get; set; } = new();
}

public interface IThirdConfig
{
    string ThirdValue { get; }

    TimeSpan Timeout { get; }
}

public class ThirdConfig : IThirdConfig
{
    public int TimeoutInSeconds { get; set; } = 123;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutInSeconds);

    public string ThirdValue { get; set; }
}