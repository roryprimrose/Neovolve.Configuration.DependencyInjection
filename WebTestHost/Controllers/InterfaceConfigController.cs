namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class InterfaceConfigController : ControllerBase
{
    private readonly IConfig _config;

    public InterfaceConfigController(IConfig config)
    {
        _config = config;
    }

    [HttpGet]
    public IConfig Get()
    {
        return _config;
    }
}