using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Reflection.Metadata;

namespace MilFoodReportCard
{
    public class MfrcDbContext: DbContext
    {
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<FedLayout> FedLayouts { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Product> Products { get; set; }
        //public DbSet<Remain> Remains { get; set; }
        public DbSet<OutgoingsDoc> Outgoings { get; set; }
        public DbSet<OutgoingsDoc1> Outgoings1 { get; set; }
        public DbSet<IncomesDoc> Incomes { get; set; }
        public DbSet<IncomesDoc1> Incomes1 { get; set; }
        public DbSet<LayoutEntry> LayoutEntries { get; set; }
        public DbSet<Agreement> Agreement { get; set; }
        public DbSet<AgreementDetails> AgreementDetails { get; set; }
        public DbSet<OutgoingsDocFed> OutgoingsDocFeds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseFilename = ConfigurationManager.AppSettings.Get("DatabaseFilename");
            var databasePassword = ConfigurationManager.AppSettings.Get("DatabasePassword");
            var connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={databaseFilename};Persist Security Info=True;Jet OLEDB:Database Password={databasePassword}";
            optionsBuilder.UseJet(connectionString);
        }
    }
}
