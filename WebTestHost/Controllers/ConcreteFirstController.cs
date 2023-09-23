using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConcreteFirstController : ControllerBase
    {
        private readonly IFirstConfig _firstConfig;

        public ConcreteFirstController(FirstConfig firstConfig)
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