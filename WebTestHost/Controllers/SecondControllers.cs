namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("[controller]")]
public class SecondConcreteController : ControllerBase
{
    private readonly ISecondConfig _secondConfig;

    public SecondConcreteController(SecondConfig secondConfig)
    {
        _secondConfig = secondConfig;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _secondConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondInterfaceController : ControllerBase
{
    private readonly ISecondConfig _secondConfig;

    public SecondInterfaceController(ISecondConfig secondConfig)
    {
        _secondConfig = secondConfig;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _secondConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondMonitorInterfaceController : ControllerBase
{
    private readonly IOptionsMonitor<ISecondConfig> _config;

    public SecondMonitorInterfaceController(IOptionsMonitor<ISecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondOptionsInterfaceController : ControllerBase
{
    private readonly IOptions<ISecondConfig> _config;

    public SecondOptionsInterfaceController(IOptions<ISecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondSnapshotInterfaceController : ControllerBase
{
    private readonly IOptionsSnapshot<ISecondConfig> _config;

    public SecondSnapshotInterfaceController(IOptionsSnapshot<ISecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondMonitorConcreteController : ControllerBase
{
    private readonly IOptionsMonitor<SecondConfig> _config;

    public SecondMonitorConcreteController(IOptionsMonitor<SecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondOptionsConcreteController : ControllerBase
{
    private readonly IOptions<SecondConfig> _config;

    public SecondOptionsConcreteController(IOptions<SecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class SecondSnapshotConcreteController : ControllerBase
{
    private readonly IOptionsSnapshot<SecondConfig> _config;

    public SecondSnapshotConcreteController(IOptionsSnapshot<SecondConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _config.Value;
    }
}