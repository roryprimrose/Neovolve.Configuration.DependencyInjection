using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FirstConfigController : ControllerBase
    {
        private readonly IFirstConfig _firstConfig;

        public FirstConfigController(IFirstConfig firstConfig)
        {
            _firstConfig = firstConfig;
        }

        [HttpGet(Name = "GetFirstConfig")]
        public IFirstConfig Get()
        {
            return _firstConfig;
        }
    }
}