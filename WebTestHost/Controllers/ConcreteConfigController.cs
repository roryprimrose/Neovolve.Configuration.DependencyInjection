using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConcreteConfigController : ControllerBase
    {
        private readonly IConfig _config;

        public ConcreteConfigController(Config config)
        {
            _config = config;
        }

        [HttpGet]
        public IConfig Get()
        {
            return _config;
        }
    }
}