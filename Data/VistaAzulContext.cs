using Microsoft.EntityFrameworkCore;

namespace VistaAzul.Modelos
{
    public class VistaAzulDbContext : DbContext
    {
        public VistaAzulDbContext(DbContextOptions<VistaAzulDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<AmaDeLlaves> AmasDeLlaves { get; set; } = null!;
        public DbSet<Habitacion> Habitaciones { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<Traza> Trazas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CI)
                .IsUnique();

            modelBuilder.Entity<AmaDeLlaves>()
                .HasIndex(a => a.CI)
                .IsUnique();

            modelBuilder.Entity<Habitacion>()
                .HasMany(h => h.AmasDeLlaves)
                .WithMany(a => a.Habitaciones)
                .UsingEntity(j => j.ToTable("HabitacionAmaDeLlaves"));

            // --- SEED DATA ---
            /*
            var habitacionesSeed = new List<Habitacion>();
            for (int piso = 1; piso <= 3; piso++)
                for (int hab = 1; hab <= 5; hab++)
                    habitacionesSeed.Add(new Habitacion { Numero = piso * 10 + hab, EstaFueraDeServicio = false });

            modelBuilder.Entity<Habitacion>().HasData(habitacionesSeed);

            modelBuilder.Entity<Cliente>().HasData(
                new Cliente { Id = 1, NombreApellidos = "Juan Perez Gomez",       CI = "99010212345", NumeroTelefono = "+5352345678", EsVIP = false },
                new Cliente { Id = 2, NombreApellidos = "Maria Carmen Rodriguez", CI = "95051254321", NumeroTelefono = "+5358765432", EsVIP = true  },
                new Cliente { Id = 3, NombreApellidos = "Carlos Diaz Gutierrez",  CI = "98122598765", NumeroTelefono = "+5351112223", EsVIP = false }
            );

            modelBuilder.Entity<AmaDeLlaves>().HasData(
                new AmaDeLlaves { Id = 1, NombreApellidos = "Elena Garcia Fernandez", CI = "85031445678", NumeroTelefono = "+5353334445" },
                new AmaDeLlaves { Id = 2, NombreApellidos = "Rosa Martinez Perez",    CI = "89110298765", NumeroTelefono = "+5355556667" }
            );
            */
        }
    }
}