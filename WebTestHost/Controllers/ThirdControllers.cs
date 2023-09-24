namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

[ApiController]
[Route("[controller]")]
public class ThirdConcreteController : ControllerBase
{
    private readonly IThirdConfig _thirdConfig;

    public ThirdConcreteController(ThirdConfig thirdConfig)
    {
        _thirdConfig = thirdConfig;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _thirdConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdInterfaceController : ControllerBase
{
    private readonly IThirdConfig _thirdConfig;

    public ThirdInterfaceController(IThirdConfig thirdConfig)
    {
        _thirdConfig = thirdConfig;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _thirdConfig;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdMonitorInterfaceController : ControllerBase
{
    private readonly IOptionsMonitor<IThirdConfig> _config;

    public ThirdMonitorInterfaceController(IOptionsMonitor<IThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdOptionsInterfaceController : ControllerBase
{
    private readonly IOptions<IThirdConfig> _config;

    public ThirdOptionsInterfaceController(IOptions<IThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdSnapshotInterfaceController : ControllerBase
{
    private readonly IOptionsSnapshot<IThirdConfig> _config;

    public ThirdSnapshotInterfaceController(IOptionsSnapshot<IThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdMonitorConcreteController : ControllerBase
{
    private readonly IOptionsMonitor<ThirdConfig> _config;

    public ThirdMonitorConcreteController(IOptionsMonitor<ThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.CurrentValue;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdOptionsConcreteController : ControllerBase
{
    private readonly IOptions<ThirdConfig> _config;

    public ThirdOptionsConcreteController(IOptions<ThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.Value;
    }
}

[ApiController]
[Route("[controller]")]
public class ThirdSnapshotConcreteController : ControllerBase
{
    private readonly IOptionsSnapshot<ThirdConfig> _config;

    public ThirdSnapshotConcreteController(IOptionsSnapshot<ThirdConfig> config)
    {
        _config = config;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _config.Value;
    }
}