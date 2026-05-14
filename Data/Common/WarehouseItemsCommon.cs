using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;

namespace PetrolStationNetwork.Data.Common
{
    public class WarehouseItemsCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей
        /// </summary>
        /// <returns>Список записей</returns>
        public static async Task<ObservableCollection<Models.WarehouseItem>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "WarehouseItems/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.WarehouseItem> existWarehouseItems = JsonConvert.DeserializeObject<List<Models.WarehouseItem>>(sResponse) ?? new List<Models.WarehouseItem>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.WarehouseItem> items = new ObservableCollection<Models.WarehouseItem>(existWarehouseItems);
                        return items;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи
        /// </summary>
        /// <param name="warehouseItem">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.WarehouseItem> Update(Models.WarehouseItem warehouseItem)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "WarehouseItems/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    string JsonData = JsonConvert.SerializeObject(warehouseItem);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.WarehouseItem updateWarehouseItem = JsonConvert.DeserializeObject<Models.WarehouseItem>(sResponse) ?? new Models.WarehouseItem();
                        return updateWarehouseItem;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод для удаления записи
        /// </summary>
        /// <param name="id">id записи</param>
        /// <returns>Результат True или False</returns>
        public static async Task<bool> Delete(int id)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "WarehouseItems/delete"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Dictionary<string, string> FormData = new Dictionary<string, string>()
                    {
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
