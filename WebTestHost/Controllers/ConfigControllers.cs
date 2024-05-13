namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ConfigConcreteController : ControllerBase
{
    private readonly IConfig _rootConfig;

    public ConfigConcreteController(RootConfig rootConfig)
    {
        _rootConfig = rootConfig;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _rootConfig;
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