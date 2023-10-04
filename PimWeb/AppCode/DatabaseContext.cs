using System.Globalization;
using JetBrains.Annotations;
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
    public DatabaseContext([NotNull] DbContextOptions options, AppOptions appOptions) : base(options)
    {
        AppOptions = appOptions;
    }
    
    public DbSet<Note> Notes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.UseKeySequences("Id", "public");
        modelBuilder.HasSequence<int>("NoteId", "public");
        
        modelBuilder.Entity<Note>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseSequence("NoteId", "public");
            b.Property(x => x.Text).IsRequired(); //.UseCompressionMethod("lz4");
            b.Property(x => x.IntegrityVersion).IsConcurrencyToken();
            b.Property(x => x.CreateTime); //.HasColumnType("timestamp");
            b.Property(x => x.LastUpdateTime); //.HasColumnType("timestamp");
            b.HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    AppOptions.FulltextConfig,
                    p => new { p.Text })  // Included properties
                .HasIndex(p => p.SearchVector, IndexNameNoteSearch)
                .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
        });
    }
}