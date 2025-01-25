using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using Utility;

namespace DataAccess
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<ShiftHour> ShiftHours { get; set; }
        public DbSet<MinuteRange> MinuteRanges { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SubscriptionNote> SubscriptionNotes { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<ChildSubscription> ChildSubscriptions { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<CheckInOut> CheckInOut { get; set; }
        public DbSet<CheckProduct> CheckProducts { get; set; }
        public DbSet<CheckChild> CheckChildren { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SubscriptionPlan>()
               .Property<string>("WhichDays") 
               .HasColumnName("WhichDays");

            #region ManyToMany
            modelBuilder.Entity<Child>(eb => {
                eb.HasMany(e => e.subscriptionPlans)
                .WithMany(e => e.Children)
                .UsingEntity<ChildSubscription>();
            });

            modelBuilder.Entity<Product>(eb => {
                eb.HasMany(e => e.CheckInOuts)
                .WithMany(e => e.Products)
                .UsingEntity<CheckProduct>();
            });

            modelBuilder.Entity<Child>(eb => {
                eb.HasMany(e => e.CheckInOuts)
                .WithMany(e => e.Children)
                .UsingEntity<CheckChild>();
            });
            #endregion
        }

    }
}
