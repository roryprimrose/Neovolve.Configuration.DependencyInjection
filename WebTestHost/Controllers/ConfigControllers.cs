namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("[controller]")]
public class ConfigConcreteController : ControllerBase
{
    private readonly IConfig _config;

    public ConfigConcreteController(Config config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigInterfaceController : ControllerBase
{
    private readonly IConfig _config;

    public ConfigInterfaceController(IConfig config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigMonitorInterfaceController : ControllerBase
{
    private readonly IOptionsMonitor<IConfig> _config;

    public ConfigMonitorInterfaceController(IOptionsMonitor<IConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigOptionsInterfaceController : ControllerBase
{
    private readonly IOptions<IConfig> _config;

    public ConfigOptionsInterfaceController(IOptions<IConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigSnapshotInterfaceController : ControllerBase
{
    private readonly IOptionsSnapshot<IConfig> _config;

    public ConfigSnapshotInterfaceController(IOptionsSnapshot<IConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigMonitorConcreteController : ControllerBase
{
    private readonly IOptionsMonitor<Config> _config;

    public ConfigMonitorConcreteController(IOptionsMonitor<Config> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigOptionsConcreteController : ControllerBase
{
    private readonly IOptions<Config> _config;

    public ConfigOptionsConcreteController(IOptions<Config> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ConfigSnapshotConcreteController : ControllerBase
{
    private readonly IOptionsSnapshot<Config> _config;

    public ConfigSnapshotConcreteController(IOptionsSnapshot<Config> config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config.Value;
    }
}