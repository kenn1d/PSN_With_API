using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using System.Data;
using System.Text.RegularExpressions;

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
        /// Экземпляр Serilog
        /// </summary>
        private readonly ILogger<AuthController> log;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public UsersController(DataContext dataBase, ILogger<AuthController> log)
        {
            this.dataBase = dataBase;
            this.log = log;
        }

        /// <summary>
        /// Получение записей пользователей
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
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                if (UserRole == "admin")
                {
                    var users = await dataBase.Users.ToListAsync();
                    return Ok(users);
                }
                var dtoUsers = await dataBase.Users.Select(x => new UserDTO { id = x.id, full_name = x.Full_name, tel_number = x.Tel_number }).ToListAsync();
                return Ok(dtoUsers);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода GET в Users");

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
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.User User)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                string namePattern = @"^[А-ЯЁ][а-яё]+(-[А-ЯЁ][а-яё]+)? [А-ЯЁ][а-яё]+( [А-ЯЁ][а-яё]+)?$";
                if (string.IsNullOrWhiteSpace(User.Full_name) || !Regex.IsMatch(User.Full_name, namePattern))
                {
                    return BadRequest("Ошибка: Неверный формат ФИО. Каждое слово должно начинаться с заглавной буквы (например: Иванов Иван Иванович)");
                }

                string phonePattern = @"^8\d{10}$";
                if (string.IsNullOrWhiteSpace(User.Tel_number) || !Regex.IsMatch(User.Tel_number, phonePattern))
                {
                    return BadRequest("Ошибка: Неверный формат номера телефона. Номер должен состоять из 11 цифр и начинаться с 8 (например, 89991234567)");
                }

                var existingUser = await dataBase.Users.FirstOrDefaultAsync(x => x.Login == User.Login);
                if (existingUser == null)
                {
                    Models.User newUser = new Models.User()
                    {
                        Full_name = User.Full_name,
                        Tel_number = User.Tel_number,
                        Login = User.Login,
                        Password = User.Password
                    };
                    await dataBase.Users.AddAsync(newUser);
                }
                else return BadRequest("Ошибка: Пользователь с таким логином уже существует");

                await dataBase.SaveChangesAsync();

                return Ok(User);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в Users");

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
        public async Task<ActionResult> Update([FromHeader] string token, [FromHeader] int updateUserId, [FromBody] Models.User User)
       {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingUser = await dataBase.Users.FirstOrDefaultAsync(x => x.id == updateUserId);
                if (updatingUser == null) return BadRequest("Ошибка: Редактируемый пользователь не найден");

                if (updatingUser.Login != User.Login)
                {
                    // Проверяем, существует ли пользователь с новым логином
                    if (await dataBase.Users.AnyAsync(u => u.Login == User.Login))
                        return BadRequest("Ошибка: Пользователь с таким логином уже существует");
                }
                updatingUser.Full_name = User.Full_name;
                updatingUser.Tel_number = User.Tel_number;
                updatingUser.Login = User.Login;
                updatingUser.Password = User.Password;

                await dataBase.SaveChangesAsync();
                return Ok(updatingUser);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в Users");

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
        public async Task<ActionResult> Delete([FromHeader] string token, [FromForm] int user_id)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existUser = await dataBase.Users.FirstOrDefaultAsync(x => x.id == user_id);

                if (existUser == null) return NotFound();

                dataBase.Users.Remove(existUser);
                await dataBase.SaveChangesAsync();

                return Ok(existUser);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в Users");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
