using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryItemsController : ControllerBase
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
        public DeliveryItemsController(DataContext dataBase, ILogger<AuthController> logger)
        {
            this.dataBase = dataBase;
            this.log = logger;
        }

        /// <summary>
        /// Получение записей элементов поставок
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список поставок или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                List<Models.DeliveryItem> deliveryItems = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).ToListAsync();
                return Ok(deliveryItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в DeliveryItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи элемента поставки
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="deliveryItem">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.DeliveryItem deliveryItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                // Проверяем есть ли уже элемент такой поставки с таким же продуктом
                // Если true то суммируем количество - обновляем запись
                var existingDeliveryItem = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product)
                    .FirstOrDefaultAsync(x => x.Delivery_id == deliveryItem.Delivery_id && x.Product_id == deliveryItem.Product_id);
                if (existingDeliveryItem != null) 
                {
                    existingDeliveryItem.Count += deliveryItem.Count;
                    await dataBase.SaveChangesAsync();
                    var Result = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefaultAsync(x => x.id == existingDeliveryItem.id);
                    return Ok(Result);
                }
                else // Создаём новую запись
                {
                    Models.DeliveryItem newDeliveryItem;
                    await dataBase.DeliveryItems.AddAsync(newDeliveryItem = new Models.DeliveryItem()
                    {
                        Delivery_id = deliveryItem.Delivery_id,
                        Product_id = deliveryItem.Product_id,
                        Count = deliveryItem.Count,
                        Exp_date = deliveryItem.Exp_date
                    });
                    await dataBase.SaveChangesAsync();
                    var Result = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefaultAsync(x => x.id == newDeliveryItem.id);
                    return Ok(Result);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в DeliveryItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи элемента поставки
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="deliveryItem">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public async Task<ActionResult> Update([FromHeader] string token, [FromBody] Models.DeliveryItem deliveryItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                Models.DeliveryItem existingDeliveryItem = await dataBase.DeliveryItems.FirstOrDefaultAsync(x => x.id == deliveryItem.id);
                if (existingDeliveryItem == null) return NotFound();

                existingDeliveryItem.Delivery_id = deliveryItem.Delivery_id;
                existingDeliveryItem.Product_id = deliveryItem.Product_id;
                existingDeliveryItem.Count = deliveryItem.Count;
                existingDeliveryItem.Exp_date = deliveryItem.Exp_date;
                await dataBase.SaveChangesAsync();

                var Result = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefaultAsync(x => x.id == existingDeliveryItem.id);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в DeliveryItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи элемента поставки
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
                if (UserRole != "Supplier" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingDeliveryItem = await dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefaultAsync(x => x.id == id);
                if (existingDeliveryItem == null) return NotFound();

                dataBase.DeliveryItems.Remove(existingDeliveryItem);
                await dataBase.SaveChangesAsync();

                return Ok(existingDeliveryItem);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в DeliveryItems");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
