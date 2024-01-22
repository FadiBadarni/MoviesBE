using Microsoft.AspNetCore.Mvc;

namespace MoviesBE.Controllers
{
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        [Route("/message")]
        public string GetMessage()
        {
            return "Hey You!";
        }
    }
}