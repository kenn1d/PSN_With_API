using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        /// <summary>
        /// Приватное поле для хранения экземпляра DataContext
        /// Используется для работы с БД
        /// </summary>
        private DataContext dataBase;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public ProductsController(DataContext dataBase) {
            this.dataBase = dataBase;
        }
    }
}
