using Microsoft.EntityFrameworkCore;

namespace PimWeb.AppCode;

public class DatabaseContext : DbContext
{
    private const string IndexNameNoteSearch = "idx_Note_Search";
    private AppOptions AppOptions { get; }
    
    /// <inheritdoc />
    protected DatabaseContext(AppOptions appOptions)
    {
        AppOptions = appOptions;
    }

    /// <inheritdoc />
    public DatabaseContext(DbContextOptions<DatabaseContext> options, AppOptions appOptions) : base(options)
    {
        AppOptions = appOptions;
    }
    
    public DbSet<Note> Notes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(b =>
        {
            b.ToTable("Note", "public");
            b.HasKey(x => x.Id);
            //b.UseXminAsConcurrencyToken();
            b.Property(x => x.Id).UseIdentityByDefaultColumn();
            b.Property(x => x.Text).IsRequired().UsePropertyAccessMode(PropertyAccessMode.Property);
            b.Property(x => x.IntegrityVersion).IsConcurrencyToken();
            b.Property(x => x.CreateTime).HasColumnType("timestamp without time zone");
            b.Property(x => x.LastUpdateTime).HasColumnType("timestamp without time zone");
            b.HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    AppOptions.FulltextConfig ?? "english",
                    p => new { p.Text })  // Included properties
                .HasIndex(p => p.SearchVector, IndexNameNoteSearch)
                .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
        });
    }
}