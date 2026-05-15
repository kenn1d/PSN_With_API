using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;

namespace PetrolStationNetwork.Data.Common
{
    public class UsersCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей
        /// </summary>
        /// <returns>Список записей</returns>
        public static async Task<ObservableCollection<Models.User>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "Users/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.User> users = JsonConvert.DeserializeObject<List<Models.User>>(sResponse) ?? new List<Models.User>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.User> items = new ObservableCollection<Models.User>(users);
                        return items;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод добавления записи продукта
        /// </summary>
        /// <param name="User">Объект для добавления</param>
        /// <returns>Созданный объект или null</returns>
        public static async Task<Models.User> Add(Models.User User)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "Users/add"))
                {
                    // В заголовок запроса добавляем токен
                    Request.Headers.Add("token", UserSession.Token);
                    // Сериализуем новый товар в Json формат
                    string JsonData = JsonConvert.SerializeObject(User);
                    // В тело запроса добавляем товар в Json формате
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    // Асинхронно отправляем запрос
                    var Response = await Client.SendAsync(Request);
                    // Проверяем на успех
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем полученный объект
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Конверируем обратно из Json в объект Models.Product
                        Models.User newUser = JsonConvert.DeserializeObject<Models.User>(sResponse) ?? new Models.User();
                        // Возвращаем новый товар
                        return newUser;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи продукта
        /// </summary>
        /// <param name="User">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.User> Update(Models.User User)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "Users/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    string JsonData = JsonConvert.SerializeObject(User);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.User updateUser = JsonConvert.DeserializeObject<Models.User>(sResponse) ?? new Models.User();
                        return updateUser;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод для удаления записи
        /// </summary>
        /// <param name="user_id">id пользователя</param>
        /// <returns>Результат True или False</returns>
        public static async Task<bool> Delete(int user_id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "Users/delete"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Dictionary<string, string> FormData = new Dictionary<string, string>()
                    {
                        ["user_id"] = user_id.ToString()
                    };
                    FormUrlEncodedContent content = new FormUrlEncodedContent(FormData);
                    Request.Content = content;

                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Request.Content.ReadAsStringAsync();
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
