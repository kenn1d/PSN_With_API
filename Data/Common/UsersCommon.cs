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
    }
}
