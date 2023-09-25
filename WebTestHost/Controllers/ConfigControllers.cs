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