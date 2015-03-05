using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SquidReports.DataCollector.Plugin.BES.Model;

namespace SquidReports.DataCollector.Plugin.BES.Context
{
    public class BESContext : DbContext
    {
        public BESContext() : base("DB")
        {

        }
        
        public DbSet<Model.Action> Actions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
