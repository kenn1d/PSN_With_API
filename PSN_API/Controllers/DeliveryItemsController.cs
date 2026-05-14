using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using PSN_API.Classes;
using PSN_API.Data;
using PSN_API.Models;
using System.Collections.ObjectModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        /// Конструктор контроллера
        /// </summary>
        public DeliveryItemsController(DataContext dataBase)
        {
            this.dataBase = dataBase;
        }

        /// <summary>
        /// Получение записей поставок
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список поставок или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public ActionResult Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                List<Models.DeliveryItem> deliveryItems = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).ToList();
                return Ok(deliveryItems);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="deliveryItem">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.DeliveryItem deliveryItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                // Проверяем есть ли уже элемент такой поставки с таким же продуктом
                // Если true то суммируем количество - обновляем запись
                var existingDeliveryItem = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product)
                    .FirstOrDefault(x => x.Delivery_id == deliveryItem.Delivery_id && x.Product_id == deliveryItem.Product_id);
                if (existingDeliveryItem != null) 
                {
                    existingDeliveryItem.Count += deliveryItem.Count;
                    dataBase.SaveChanges();
                    var Result = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefault(x => x.id == existingDeliveryItem.id);
                    return Ok(Result);
                }
                else // Создаём новую запись
                {
                    Models.DeliveryItem newDeliveryItem;
                    dataBase.DeliveryItems.Add(newDeliveryItem = new Models.DeliveryItem()
                    {
                        Delivery_id = deliveryItem.Delivery_id,
                        Product_id = deliveryItem.Product_id,
                        Count = deliveryItem.Count,
                        Exp_date = deliveryItem.Exp_date
                    });
                    dataBase.SaveChanges();
                    var Result = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefault(x => x.id == newDeliveryItem.id);
                    return Ok(Result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи поставки
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="deliveryItem">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromBody] Models.DeliveryItem deliveryItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                Models.DeliveryItem existingDeliveryItem = dataBase.DeliveryItems.FirstOrDefault(x => x.id == deliveryItem.id);
                if (existingDeliveryItem == null) return NotFound();

                existingDeliveryItem.Delivery_id = deliveryItem.Delivery_id;
                existingDeliveryItem.Product_id = deliveryItem.Product_id;
                existingDeliveryItem.Count = deliveryItem.Count;
                existingDeliveryItem.Exp_date = deliveryItem.Exp_date;
                dataBase.SaveChanges();

                var Result = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefault(x => x.id == existingDeliveryItem.id);
                return Ok(Result);
            }
            catch (Exception ex)
            {
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
        public ActionResult Delete([FromHeader] string token, [FromForm] int id)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingDeliveryItem = dataBase.DeliveryItems.Include(x => x.Delivery).Include(x => x.Product).FirstOrDefault(x => x.id == id);
                if (existingDeliveryItem == null) return NotFound();

                dataBase.DeliveryItems.Remove(existingDeliveryItem);
                dataBase.SaveChanges();

                return Ok(existingDeliveryItem);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
