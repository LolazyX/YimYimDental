using Microsoft.EntityFrameworkCore;
using YimYimDental.Models;
namespace YimYimDental.Data
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        public DbSet<UserViewModel> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<TreatmentHistory> TreatmentHistories { get; set; }
        public DbSet<TreatmentQueue> TreatmentQueues { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<WorkingTime> WorkingTimes { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Xray> Xrays { get; set; }
    }
}


