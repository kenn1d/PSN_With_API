using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetrolStationNetwork.Models
{
    public partial class Staff : ObservableObject
    {
        [Key]
        [ForeignKey(nameof(User))]
        public int user_id;

        [ObservableProperty]
        private string role;

        public User User { get; set; }
    }
}
