using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InterfaceFirstController : ControllerBase
    {
        private readonly IFirstConfig _firstConfig;

        public InterfaceFirstController(IFirstConfig firstConfig)
        {
            _firstConfig = firstConfig;
        }

        [HttpGet]
        public IFirstConfig Get()
        {
            return _firstConfig;
        }
    }
}