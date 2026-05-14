using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using PSN_API.Models;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        /// <summary>
        /// Приватное поле для хранения экземпляра DataContext
        /// Используется для работы с БД
        /// </summary>
        private DataContext dataBase;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public SuppliersController(DataContext dataBase)
        {
            this.dataBase = dataBase;
        }

        /// <summary>
        /// Получение записей
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список или ошибка</returns>
        [Route("get")]
        [HttpGet]
        public ActionResult Get([FromHeader] string token)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized(); // StatusCode 401

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                List<Models.Supplier> suppliers = dataBase.Suppliers.Include(x => x.User).ToList();
                return Ok(suppliers);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи нового
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="supplier">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.Supplier supplier)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingStaff = dataBase.Staff.Include(x => x.User).FirstOrDefault(x => x.user_id == supplier.user_id);
                var existingSupplier = dataBase.Suppliers.Include(x => x.User).FirstOrDefault(x => x.user_id == supplier.user_id);
                if (existingStaff == null && existingSupplier == null)
                {
                    Supplier newSupplier = new Supplier()
                    {
                        user_id = supplier.user_id,
                        Company_name = supplier.Company_name
                    };
                    dataBase.Suppliers.Add(newSupplier);
                }
                else return BadRequest("Ошибка: У пользователя уже есть роль");

                dataBase.SaveChanges();

                return Ok(supplier);
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
        /// <param name="supplier">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromHeader] int updateUserId, [FromBody] Models.Supplier supplier)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingSupplier = dataBase.Suppliers.Include(x => x.User).FirstOrDefault(x => x.user_id == updateUserId);
                if (updatingSupplier == null) return BadRequest("Ошибка: Редактируемый поставщик не найден");

                if (updateUserId != supplier.user_id)
                {
                    // Проверяем, существует ли новый пользователь
                    if (!dataBase.Users.Any(u => u.id == supplier.user_id))
                        return BadRequest("Ошибка: Новый пользователь не существует");

                    // Проверяем, не занят ли он уже другой ролью
                    if (dataBase.Suppliers.Any(s => s.user_id == supplier.user_id) ||
                        dataBase.Staff.Any(st => st.user_id == supplier.user_id))
                        return BadRequest("Ошибка: У нового пользователя уже есть роль");

                    dataBase.Suppliers.Remove(updatingSupplier);
                    dataBase.Suppliers.Add(supplier);
                }
                else
                {
                    updatingSupplier.Company_name = supplier.Company_name;
                }

                dataBase.SaveChanges();
                return Ok(supplier);
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
        /// <param name="user_id">id пользователя</param>
        /// <returns>Статус операции</returns>
        [Route("delete")]
        [HttpDelete]
        public ActionResult Delete([FromHeader] string token, [FromForm] int user_id)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existSupplier = dataBase.Suppliers.Include(x => x.User).FirstOrDefault(x => x.user_id == user_id);
                if (existSupplier == null) return NotFound();

                dataBase.Suppliers.Remove(existSupplier);
                dataBase.SaveChanges();

                return Ok(existSupplier);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}