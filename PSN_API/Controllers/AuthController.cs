using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PSN_API.Classes;
using PSN_API.Data;
using PSN_API.Models;

namespace PSN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Приватное поле для хранения экземпляра DatabaseManager
        /// Используется для работы с БД
        /// </summary>
        private DataContext dataBase;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        /// <param name="dataBase">База данных</param>
        public AuthController(DataContext dataBase)
        {
            this.dataBase = dataBase;
        }

        /// <summary>
        /// Метод для аутентификации пользователя
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>JWT токен или код ошибки</returns>
        [Route("login")]
        [HttpPost]
        public ActionResult Login([FromForm] string login, [FromForm] string password)
        {
            try
            {
                var user = dataBase.Users.Include(x => x.Supplier).Include(x => x.Staff).FirstOrDefault(x => x.Login == login && x.Password == password);
                if (user == null) return StatusCode(401);

                string role = "";
                string company = null;

                if (login == "admin" && password == "admin")
                {
                    role = "admin";
                }
                else
                {
                    if (user.Supplier != null)
                    {
                        role = "Supplier";
                        company = user.Supplier.Company_name;
                    }
                    else if (user.Staff != null)
                    {
                        role = user.Staff.Role;
                    }
                }

                var token = JwtToken.Generate(user, role);

                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        id = user.id,
                        full_name = user.Full_name,
                        tel_number = user.Tel_number,
                        role = role,
                        company = company
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
