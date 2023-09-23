namespace WebTestHost.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class InterfaceThirdController : ControllerBase
    {
        private readonly IThirdConfig _thirdConfig;

        public InterfaceThirdController(IThirdConfig thirdConfig)
        {
            _thirdConfig = thirdConfig;
        }

        [HttpGet]
        public IThirdConfig Get()
        {
            return _thirdConfig;
        }
    }
}