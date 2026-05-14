using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;

namespace PetrolStationNetwork.Data.Common
{
    public class ShopItemsCommon
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        /// <summary>
        /// Асинхронный метод получения записей
        /// </summary>
        /// <returns>Список записей</returns>
        public static async Task<ObservableCollection<Models.ShopItem>> Get()
        {
            // Создаём Http клиент
            using (HttpClient Client = new HttpClient())
            {
                // Создаём get запрос
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Get, url + "ShopItems/get"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем плученные данные
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Десериализуем данные в список
                        List<Models.ShopItem> existShopItems = JsonConvert.DeserializeObject<List<Models.ShopItem>>(sResponse) ?? new List<Models.ShopItem>();
                        // Преобразуем в тип ObservableCollection
                        ObservableCollection<Models.ShopItem> items = new ObservableCollection<Models.ShopItem>(existShopItems);
                        return items;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод добавления записи
        /// </summary>
        /// <param name="shopItem">Объект для добавления</param>
        /// <returns>Созданный объект или null</returns>
        public static async Task<Models.ShopItem> Add(Models.ShopItem shopItem)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "ShopItems/add"))
                {
                    // В заголовок запроса добавляем токен
                    Request.Headers.Add("token", UserSession.Token);
                    // Сериализуем новый товар в Json формат
                    string JsonData = JsonConvert.SerializeObject(shopItem);
                    // В тело запроса добавляем товар в Json формате
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    // Асинхронно отправляем запрос
                    var Response = await Client.SendAsync(Request);
                    // Проверяем на успех
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        // Асинхронно читаем полученный объект
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        // Конверируем обратно из Json в объект Models.ShopItem
                        Models.ShopItem newItem = JsonConvert.DeserializeObject<Models.ShopItem>(sResponse) ?? new Models.ShopItem();
                        // Возвращаем новый товар
                        return newItem;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи
        /// </summary>
        /// <param name="shopItem">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.ShopItem> Update(Models.ShopItem shopItem)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "ShopItems/update"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    string JsonData = JsonConvert.SerializeObject(shopItem);
                    Request.Content = new StringContent(JsonData, System.Text.Encoding.UTF8, "application/json");
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.ShopItem updateShopItem = JsonConvert.DeserializeObject<Models.ShopItem>(sResponse) ?? new Models.ShopItem();
                        return updateShopItem;
                    }
                    else
                    {
                        // Читаем текст ошибки, которую вернул сервер (например, "Ошибка: На складе недостаточно товара")
                        string errorText = await Response.Content.ReadAsStringAsync();
                        System.Windows.MessageBox.Show($"Сервер вернул {Response.StatusCode}: {errorText}", "Ошибка API");
                    }
                    //if (Response.StatusCode == HttpStatusCode.OK)
                    //{
                    //    string sResponse = await Response.Content.ReadAsStringAsync();
                    //    Models.ShopItem updateShopItem = JsonConvert.DeserializeObject<Models.ShopItem>(sResponse) ?? new Models.ShopItem();
                    //    return updateShopItem;
                    //}
                }
            }
            return null;
        }

        /// <summary>
        /// Асинхронный метод изменения записи
        /// </summary>
        /// <param name="shopItem">Изменяемый объект с новыми данными</param>
        /// <returns>Изменённый объект или null</returns>
        public static async Task<Models.ShopItem> Sale(int id, int count)
        {
            using (HttpClient Client = new HttpClient())
            {
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Put, url + "ShopItems/sale"))
                {
                    Request.Headers.Add("token", UserSession.Token);
                    Dictionary<string, string> FormData = new Dictionary<string, string>()
                    {
                        ["id"] = id.ToString(),
                        ["count"] = count.ToString()
                    };
                    FormUrlEncodedContent content = new FormUrlEncodedContent(FormData);
                    Request.Content = content;
                    var Response = await Client.SendAsync(Request);
                    if (Response.StatusCode == HttpStatusCode.OK)
                    {
                        string sResponse = await Response.Content.ReadAsStringAsync();
                        Models.ShopItem updateShopItem = JsonConvert.DeserializeObject<Models.ShopItem>(sResponse) ?? new Models.ShopItem();
                        return updateShopItem;
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
                using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Delete, url + "ShopItems/delete"))
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
