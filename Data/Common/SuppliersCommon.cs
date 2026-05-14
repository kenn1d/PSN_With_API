using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows;

namespace PetrolStationNetwork.Data.Common
{
    public class SuppliersCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей продуктов
        /// </summary>
        /// <returns>Список продуктов</returns>
        public static async Task<ObservableCollection<Models.Supplier>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "Suppliers/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.Supplier> existSupplier = JsonConvert.DeserializeObject<List<Models.Supplier>>(sResponse) ?? new List<Models.Supplier>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.Supplier> suppliers = new ObservableCollection<Models.Supplier>(existSupplier);
                        return suppliers;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод добавления записи продукта
        /// </summary>
        /// <param name="supplier">Объект для добавления</param>
        /// <returns>Созданный объект или null</returns>
        public static async Task<Models.Supplier> Add(Models.Supplier supplier)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "Suppliers/add"))
                {
                    // В заголовок запроса добавляем токен
                    Request.Headers.Add("token", UserSession.Token);
                    // Сериализуем новый товар в Json формат
                    string JsonData = JsonConvert.SerializeObject(supplier);
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
                        Models.Supplier newSupplier = JsonConvert.DeserializeObject<Models.Supplier>(sResponse) ?? new Models.Supplier();
                        // Возвращаем новый товар
                        return newSupplier;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи продукта
        /// </summary>
        /// <param name="supplier">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.Supplier> Update(int updateUserId, Models.Supplier supplier)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "Suppliers/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Request.Headers.Add("updateUserId", updateUserId.ToString());
                    string JsonData = JsonConvert.SerializeObject(supplier);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.Supplier updateSupplier = JsonConvert.DeserializeObject<Models.Supplier>(sResponse) ?? new Models.Supplier();
                        return updateSupplier;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод для удаления записи продукта
        /// </summary>
        /// <param name="id">id продукта</param>
        /// <returns>Результат True или False</returns>
        public static async Task<bool> Delete(int user_id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "Suppliers/delete"))
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
