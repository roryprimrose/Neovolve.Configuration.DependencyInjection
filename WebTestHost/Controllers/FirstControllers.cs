namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("[controller]")]
public class FirstConcreteController : ControllerBase
{
    private readonly IFirstConfig _firstConfig;

    public FirstConcreteController(FirstConfig firstConfig)
    {
        _firstConfig = firstConfig;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _firstConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstInterfaceController : ControllerBase
{
    private readonly IFirstConfig _firstConfig;

    public FirstInterfaceController(IFirstConfig firstConfig)
    {
        _firstConfig = firstConfig;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _firstConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstMonitorInterfaceController : ControllerBase
{
    private readonly IOptionsMonitor<IFirstConfig> _config;

    public FirstMonitorInterfaceController(IOptionsMonitor<IFirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstOptionsInterfaceController : ControllerBase
{
    private readonly IOptions<IFirstConfig> _config;

    public FirstOptionsInterfaceController(IOptions<IFirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstSnapshotInterfaceController : ControllerBase
{
    private readonly IOptionsSnapshot<IFirstConfig> _config;

    public FirstSnapshotInterfaceController(IOptionsSnapshot<IFirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstMonitorConcreteController : ControllerBase
{
    private readonly IOptionsMonitor<FirstConfig> _config;

    public FirstMonitorConcreteController(IOptionsMonitor<FirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstOptionsConcreteController : ControllerBase
{
    private readonly IOptions<FirstConfig> _config;

    public FirstOptionsConcreteController(IOptions<FirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class FirstSnapshotConcreteController : ControllerBase
{
    private readonly IOptionsSnapshot<FirstConfig> _config;

    public FirstSnapshotConcreteController(IOptionsSnapshot<FirstConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IFirstConfig Get()
    {
        return _config.Value;
    }
}