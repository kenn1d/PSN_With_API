using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using System.Data;

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
        /// Получение записей пользователей
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
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                if (UserRole == "admin")
                {
                    var users = dataBase.Users.ToList();
                    return Ok(users);
                }
                var dtoUsers = dataBase.Users.Select(x => new UserDTO { id = x.id, full_name = x.Full_name, tel_number = x.Tel_number }).ToList();
                return Ok(dtoUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи нового пользователя
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

                var existingUser = dataBase.Users.FirstOrDefault(x => x.Login == User.Login);
                if (existingUser == null)
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
                else return BadRequest("Ошибка: Пользователь с таким логином уже существует");

                dataBase.SaveChanges();

                return Ok(User);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи пользователя
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="User">Обновлённые данные записи</param>
        /// <param name="updateUserId">id Редатируемой записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public ActionResult Update([FromHeader] string token, [FromHeader] int updateUserId, [FromBody] Models.User User)
       {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingUser = dataBase.Users.FirstOrDefault(x => x.id == updateUserId);
                if (updatingUser == null) return BadRequest("Ошибка: Редактируемый пользователь не найден");

                if (updatingUser.Login != User.Login)
                {
                    // Проверяем, существует ли пользователь с новым логином
                    if (dataBase.Users.Any(u => u.Login == User.Login))
                        return BadRequest("Ошибка: Пользователь с таким логином уже существует");
                }
                updatingUser.Full_name = User.Full_name;
                updatingUser.Tel_number = User.Tel_number;
                updatingUser.Login = User.Login;
                updatingUser.Password = User.Password;

                dataBase.SaveChanges();
                return Ok(updatingUser);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи пользователя
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

                var existUser = dataBase.Users.FirstOrDefault(x => x.id == user_id);

                if (existUser == null) return NotFound();

                dataBase.Users.Remove(existUser);
                dataBase.SaveChanges();

                return Ok(existUser);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
