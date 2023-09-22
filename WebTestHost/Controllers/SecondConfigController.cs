using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    using Microsoft.Extensions.Options;

    [ApiController]
    [Route("[controller]")]
    public class SecondConfigController : ControllerBase
    {
        private readonly ISecondConfig _secondConfig;

        public SecondConfigController(ISecondConfig secondConfig)
        {
            _secondConfig = secondConfig;
        }

        [HttpGet(Name = "GetSecondConfig")]
        public ISecondConfig Get()
        {
            return _secondConfig;
        }
    }
}