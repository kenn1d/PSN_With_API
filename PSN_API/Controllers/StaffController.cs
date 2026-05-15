using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        /// <summary>
        /// Приватное поле для хранения экземпляра DataContext
        /// Используется для работы с БД
        /// </summary>
        private DataContext dataBase;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public StaffController(DataContext dataBase)
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

                // Конвертируем модель в DTO модель
                var staff = dataBase.Staff.Include(x => x.User)
                    .Select(x => new StaffDTO { 
                        id = x.user_id, 
                        role = x.Role,
                        FullName = x.FullName
                    })
                    // Запоминаем полученные данные с сервера
                    .AsEnumerable()
                    // Конвертируем DTO обратно в обычную модель для отправки клиенту
                    .Select(x => new Models.Staff
                    {
                        user_id = x.id,
                        Role = x.role,
                        User = new Models.User
                        {
                            id = x.id,
                            Full_name = x.FullName
                        }
                    })
                    .ToList();

                return Ok(staff);
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
        /// <param name="staff">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.Staff staff)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingStaff = dataBase.Staff.Include(x => x.User).FirstOrDefault(x => x.user_id == staff.user_id);
                var existingSupplier = dataBase.Suppliers.Include(x => x.User).FirstOrDefault(x => x.user_id == staff.user_id);
                if (existingStaff == null && existingSupplier == null)
                {
                    Models.Staff newStaff = new Models.Staff()
                    {
                        user_id = staff.user_id,
                        Role = staff.Role
                    };
                    dataBase.Staff.Add(newStaff);
                }
                else return BadRequest("Ошибка: У пользователя уже есть роль");

                dataBase.SaveChanges();

                return Ok(staff);
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
        /// <param name="updateUserId">id обновляемой записи</param>
        /// <param name="staff">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromHeader] int updateUserId, [FromBody] Models.Staff staff)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingStaff = dataBase.Staff.Include(x => x.User).FirstOrDefault(x => x.user_id == updateUserId);
                if (updatingStaff == null) return BadRequest("Ошибка: Редактируемый поставщик не найден");

                if (updateUserId != staff.user_id)
                {
                    // Проверяем, существует ли новый пользователь
                    if (!dataBase.Users.Any(u => u.id == staff.user_id))
                        return BadRequest("Ошибка: Новый пользователь не существует");

                    // Проверяем, не занят ли он уже другой ролью
                    if (dataBase.Suppliers.Any(s => s.user_id == staff.user_id) ||
                        dataBase.Staff.Any(st => st.user_id == staff.user_id))
                        return BadRequest("Ошибка: У нового пользователя уже есть роль");

                    dataBase.Staff.Remove(updatingStaff);
                    dataBase.Staff.Add(staff);
                }
                else
                {
                    updatingStaff.Role = staff.Role;
                }

                dataBase.SaveChanges();
                return Ok(staff);
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

                var existStaff = dataBase.Staff.Include(x => x.User).FirstOrDefault(x => x.user_id == user_id);
                if (existStaff == null) return NotFound();

                dataBase.Staff.Remove(existStaff);
                dataBase.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
