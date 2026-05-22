using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopItemsController : ControllerBase
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
        public ShopItemsController(DataContext dataBase, ILogger<AuthController> log)
        {
            this.dataBase = dataBase;
            this.log = log;
        }

        /// <summary>
        /// Получение записей продуктов на продаже
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

                List<Models.ShopItem> shopItems = await dataBase.ShopItems.Include(x => x.WarehouseItem).ToListAsync();
                return Ok(shopItems);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в ShopItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи продукта на продаже
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="shopItem">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.ShopItem shopItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                // Проверяем есть ли уже элемент такой позицией
                // Если true то изменяем количество - обновляем запись
                var warehouseItem = await dataBase.WarehouseItems.Include(x => x.Product).Include(x => x.DeliveryItem).ThenInclude(x => x.Delivery).FirstAsync(x => x.id == shopItem.Warehouse_item_id);
                var itemPositionOnWarehouse = warehouseItem.Position;

                var existingShopItem = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.WarehouseItem.Position == itemPositionOnWarehouse); 
                // Ищем этот же товар на складе
                var WI = await dataBase.WarehouseItems.FirstOrDefaultAsync(x => x.id == shopItem.Warehouse_item_id);
                if (existingShopItem != null)
                {
                    // Cчитаем разницу между тем что было и тем что ввели, 0 - если значение null
                    int diff = (shopItem.Count ?? 0) - (existingShopItem.Count ?? 0);
                    if (WI.Count >= diff)
                    {
                        WI.Count -= diff; // Если diff > 0, забираем со склада, иначе возвращаем на склад
                        existingShopItem.Count = shopItem.Count;
                    }
                    else
                    {
                        return BadRequest("Ошибка: На складе недостаточно товара");
                    }
                    
                    await dataBase.SaveChangesAsync();
                    var Result = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == existingShopItem.id);
                    return Ok(Result);
                }
                else // Создаём новую запись
                {
                    // Обновляем остатки
                    if (WI != null)
                    {
                        if (WI.Count >= (int)shopItem.Count)
                        {
                            WI.Count -= (int)shopItem.Count;

                            Models.ShopItem newShopItem;
                            await dataBase.ShopItems.AddAsync(newShopItem = new Models.ShopItem()
                            {
                                Warehouse_item_id = shopItem.Warehouse_item_id,
                                Count = shopItem.Count
                            });
                        }
                        else
                        {
                            return BadRequest("Ошибка: На складе недостаточно товара");
                        }
                    }
                    
                    await dataBase.SaveChangesAsync();
                    var Result = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.Warehouse_item_id == shopItem.Warehouse_item_id);
                    return Ok(Result);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в ShopItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи продукта на продаже
        /// </summary>
        /// <param name="token">JWT токен запроса</param>FirstOrDefault
        /// <param name="shopItem">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public async Task<ActionResult> Update([FromHeader] string token, [FromBody] Models.ShopItem shopItem)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа
                
                // Получаем изменяемую позицию в списке
                var existingShopItem = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == shopItem.id);
                if (existingShopItem == null) return NotFound("Ошибка: Изменяемая запись не найдена");

                // Ищем складской товар, который прислал клиент
                var newWI = await dataBase.WarehouseItems.FirstOrDefaultAsync(x => x.id == shopItem.Warehouse_item_id);
                if (newWI == null) return BadRequest("Ошибка: Позиция на складе не найдена");

                // 1: Пользователь сменил сам товар со склада
                if (existingShopItem.Warehouse_item_id != shopItem.Warehouse_item_id)
                {
                    // Ищем старый складской товар, чтобы вернуть его остатки
                    var oldWI = await dataBase.WarehouseItems.FirstOrDefaultAsync(x => x.id == existingShopItem.Warehouse_item_id);
                    if (oldWI != null) oldWI.Count += existingShopItem.Count ?? 0; // Полностью возвращаем старый товар на старую позицию

                    // Проверяем на дублирование позиции
                    var duplicateShopItem = await dataBase.ShopItems.FirstOrDefaultAsync(x => x.Warehouse_item_id == shopItem.Warehouse_item_id);

                    // Проверяем, хватает ли нового товара на складе для добавления
                    int requestedCount = shopItem.Count ?? 0;
                    if (newWI.Count >= requestedCount)
                    {
                        newWI.Count -= requestedCount; // Списываем с нового склада

                        if (duplicateShopItem != null)
                        {
                            // Прибавляем введенное количество к существующей записи
                            duplicateShopItem.Count += requestedCount;
                            // Удаляем редактируемую запись
                            dataBase.ShopItems.Remove(existingShopItem);
                            await dataBase.SaveChangesAsync();

                            var duplicateResult = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == duplicateShopItem.id);
                            return Ok(duplicateResult);
                        }
                        else // Просто обновляем текущую запись
                        {
                            existingShopItem.Warehouse_item_id = shopItem.Warehouse_item_id;
                            existingShopItem.Count = shopItem.Count;
                        }
                    }
                    else
                    {
                        // Если товара не хватило, нужно вернуть количество
                        if (oldWI != null) oldWI.Count -= existingShopItem.Count ?? 0;
                        return BadRequest("Ошибка: На новой позиции недостаточно товара");
                    }
                }
                // 2: Товар тот же, просто изменилось его количество
                else
                {
                    int diff = (shopItem.Count ?? 0) - (existingShopItem.Count ?? 0);
                    if (newWI.Count >= diff)
                    {
                        newWI.Count -= diff;
                        existingShopItem.Count = shopItem.Count;
                    }
                    else
                    {
                        return BadRequest("Ошибка: На складе недостаточно товара");
                    }
                }

                await dataBase.SaveChangesAsync();
                var Result = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == existingShopItem.id);
                return Ok(Result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в ShopItems");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Продажа позиции (обновление записи)
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="id">id позиции для продажи</param>
        /// <param name="count">Количество товаров в позиции</param>
        /// <returns>Обновлённая запись</returns>
        [Route("sale")]
        [HttpPut]
        public async Task<ActionResult> Sale([FromHeader] string token, [FromForm] int id, [FromForm] int count)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                // Получаем изменяемую позицию в списке
                var existingShopItem = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == id);
                if (existingShopItem == null) return NotFound("Ошибка: Изменяемая запись не найдена");

                if (existingShopItem.Count < count) return BadRequest("Ошибка: Недостаточно товара для продажи");

                existingShopItem.Count -= count;
                await dataBase.SaveChangesAsync();
                return Ok(existingShopItem);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода SALE в ShopItems");

                return StatusCode(501, ex.Message);
            }
        }

        //TODO: Реализовать удаление при отсутствии записи на складе
        /// <summary>
        /// Удаление записи продукта на продаже
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
                if (UserRole == "Supplier") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingShopItem = await dataBase.ShopItems.Include(x => x.WarehouseItem).FirstOrDefaultAsync(x => x.id == id);
                if (existingShopItem == null) return NotFound("Ошибка: Такая не найдена на продаже");

                var WI = await dataBase.WarehouseItems.FirstOrDefaultAsync(x => x.id == existingShopItem.Warehouse_item_id);
                if (WI == null) return BadRequest("Ошибка: На складе такая позиция не найдена");

                WI.Count += existingShopItem.Count ?? 0;

                dataBase.ShopItems.Remove(existingShopItem);
                await dataBase.SaveChangesAsync();

                return Ok(existingShopItem);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в ShopItems");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
