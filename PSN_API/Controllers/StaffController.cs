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
        /// Экземпляр Serilog
        /// </summary>
        private readonly ILogger<AuthController> log;

        /// <summary>
        /// Конструктор контроллера
        /// </summary>
        public StaffController(DataContext dataBase, ILogger<AuthController> log)
        {
            this.dataBase = dataBase;
            this.log = log;
        }

        /// <summary>
        /// Получение записей сотрудников
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <returns>Список или ошибка</returns>
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

                // Конвертируем модель в DTO модель
                var staff = ( await dataBase.Staff.Include(x => x.User)
                    .Select(x => new StaffDTO { 
                        id = x.user_id, 
                        role = x.Role,
                        FullName = x.FullName
                    }).ToListAsync())
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
                log.LogError(ex, "Ошибка при выполнении метода GET в Staff");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Добавление записи нового сотруднкиа
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="staff">Новый объект</param>
        /// <returns>Новый объект или ошибка</returns>
        [Route("add")]
        [HttpPost]
        public async Task<ActionResult> Add([FromHeader] string token, [FromBody] Models.Staff staff)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existingStaff = await dataBase.Staff.Include(x => x.User).FirstOrDefaultAsync(x => x.user_id == staff.user_id);
                var existingSupplier = await dataBase.Suppliers.Include(x => x.User).FirstOrDefaultAsync(x => x.user_id == staff.user_id);
                if (existingStaff == null && existingSupplier == null)
                {
                    Models.Staff newStaff = new Models.Staff()
                    {
                        user_id = staff.user_id,
                        Role = staff.Role
                    };
                    await dataBase.Staff.AddAsync(newStaff);
                }
                else return BadRequest("Ошибка: У пользователя уже есть роль");

                await dataBase.SaveChangesAsync();

                return Ok(staff);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода ADD в Staff");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Обновление записи сотрудника
        /// </summary>
        /// <param name="token">JWT токен запроса</param>
        /// <param name="updateUserId">id обновляемой записи</param>
        /// <param name="staff">Обновлённые данные записи</param>
        /// <returns>Обновлённая запись</returns>
        [Route("update")]
        [HttpPut]
        public async Task<ActionResult> Update([FromHeader] string token, [FromHeader] int updateUserId, [FromBody] Models.Staff staff)
        {
            try
            {
                int? UserId = JwtToken.GetUserIdFromToken(token);
                if (UserId == null) return Unauthorized();

                string? UserRole = JwtToken.GetRoleFromToken(token);
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var updatingStaff = await dataBase.Staff.Include(x => x.User).FirstOrDefaultAsync(x => x.user_id == updateUserId);
                if (updatingStaff == null) return BadRequest("Ошибка: Редактируемый поставщик не найден");

                if (updateUserId != staff.user_id)
                {
                    // Проверяем, существует ли новый пользователь
                    if (!await dataBase.Users.AnyAsync(u => u.id == staff.user_id))
                        return BadRequest("Ошибка: Новый пользователь не существует");

                    // Проверяем, не занят ли он уже другой ролью
                    if (await dataBase.Suppliers.AnyAsync(s => s.user_id == staff.user_id) ||
                        await dataBase.Staff.AnyAsync(st => st.user_id == staff.user_id))
                        return BadRequest("Ошибка: У нового пользователя уже есть роль");

                    dataBase.Staff.Remove(updatingStaff);
                    await dataBase.Staff.AddAsync(staff);
                }
                else
                {
                    updatingStaff.Role = staff.Role;
                }

                await dataBase.SaveChangesAsync();
                return Ok(staff);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода UPDATE в Staff");

                return StatusCode(501, ex.Message);
            }
        }

        /// <summary>
        /// Удаление записи сотрудника
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
                if (UserRole != "leader" && UserRole != "admin") return BadRequest("Ошибка 403: Отсутствуют права доступа"); // StatusCode 403 нет доступа

                var existStaff = await dataBase.Staff.Include(x => x.User).FirstOrDefaultAsync(x => x.user_id == user_id);
                if (existStaff == null) return NotFound();

                dataBase.Staff.Remove(existStaff);
                await dataBase.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Ошибка при выполнении метода DELETE в Staff");

                return StatusCode(501, ex.Message);
            }
        }
    }
}
