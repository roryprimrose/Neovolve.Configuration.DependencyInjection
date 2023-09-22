using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ThirdConfigController : ControllerBase
    {
        private readonly IThirdConfig _thirdConfig;

        public ThirdConfigController(IThirdConfig thirdConfig)
        {
            _thirdConfig = thirdConfig;
        }

        [HttpGet(Name = "GetThirdConfig")]
        public IThirdConfig Get()
        {
            return _thirdConfig;
        }
    }
}