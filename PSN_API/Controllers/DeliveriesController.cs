using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using PSN_API.Models;
using System.Collections.ObjectModel;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveriesController : ControllerBase
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
        public DeliveriesController(DataContext dataBase, ILogger<AuthController> logger)
        {
            this.dataBase = dataBase;
            this.log = logger;
        }

        /// <summary>
        /// Получение записей поставок
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

                List<Models.Delivery> deliveries = await dataBase.Deliveries.Include(x => x.User).ToListAsync();
                return Ok(deliveries);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в Deliveries");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи поставки
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="delivery">Новый объект</param>
        /// <returns></returns>
        [Route("add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.Delivery delivery)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingDelivery = await dataBase.Deliveries.Include(x => x.User).FirstOrDefaultAsync(x => x.Serial_number == delivery.Serial_number);
                if (existingDelivery != null) return StatusCode(409);

                Models.Delivery newDelivery;
                await dataBase.Deliveries.AddAsync(newDelivery = new Models.Delivery()
                {
                    Supplier_id = UserId,
                    Serial_number = delivery.Serial_number,
                    Date = DateTime.Now,
                    Status = "В ожидании"
                });
                await dataBase.SaveChangesAsync();

                return Ok(newDelivery);
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException Ex && Ex.Number == 1452)
            {
                return BadRequest("Ошибка: Текущий пользователь не зарегистрирован как поставщик");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в Deliveries");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи поставки
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="delivery">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public async Task<ActionResult> Update([FromHeader] string token, [FromBody] Models.Delivery delivery)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                Models.Delivery existingDelivery = await dataBase.Deliveries.Include(x => x.User).FirstOrDefaultAsync(x => x.id == delivery.id);
                if (existingDelivery == null) return NotFound();

                // Если статус "Принята", то копируем поставку в таблицу склада, иначе просто обновляем статус и серийный номер
                if (delivery.Status == "Принята")
                {
                    // Получаем содержимое поставки
                    var deliveryItems = new ObservableCollection<DeliveryItem>( await dataBase.DeliveryItems
                        .Where(x => x.Delivery_id == delivery.id).ToListAsync());
                    foreach (var i in deliveryItems)
                    {
                        // Создаём новый объект на складе
                        WarehouseItem newWarehouseItem = new WarehouseItem()
                        {
                            Delivery_items_id = i.id,
                            Product_id = i.Product_id,
                            Count = i.Count,
                            Exp_date = i.Exp_date,
                            Position = "Н/Д"
                        };
                        await dataBase.WarehouseItems.AddAsync(newWarehouseItem);
                    }
                }
                existingDelivery.Serial_number = delivery.Serial_number;
                existingDelivery.Status = delivery.Status;
                await dataBase.SaveChangesAsync();

                return Ok(existingDelivery);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в Deliveries");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи поставки
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

                var existingDelivery = await dataBase.Deliveries.FirstOrDefaultAsync(x => x.id == id);
                if (existingDelivery == null) return NotFound();

                dataBase.Deliveries.Remove(existingDelivery);
                await dataBase.SaveChangesAsync();

                return Ok(existingDelivery);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в Deliveries");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
