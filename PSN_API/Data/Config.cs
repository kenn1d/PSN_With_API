using Microsoft.EntityFrameworkCore;

namespace PetrolStationNetwork.Data
{
    public class Config
    {
        public static readonly string connection = "server=localhost;uid=root;pwd=;database=PetrolStationNetwork";
        public static readonly MySqlServerVersion version = new MySqlServerVersion(new Version(8, 0, 11));
    }
}
