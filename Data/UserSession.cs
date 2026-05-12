using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PetrolStationNetwork.Data
{
    public class UserSession
    {
        public static string Token { get; set; }
        public static int Id { get; set; }
        public static string Full_name { get; set; }
        public static string Tel_number { get; set; }
        public static string Role { get; set; }
        public static string? CompanyName { get; set; }

        public static void LoadUser(string Token, int Id, string Full_name, string Tel_number, string Role, string CompanyName = null)
        {
            UserSession.Token = Token;
            UserSession.Id = Id;
            UserSession.Full_name = Full_name;
            UserSession.Tel_number = Tel_number;
            if (Role == "Supplier")
                UserSession.CompanyName = CompanyName;
            UserSession.Role = Role;

            MainWindow.init.frame.Navigate(new Views.Pages.Main(Full_name));
        }

        public static void DeleteSession()
        {
            Token = null;
            Id = 0;
            Full_name = null;
            Tel_number = null;
            Role = null;
            MainWindow.init.frame.Navigate(new Views.Pages.Authorisation());
        }
    }
}
