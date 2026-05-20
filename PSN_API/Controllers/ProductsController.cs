using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        /// Экземпляр Serilog
        /// </summary>
        private readonly ILogger<AuthController> log;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public ProductsController(DataContext dataBase, ILogger<AuthController> logger) {
            this.dataBase = dataBase;
            this.log = logger;
        }

        /// <summary>
        /// Получение записей продуктов
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список продуктов или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "worker") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                List<Models.Product> products = await dataBase.Products.ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в Products");

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
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.Product product)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingProduct = await dataBase.Products.FirstOrDefaultAsync(x => x.Name == product.Name);
                if (existingProduct != null) return StatusCode(409);

                var newProduct = new Models.Product();
                await dataBase.Products.AddAsync(newProduct = new Models.Product()
                {
                    Name = product.Name
                });
                await dataBase.SaveChangesAsync();

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в Products");

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
        public async Task<ActionResult> Update([FromHeader] string token, [FromBody] Models.Product product)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                Models.Product existProduct = await dataBase.Products.FirstOrDefaultAsync(x => x.id == product.id);
                if (existProduct == null) return NotFound();

                existProduct.Name = product.Name;
                await dataBase.SaveChangesAsync();
                
                return Ok(existProduct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в Products");

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
        public async Task<ActionResult> Delete([FromHeader] string token, [FromForm] int id)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existProduct = await dataBase.Products.FirstOrDefaultAsync(x => x.id == id);
                if (existProduct == null) return NotFound();

                dataBase.Products.Remove(existProduct);
                await dataBase.SaveChangesAsync();

                return Ok(existProduct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в Products");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
