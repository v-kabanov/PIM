using Microsoft.EntityFrameworkCore;

namespace PimWeb.AppCode;

public class DatabaseContext : DbContext
{
    private const string IndexNameNoteSearch = "note_search_vector_idx";
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
            b.ToTable("note", "public");
            b.HasKey(x => x.Id);
            //b.UseXminAsConcurrencyToken();
            b.Property(x => x.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            b.Property(x => x.Text).HasColumnName("text").IsRequired().UsePropertyAccessMode(PropertyAccessMode.Property);
            b.Property(x => x.IntegrityVersion).HasColumnName("integrity_version").IsConcurrencyToken();
            b.Property(x => x.CreateTime).HasColumnName("create_time").HasColumnType("timestamp with time zone");
            b.Property(x => x.LastUpdateTime).HasColumnName("last_update_time").HasColumnType("timestamp with time zone");
            b.HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    AppOptions.FulltextConfig ?? "english",
                    p => new { p.Text })  // Included properties
                .HasIndex(p => p.SearchVector, IndexNameNoteSearch)
                .HasMethod("gin"); // Index method on the search vector (GIN or GIST)
            
            b.Property(p => p.SearchVector).HasColumnName("search_vector").IsGeneratedTsVectorColumn(AppOptions.FulltextConfig, nameof(Note.Text));
        });
    }
}