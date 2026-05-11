using CommunityToolkit.Mvvm.ComponentModel;

namespace PSN_API.Models
{
    public partial class Product : ObservableObject
    {
        public int id { get; set; }

        [ObservableProperty]
        private string name;
    }
}
