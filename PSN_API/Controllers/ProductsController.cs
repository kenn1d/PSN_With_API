using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using PSN_API.Classes;
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

        /// <summary>
        /// Получение записей продуктов
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список продуктов или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public ActionResult Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                List<Models.Product> products = dataBase.Products.ToList();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи нового продукта
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="product">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.Product product)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                var existingProduct = dataBase.Products.FirstOrDefault(x => x.Name == product.Name);
                if (existingProduct != null) return StatusCode(409);

                var newProduct = new Models.Product();
                dataBase.Products.Add(newProduct = new Models.Product()
                {
                    Name = product.Name
                });
                dataBase.SaveChanges();

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи продукта
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="product">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromBody] Models.Product product)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                Models.Product existProduct = dataBase.Products.FirstOrDefault(x => x.id == product.id);
                if (existProduct == null) return NotFound();

                existProduct.Name = product.Name;
                dataBase.SaveChanges();
                
                return Ok(existProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи продукта
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="id">id записи</param>
        /// <returns>Статус операции</returns>
        [Route("delete")]
        [HttpDelete]
        public ActionResult Delete([FromHeader] string token, [FromForm] int id)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                var existProduct = dataBase.Products.FirstOrDefault(x => x.id == id);
                if (existProduct == null) return NotFound();

                dataBase.Products.Remove(existProduct);
                dataBase.SaveChanges();

                return Ok(existProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
