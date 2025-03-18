using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AQUACOOLCUSTOMER_PORTAL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        public string Test()
        {
            return "working.";
        }
    }
}
