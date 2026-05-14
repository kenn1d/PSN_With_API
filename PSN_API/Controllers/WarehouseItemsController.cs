using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;

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
        /// Конструктор контроллера
        /// </summary>
        public WarehouseItemsController(DataContext dataBase)
        {
            this.dataBase = dataBase;
        }

        /// <summary>
        /// Получение записей
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список записей или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public ActionResult Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                List<Models.WarehouseItem> warehouseItems = dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).ToList();
                return Ok(warehouseItems);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="warehouseItem">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromBody] Models.WarehouseItem warehouseItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                Models.WarehouseItem existingWarehouseItem = dataBase.WarehouseItems.FirstOrDefault(x => x.id == warehouseItem.id);
                if (existingWarehouseItem == null) return NotFound();

                existingWarehouseItem.Position = warehouseItem.Position; 
                dataBase.SaveChanges();

                var Result = dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).FirstOrDefault(x => x.id == existingWarehouseItem.id);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи
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

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingWarehouseItem = dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).FirstOrDefault(x => x.id == id);
                if (existingWarehouseItem == null) return NotFound();

                dataBase.WarehouseItems.Remove(existingWarehouseItem);
                dataBase.SaveChanges();

                return Ok(existingWarehouseItem);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
