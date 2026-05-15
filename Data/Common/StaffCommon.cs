using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace PetrolStationNetwork.Data.Common
{
    internal class StaffCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей
        /// </summary>
        /// <returns>Список</returns>
        public static async Task<ObservableCollection<Models.Staff>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "Staff/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.Staff> existStaff = JsonConvert.DeserializeObject<List<Models.Staff>>(sResponse) ?? new List<Models.Staff>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.Staff> Staff = new ObservableCollection<Models.Staff>(existStaff);
                        return Staff;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод добавления записи
        /// </summary>
        /// <param name="Staff">Объект для добавления</param>
        /// <returns>Созданный объект или null</returns>
        public static async Task<Models.Staff> Add(Models.Staff Staff)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "Staff/add"))
                {
                    // В заголовок запроса добавляем токен
                    Request.Headers.Add("token", UserSession.Token);
                    // Сериализуем новый товар в Json формат
                    string JsonData = JsonConvert.SerializeObject(Staff);
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
                        Models.Staff newStaff = JsonConvert.DeserializeObject<Models.Staff>(sResponse) ?? new Models.Staff();
                        // Возвращаем новый товар
                        return newStaff;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи
        /// </summary>
        /// <param name="Staff">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.Staff> Update(int updateUserId, Models.Staff Staff)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "Staff/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Request.Headers.Add("updateUserId", updateUserId.ToString());
                    string JsonData = JsonConvert.SerializeObject(Staff);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.Staff updateStaff = JsonConvert.DeserializeObject<Models.Staff>(sResponse) ?? new Models.Staff();
                        return updateStaff;
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
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "Staff/delete"))
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
