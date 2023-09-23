namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ConcreteThirdController : ControllerBase
{
    private readonly IThirdConfig _thirdConfig;

    public ConcreteThirdController(ThirdConfig thirdConfig)
    {
        _thirdConfig = thirdConfig;
    }

    [HttpGet]
    public IThirdConfig Get()
    {
        return _thirdConfig;
    }
}