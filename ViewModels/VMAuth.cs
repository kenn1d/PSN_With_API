using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PetrolStationNetwork.Data;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;

namespace PetrolStationNetwork.ViewModels
{
    public partial class VMAuth : ObservableObject
    {
        /// <summary>Базовый URL API для пользовательских операций</summary>
        public static string url = "https://localhost:7101/api/";

        [ObservableProperty]
        private ObservableCollection<Models.User> users;

        [ObservableProperty]
        private ObservableCollection<Models.Supplier> suppliers;

        [ObservableProperty]
        private ObservableCollection<Models.Staff> staff;

        [ObservableProperty]
        private string login;

        [ObservableProperty]
        private string password;

        public ICommand LogIn { get; }

        public VMAuth()
        {
            LogIn = new RelayCommand<object>(async (param) =>
            {
                // Получаем пароль из PasswordBox
                var passwordBox = param as System.Windows.Controls.PasswordBox;
                Password = passwordBox.Password;

                // Создаём http клиент для отправки запроса
                using (HttpClient Client = new HttpClient())
                {
                    // Создаём запрос с методом post
                    using (HttpRequestMessage Request = new HttpRequestMessage(HttpMethod.Post, url + "Auth/login"))
                    {
                        // Формируем данные для отправки
                        Dictionary<string, string> FormData = new Dictionary<string, string>
                        {
                            ["login"] = login,
                            ["password"] = Password
                        };
                        // Создаём контент запроса из данных формы
                        FormUrlEncodedContent Content = new FormUrlEncodedContent(FormData);
                        // Устанавливаем контент в запрос
                        Request.Content = Content;
                        // Отправляем запрос и ждём ответ
                        var Response = await Client.SendAsync(Request);
                        // Проверяем статус ответа
                        if (Response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // Читаем асинхронно json файл от сервера
                            string sResponse = await Response.Content.ReadAsStringAsync();
                            // Десериализуем json в объект
                            AuthData DataAuth = JsonConvert.DeserializeObject<AuthData>(sResponse);
                            UserSession.LoadUser(DataAuth.Token, DataAuth.User.id, DataAuth.User.full_name, DataAuth.User.tel_number, DataAuth.User.role, DataAuth.User.company);
                        }
                        else
                        {
                            MessageBox.Show($"Пользователь с таким логином и паролем не найден.", "Пользователь не найден!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Временный класс для десериализации данных
        /// </summary>
        private class AuthData()
        {
            public string Token { get; set; }
            public UserData User { get; set; }
        }
        /// <summary>
        /// Временный класс для десериализации данных
        /// </summary>
        private class UserData()
        {
            public int id { get; set; }
            public string full_name { get; set; }
            public string tel_number { get; set; }
            public string role { get; set; }
            public string? company { get; set; }
        }
    }
}
