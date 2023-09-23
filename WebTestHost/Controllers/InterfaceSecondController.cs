namespace WebTestHost.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class InterfaceSecondController : ControllerBase
{
    private readonly ISecondConfig _secondConfig;

    public InterfaceSecondController(ISecondConfig secondConfig)
    {
        _secondConfig = secondConfig;
    }

    [HttpGet]
    public ISecondConfig Get()
    {
        return _secondConfig;
    }
}