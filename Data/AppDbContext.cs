using Microsoft.EntityFrameworkCore;
using projetStage.Models;  // Assurez-vous que ces espaces de noms sont corrects
using scrapp_app.Models;

namespace scrapp_app.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Définir les DbSet pour les entités
        public DbSet<User> Users { get; set; }
        public DbSet<ScrappData> ScrappData { get; set; }

       
        public DbSet<ScrappDataShift> ScrappDataShift { get; set; }

        public DbSet<ScrappDataHistory> ScrappDataHistory { get; set; }

        public DbSet<ScrappDataShiftHistory> ScrappDataShiftHistory { get; set; }

        // Configurer le modèle de données
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
    
            // Configuration de l'entité User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("WESM_users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                    .IsRequired();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Password)
                    .IsRequired();

                entity.Property(e => e.Departement)
                    .HasMaxLength(100);

                // Configuration des propriétés booléennes
                entity.Property(e => e.NeedsPasswordChange)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.Role)
                    .HasMaxLength(50);

                entity.Property(e => e.IsAdmin)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsPurchaser)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsRequester)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsValidator)
                    .HasDefaultValue(false);

                entity.Property(e => e.ReOpenRequestAfterValidation)
                    .HasDefaultValue(false);
            });

            // Configuration de l'entité ScrappData
            modelBuilder.Entity<ScrappData>(entity =>
            {
                entity.ToTable("ScrappData");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Date)
                    .IsRequired();

                // Configurez d'autres propriétés comme nécessaire
            });

            // Configuration de l'entité ScrappDataShift
            modelBuilder.Entity<ScrappDataShift>(entity =>
            {
                entity.ToTable("ScrappDataShift");
                entity.HasKey(e => e.Id);


                entity.Property(e => e.Date)
                    .IsRequired();

                entity.Property(e => e.Purge)
                    .IsRequired();

                entity.Property(e => e.DefautInjection)
                    .IsRequired();

                entity.Property(e => e.DefautAssemblage)
                    .IsRequired();

                entity.Property(e => e.Bavures)
                    .IsRequired();

                // Configurez d'autres propriétés comme nécessaire
            });

            // Ajouter d'autres configurations si nécessaire
        }
    }
}
