using Microsoft.AspNetCore.Mvc;

namespace WebTestHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConcreteSecondController : ControllerBase
    {
        private readonly ISecondConfig _secondConfig;

        public ConcreteSecondController(SecondConfig secondConfig)
        {
            _secondConfig = secondConfig;
        }

        [HttpGet]
        public ISecondConfig Get()
        {
            return _secondConfig;
        }
    }
}