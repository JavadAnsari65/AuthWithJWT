using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sample3.Filter;

namespace sample3.Controller2
{
    [Route("[Action]")]
    [ApiController]
    public class HomeWithAttributesController : ControllerBase
    {
        [HttpGet]
        [Permission("AddProduct")]
        public string AddProduct2()
        {
            return "Hello World";
        }

        [HttpGet]
        [Permission("GetProduct")]
        public string GetProduct2()
        {
            return "Hello World";
        }

        [HttpGet]
        [Permission("UpdateProduct")]
        public string UpdateProduct2()
        {
            return "Hello World";
        }

        [HttpGet]
        [Permission("DeleteProduct")]
        public string DeleteProduct2()
        {
            return "Hello World";
        }
    }
}
