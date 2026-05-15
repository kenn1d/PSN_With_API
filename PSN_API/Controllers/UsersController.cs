using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Приватное поле для хранения экземпляра DataContext
        /// Используется для работы с БД
        /// </summary>
        private DataContext dataBase;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public UsersController(DataContext dataBase)
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
                if (UserRole != "leader") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var users = dataBase.Users.Select(x => new UserDTO { id = x.id, full_name = x.Full_name, tel_number = x.Tel_number }).ToList();
                return Ok(users);
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
        /// <param name="User">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public ActionResult Add([FromHeader] string token, [FromBody] Models.User User)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingStaff = dataBase.Staff.Include(x => x.User).FirstOrDefault(x => x.user_id == User.user_id);
                var existingUser = dataBase.Users.Include(x => x.User).FirstOrDefault(x => x.user_id == User.user_id);
                if (existingStaff == null && existingUser == null)
                {
                    Models.User newUser = new Models.User()
                    {
                        Full_name = User.Full_name,
                        Tel_number = User.Tel_number,
                        Login = User.Login,
                        Password = User.Password
                    };
                    dataBase.Users.Add(newUser);
                }
                else return BadRequest("Ошибка: У пользователя уже есть роль");

                dataBase.SaveChanges();

                return Ok(User);
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
        /// <param name="User">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromBody] Models.User User)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingUser = dataBase.Users.Include(x => x.User).FirstOrDefault(x => x.user_id == updateUserId);
                if (updatingUser == null) return BadRequest("Ошибка: Редактируемый поставщик не найден");

                if (updateUserId != User.user_id)
                {
                    // Проверяем, существует ли новый пользователь
                    if (!dataBase.Users.Any(u => u.id == User.user_id))
                        return BadRequest("Ошибка: Новый пользователь не существует");

                    // Проверяем, не занят ли он уже другой ролью
                    if (dataBase.Users.Any(s => s.user_id == User.user_id) ||
                        dataBase.Staff.Any(st => st.user_id == User.user_id))
                        return BadRequest("Ошибка: У нового пользователя уже есть роль");

                    dataBase.Users.Remove(updatingUser);
                    dataBase.Users.Add(User);
                }
                else
                {
                    updatingUser.Company_name = User.Company_name;
                }

                dataBase.SaveChanges();
                return Ok(User);
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
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existUser = dataBase.Users.Include(x => x.User).FirstOrDefault(x => x.user_id == user_id);

                if (existUser == null) return NotFound();

                dataBase.Users.Remove(existUser);
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
