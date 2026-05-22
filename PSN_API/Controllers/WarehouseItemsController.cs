using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using PSN_API.Models;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseItemsController : ControllerBase
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
        public WarehouseItemsController(DataContext dataBase, ILogger<AuthController> log)
        {
            this.dataBase = dataBase;
            this.log = log;
        }

        /// <summary>
        /// Получение записей продуктов на складе
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список записей или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public async Task<ActionResult> Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                List<Models.WarehouseItem> warehouseItems = await dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).ToListAsync();
                return Ok(warehouseItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в WarehouseItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи продукта на складе
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="warehouseItem">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public async Task<ActionResult> Update([FromHeader] string token, [FromBody] Models.WarehouseItem warehouseItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                Models.WarehouseItem existingWarehouseItem = await dataBase.WarehouseItems.FirstOrDefaultAsync(x => x.id == warehouseItem.id);
                if (existingWarehouseItem == null) return NotFound();

                existingWarehouseItem.Position = warehouseItem.Position;
                await dataBase.SaveChangesAsync();

                var Result = await dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).FirstOrDefaultAsync(x => x.id == existingWarehouseItem.id);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в WarehouseItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи продукта на складе
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
                if (UserRole == "Supplier" && UserRole == "worker") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingWarehouseItem = await dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).FirstOrDefaultAsync(x => x.id == id);
                if (existingWarehouseItem == null) return NotFound();

                var existingShopItem = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync();
                if (existingShopItem != null) return BadRequest("Ошибка: Этот товар находится в продаже");

                dataBase.WarehouseItems.Remove(existingWarehouseItem);
                await dataBase.SaveChangesAsync();

                return Ok(existingWarehouseItem);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в WarehouseItems");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
