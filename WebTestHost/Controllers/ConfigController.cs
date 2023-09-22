using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly Config _config;

        public ConfigController(Config config)
        {
            _config = config;
        }

        [HttpGet(Name = "GetConfig")]
        public Config Get()
        {
            return _config;
        }
    }
}