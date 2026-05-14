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
        /// Конструктор контроллера
        /// </summary>
        public DeliveriesController(DataContext dataBase)
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

                List<Models.Delivery> deliveries = dataBase.Deliveries.ToList();
                return Ok(deliveries);
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
        /// <param name="delivery">Новый объект</param>
        /// <returns></returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.Delivery delivery)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                var existingDelivery = dataBase.Deliveries.FirstOrDefault(x => x.Serial_number == delivery.Serial_number);
                if (existingDelivery != null) return StatusCode(409);

                Models.Delivery newDelivery;
                dataBase.Deliveries.Add(newDelivery = new Models.Delivery()
                {
                    Supplier_id = UserId,
                    Serial_number = delivery.Serial_number,
                    Date = DateTime.Now,
                    Status = "В ожидании"
                });
                dataBase.SaveChanges();

                return Ok(newDelivery);
            }
            catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException Ex && Ex.Number == 1452)
            {
                return BadRequest("Ошибка: Текущий пользователь не зарегистрирован как поставщик");
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
        /// <param name="delivery">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromBody] Models.Delivery delivery)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                Models.Delivery existingDelivery = dataBase.Deliveries.FirstOrDefault(x => x.id == delivery.id);
                if (existingDelivery == null) return NotFound();

                // Если статус "Принята", то копируем поставку в таблицу склада, иначе просто обновляем статус и серийный номер
                if (delivery.Status == "Принята")
                {
                    // Получаем содержимое поставки
                    var deliveryItems = new ObservableCollection<DeliveryItem>(dataBase.DeliveryItems
                        .Where(x => x.Delivery_id == delivery.id).ToList());
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
                        dataBase.WarehouseItems.Add(newWarehouseItem);
                    }
                }
                existingDelivery.Serial_number = delivery.Serial_number;
                existingDelivery.Status = delivery.Status;
                dataBase.SaveChanges();

                return Ok(existingDelivery);
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

                var existingDelivery = dataBase.Deliveries.FirstOrDefault(x => x.id == id);
                if (existingDelivery == null) return NotFound();

                dataBase.Deliveries.Remove(existingDelivery);
                dataBase.SaveChanges();

                return Ok(existingDelivery);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
