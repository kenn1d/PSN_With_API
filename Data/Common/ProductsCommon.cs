using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace PetrolStationNetwork.Data.Common
{
    public class ProductsCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей продуктов
        /// </summary>
        /// <returns>Список продуктов</returns>
        public static async Task<ObservableCollection<Models.Product>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "Products/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.Product> existProducts = JsonConvert.DeserializeObject<List<Models.Product>>(sResponse) ?? new List<Models.Product>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.Product> products = new ObservableCollection<Models.Product>(existProducts);
                        return products;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод добавления записи продукта
        /// </summary>
        /// <param name="product">Объект для добавления</param>
        /// <returns>Созданный объект или null</returns>
        public static async Task<Models.Product> Add(Models.Product product)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "Products/add"))
                {
                    // В заголовок запроса добавляем токен
                    Request.Headers.Add("token", UserSession.Token);
                    // Сериализуем новый товар в Json формат
                    string JsonData = JsonConvert.SerializeObject(product);
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
                        Models.Product newProduct = JsonConvert.DeserializeObject<Models.Product>(sResponse) ?? new Models.Product();
                        // Возвращаем новый товар
                        return newProduct;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи продукта
        /// </summary>
        /// <param name="product">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.Product> Update(Models.Product product)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "Products/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    string JsonData = JsonConvert.SerializeObject(product);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding .UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.Product updateProduct = JsonConvert.DeserializeObject<Models.Product>(sResponse) ?? new Models.Product();
                        return updateProduct;
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
        public static async Task<bool> Delete(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "Products/delete"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Dictionary<string, string> FormData = new Dictionary<string, string>() {
                        ["id"] = id.ToString()
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
