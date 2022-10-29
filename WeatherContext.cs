using Microsoft.EntityFrameworkCore;

namespace MinimalisticWeatherAPI
{
    public class WeatherContext : DbContext
    {
        private static bool _created = false;
        public WeatherContext()
        {
            if (!_created)
            {
                _created = true;
                Database.EnsureCreated();
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite(@"Data Source=weatherDB.db");
        }

        public DbSet<Weather> Weather { get; set; }
    }
}
