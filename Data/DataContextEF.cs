using Microsoft.EntityFrameworkCore;
using DotNetAPI.Models;

namespace DotNetAPI.Data;

public class DataContextEF : DbContext
{
    private readonly IConfiguration _configuration; // Can only be assigned at declaration or in a constructor

    /// <summary>
    /// Constrcutor
    /// </summary>
    /// <param name="configuration"></param>
    public DataContextEF(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnectionString"),
            optionsBuilder => optionsBuilder.EnableRetryOnFailure());
        }
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSalary> UserSalary { get; set; }

    public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TutorialAppSchema");

        modelBuilder.Entity<User>().ToTable("Users", "TutorialAppSchema").HasKey(u => u.UserId);
                modelBuilder.Entity<UserSalary>().ToTable("UserSalary", "TutorialAppSchema").HasKey(u => u.UserId);
                        modelBuilder.Entity<UserJobInfo>().ToTable("UserJobInfo", "TutorialAppSchema").HasKey(u => u.UserId);
    }
}