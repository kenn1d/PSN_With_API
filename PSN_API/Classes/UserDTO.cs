using CommunityToolkit.Mvvm.ComponentModel;

namespace PSN_API.Classes
{
    /// <summary>
    /// Сокращенный класс User для предотвращаения отправки логинов и паролей
    /// </summary>
    public class UserDTO
    {
        public int id { get; set; }

        public string full_name { get; set; }

        public string tel_number { get; set; }
    }
}
