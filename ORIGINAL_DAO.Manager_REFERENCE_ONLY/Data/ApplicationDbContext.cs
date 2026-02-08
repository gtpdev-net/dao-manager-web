using Microsoft.EntityFrameworkCore;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Scan> Scans { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Assembly> Assemblies { get; set; }
    public DbSet<PackageReference> PackageReferences { get; set; }
    public DbSet<AssemblyReference> AssemblyReferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Scan
        modelBuilder.Entity<Scan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GitCommitHash).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ShortCommitHash).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RepositoryPath).HasMaxLength(2000).IsRequired();
            entity.HasIndex(e => e.ScanDate);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure Solution
        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniqueIdentifier).HasMaxLength(100).IsRequired();
            entity.Property(e => e.VisualStudioGuid).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.GuidDeterminationMethod).HasMaxLength(50);
            
            entity.HasOne(e => e.Scan)
                .WithMany(s => s.Solutions)
                .HasForeignKey(e => e.ScanId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => new { e.ScanId, e.UniqueIdentifier }).IsUnique();
        });

        // Configure Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniqueIdentifier).HasMaxLength(100).IsRequired();
            entity.Property(e => e.VisualStudioGuid).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.GuidDeterminationMethod).HasMaxLength(50);
            entity.Property(e => e.TargetFramework).HasMaxLength(100);
            entity.Property(e => e.ProjectStyle).HasMaxLength(20);
            
            entity.HasOne(e => e.Scan)
                .WithMany(s => s.Projects)
                .HasForeignKey(e => e.ScanId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ScanId);
            entity.HasIndex(e => new { e.ScanId, e.UniqueIdentifier }).IsUnique();
            entity.HasIndex(e => e.FilePath);
        });

        // Configure Assembly
        modelBuilder.Entity<Assembly>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniqueIdentifier).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.AssemblyFileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.OutputType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ProjectStyle).HasMaxLength(20).IsRequired();
            entity.Property(e => e.TargetFramework).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProjectFilePath).HasMaxLength(2000).IsRequired();
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Assemblies)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.UniqueIdentifier);
        });

        // Configure PackageReference
        modelBuilder.Entity<PackageReference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PackageName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.PackageReferences)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.PackageName);
            entity.HasIndex(e => new { e.ProjectId, e.PackageName, e.Version }).IsUnique();
        });

        // Configure AssemblyReference
        modelBuilder.Entity<AssemblyReference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AssemblyName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.HintPath).HasMaxLength(2000);
            entity.Property(e => e.Version).HasMaxLength(50);
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.AssemblyReferences)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(e => e.ProjectId);
        });
    }
}
