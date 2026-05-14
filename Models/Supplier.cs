using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetrolStationNetwork.Models
{
    public partial class Supplier : ObservableObject
    {
        [Key]
        [ForeignKey(nameof(User))]
        public int user_id { get; set; }

        [ObservableProperty]
        private string company_name;

        [NotMapped]
        public string FullName => User?.Full_name ?? "Нет ФИО";

        public User? User { get; set; }
    }
}
