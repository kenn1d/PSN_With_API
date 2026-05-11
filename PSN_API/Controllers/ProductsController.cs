using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private DataContext dataBase;

        public ProductsController(DataContext dataBase) { 
            this.dataBase = dataBase;
        }


    }
}
