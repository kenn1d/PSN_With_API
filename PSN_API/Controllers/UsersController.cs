using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

                List<Models.User> users = dataBase.Users.ToList();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(501, ex.Message);
            }
        }
    }
}
