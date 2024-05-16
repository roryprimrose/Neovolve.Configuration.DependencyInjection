namespace WebTestHost;

public interface IConfig
{
    string RootValue { get; }
}

public class RootConfig : IConfig
{
    public FirstConfig First { get; set; } = new();

    public string RootValue { get; set; } = string.Empty;
}

public interface IFirstConfig
{
    string FirstValue { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;

    public SecondConfig Second { get; set; } = new();
}

public interface ISecondConfig
{
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public string SecondValue { get; set; } = string.Empty;

    public ThirdConfig Third { get; set; } = new();
}

public interface IThirdConfig
{
    string ThirdValue { get; }

    TimeSpan Timeout { get; }
}

public class ThirdConfig : IThirdConfig
{
    public string ThirdValue { get; set; } = string.Empty;

    public TimeSpan Timeout => TimeSpan.FromSeconds(TimeoutInSeconds);
    public int TimeoutInSeconds { get; set; } = 123;
}