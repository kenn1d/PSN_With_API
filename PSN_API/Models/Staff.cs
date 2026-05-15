using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PSN_API.Models
{
    public partial class Staff : ObservableObject
    {
        [Key]
        [ForeignKey(nameof(User))]
        public int user_id { get; set; }

        [ObservableProperty]
        private string role;

        [NotMapped]
        public string FullName => User?.Full_name ?? "Нет ФИО";

        public User? User { get; set; }
    }
}
