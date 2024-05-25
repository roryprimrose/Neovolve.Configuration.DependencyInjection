namespace WebTestHost;

using System.Collections.ObjectModel;

public interface IConfig
{
    string RootValue { get; set; }
}

public class RootConfig : IConfig
{
    public FirstConfig First { get; } = new();

    public string RootValue { get; set; } = string.Empty;
}

public interface IFirstConfig
{
    string FirstValue { get; }

    Guid Id { get; }
}

public class FirstConfig : IFirstConfig
{
    public string FirstValue { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public SecondConfig Second { get; set; } = new();
}

public interface ISecondConfig
{
    Collection<string> MoreValues { get; }
    string SecondValue { get; }
}

public class SecondConfig : ISecondConfig
{
    public Collection<string> MoreValues { get; set; } = new();
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