
// DO NOT REFERENCE!
//using System.Data.Entity;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using log4net;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Pim.CommonLib;
using PimWeb.AppCode.Map;
using PimWeb.Models;
using ISession = NHibernate.ISession;

namespace PimWeb.AppCode;

public record SortPropertyOption (SortProperty SortProperty, SelectListItem SelectListItem);

public class NoteService : INoteService
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public const int DefaultPageSize = 20;
    
    public ISession Session { get; }

    public int PageSize => DefaultPageSize;
    
    public AppOptions AppOptions { get;  }
    
    public ITextExtractor TextExtractor { get;  }
    
    private IQueryable<Note> Query => Session.Query<Note>();
    
    private IQueryable<File> FileQuery => Session.Query<File>();
    
    private static readonly List<SortPropertyOption> AllSortOptions;

    private static readonly KeyValuePair<string, string>[] AdditionalColumnsForFileSearch = new Dictionary<string, string>()
    {
        {"relative_path", nameof(FileSearchResult.RelativePath)}
        , {"title", nameof(FileSearchResult.Title)}
        , {"description", nameof(FileSearchResult.Description)}
    }.ToArray();

    private static readonly KeyValuePair<string, string>[] AdditionalColumnsForNoteSearch = new Dictionary<string, string>()
    {
        {"text", nameof(NoteSearchResult.Text)}
    }.ToArray();

    private static readonly FileExtensionContentTypeProvider FileExtensionContentTypeProvider = new ();

    static NoteService()
    {
        AllSortOptions = Enum.GetValues<SortProperty>()
            .Select(x => (Value: x, Text: $"{x}"))
            .Select(x => new SortPropertyOption(x.Value, new SelectListItem(Util.InsertDelimitersIntoPascalCaseString(x.Text), x.Text)))
            .ToList();
    }
    
    public NoteService(ISession session, AppOptions appOptions, ITextExtractor textExtractor)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        AppOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
        TextExtractor = textExtractor ?? new TextExtractor();
    }

    public async Task<NoteSearchViewModel> SearchAsync(NoteSearchViewModel model, bool withDelete)
    {
        if (withDelete && model.NoteId > 0)
            await Query.Where(x => x.Id == model.NoteId).DeleteAsync().ConfigureAwait(false);

        var genericResult = await SearchAsync<NoteSearchResult>(model, "note", "search", AdditionalColumnsForNoteSearch);
        
        var result = new NoteSearchViewModel(genericResult.Model)
        {
            SearchResultPage = genericResult.Results.Select(CreateModel).ToList()
        };
        
        return result;
    }
    
    public async Task<FileSearchViewModel> SearchAsync(FileSearchViewModel model, bool withDelete)
    {
        if (withDelete && model.FileId > 0)
            await DeleteAsync(new FileViewModel { Id = model.FileId } ).ConfigureAwait(false);
            //await DeleteFileAsync(model.FileId).ConfigureAwait(false);

        var genericResult = await SearchAsync<FileSearchResult>(model, "file", "search_files", AdditionalColumnsForFileSearch);
        
        var result = new FileSearchViewModel(genericResult.Model)
        {
            SearchResultPage = new FileListViewModel
            {
                Files = genericResult.Results.Select(CreateModel).ToList(),
                StatusSuccess = true
            } 
        };
        
        return result;
    }

    private async Task<(SearchModelBase Model, List<TBean> Results)> SearchAsync<TBean>(SearchModelBase model, string tableName, string searchFunctionName, params KeyValuePair<string, string>[] additionalColumns)
    {
        var sortProperty = model.SortProperty ?? SortProperty.LastUpdateTime;
        
        var sortAscending = model.SortAscending;
        
        var whereClauseBuilder = new SqlWhereClauseBuilder()
            .AddOptionalRangeParameters(NoteMap.ColumnNameLastUpdateTime, model.LastUpdatePeriodStart, model.LastUpdatePeriodEnd)
            .AddOptionalRangeParameters(NoteMap.ColumnNameCreationTime, model.CreationPeriodStart, model.CreationPeriodEnd);
        
        var queryText = new StringBuilder();
        
        var useSqlFunction = !model.Query.IsNullOrWhiteSpace();
        
        void AddColumns()
        {
            foreach(var c in additionalColumns)
                queryText.Append(", ").Append(c.Key).Append(" as \"").Append(c.Value).Append('\"');
        }
        
        if (useSqlFunction)
        {
            queryText.Append("select id as \"Id\", last_update_time as \"LastUpdateTime\", create_time as \"CreateTime\", headline as \"Headline\", rank as \"Rank\"");
            AddColumns();
            queryText.AppendLine().Append(" from public.").Append(searchFunctionName).AppendLine("(:query, :fuzzy)");
        }
        else
        {
            queryText.Append("select id as \"Id\", last_update_time as \"LastUpdateTime\", create_time as \"CreateTime\", cast(null as text) as \"Headline\", cast(null as real) as \"Rank\"");
            AddColumns();
            queryText.AppendLine().Append(" from public.").AppendLine(tableName);
            
            if (sortProperty == SortProperty.SearchRank)
                sortProperty = SortProperty.LastUpdateTime;
        }

        if (!whereClauseBuilder.IsEmpty)
            queryText.Append("where     ").AppendLine(whereClauseBuilder.GetCombinedClause());
        
        var maxRowsToCount = (model.PageNumber + 10) * PageSize;
        
        var totalCountQuery = Session.CreateSQLQuery($"select count(*) from ({queryText} limit :maxRowsToCount) s")
            .SetInt32("maxRowsToCount", maxRowsToCount);
        if (useSqlFunction)
            totalCountQuery
                .SetString("query", model.Query)
                .SetBoolean("fuzzy", model.Fuzzy);
        whereClauseBuilder.SetParameterValues(totalCountQuery);

        // the function sorts by rank, reorder only if different
        if (!useSqlFunction || !(sortProperty == SortProperty.SearchRank && !sortAscending))
        {
            var columnName = sortProperty switch
            {
                SortProperty.CreationTime => NoteMap.ColumnNameCreationTime
                , SortProperty.LastUpdateTime => NoteMap.ColumnNameLastUpdateTime
                , SortProperty.SearchRank => nameof(SearchResultBase.Rank)
                , _ => throw new PostconditionException($"Unexpected sort property '{sortProperty}'.")
            };
            if (!columnName.IsNullOrEmpty())
            {
                var ascendance = sortAscending ? "asc" : "desc";
                queryText.Append("order by ").Append(columnName).Append(' ').Append(ascendance).Append(", id ").AppendLine(ascendance);
            }
        }
        
        var countFuture = totalCountQuery.FutureValue<long>();
        
        var query = Session.CreateSQLQuery(queryText.ToString())
            .ApplyPage(PageSize, model.PageNumber - 1);
        
        whereClauseBuilder.SetParameterValues(query);
        
        if (useSqlFunction)
            query
                .SetString("query", model.Query)
                .SetBoolean("fuzzy", model.Fuzzy);
        
        query.SetResultTransformer(Transformers.AliasToBean(typeof(TBean)));
        
        var results = (await query.Future<TBean>().GetEnumerableAsync().ConfigureAwait(false))
            .ToList();

        var totalCount = countFuture.Value;
        
        if (results.Count > PageSize)
            results.RemoveAt(results.Count - 1);

        var pageCount = (int)Math.Ceiling((double)totalCount / PageSize);
        var processedSearchModel = new SearchModelBase(model)
        {
            TotalCountedPageCount = pageCount,
            HasMore = pageCount > (model.PageNumber + 1),
            SortProperty = sortProperty,
            SortAscending = sortAscending,
            SortOptions = AllSortOptions.Select(x => new SelectListItem(x.SelectListItem.Text, x.SelectListItem.Value, x.SortProperty == sortProperty))
                .ToList(),
            PageNumber = model.PageNumber
        };
        
        return (processedSearchModel, results);
    }
    
    public async Task<HomeViewModel> GetLatestNotesAsync()
    {
        var notes = await Query
            .OrderByDescending(x => x.LastUpdateTime)
            .Take(PageSize)
            .ToListAsync()
            .ConfigureAwait(false);
        
        return new HomeViewModel { LastUpdatedNotes = notes.Select(x => CreateModel(x, false)).ToList() };
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> GetNoteAsync(int id)
    {
        var note = await Session.GetAsync<Note>(id).ConfigureAwait(false);
        
        var result = note != null ? CreateModel(note) : new NoteViewModel();
        
        return result;
    }

    /// <inheritdoc />
    public async Task<FileViewModel> GetFileAsync(int id)
    {
        var file = await Session.GetAsync<File>(id).ConfigureAwait(false);
        
        var result = file != null ? CreateModel(file) : new FileViewModel();
        
        if (result.ExistsOnDisk)
        {
            var currentContentHash = await CalculateHashAsync(System.IO.File.ReadAllBytes(result.FullPath)).ConfigureAwait(false);
            result.ContentHashMismatch = currentContentHash.Length != file.ContentHash?.Length || !currentContentHash.SequenceEqual(file.ContentHash);
        }
        
        return result;
    }

    /// <inheritdoc />
    public async Task<NoteViewModel> SaveOrUpdateAsync(NoteViewModel model)
    {
        var note = await Session.GetAsync<Note>(model.Id).ConfigureAwait(false);
        
        var newText = model.NoteText?.Trim();
        
        if (note != null)
        {
            if (note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Version mismatch: updating {model.Version ?? 0} while server has {note.IntegrityVersion}.");

            if (newText != note.Text)
            {
                note.Text = model.NoteText;
                note.LastUpdateTime = DateTime.UtcNow;
            }
        }
        else
        {
            note = new Note { Text = model.NoteText };
            await Session.SaveAsync(note).ConfigureAwait(false);
        }
        
        await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        return CreateModel(note);
    }

    /// <inheritdoc />
    public async Task<Note> DeleteAsync(NoteViewModel model, bool skipConcurrencyCheck)
    {
        var note = await Session.GetAsync<Note>(model.Id).ConfigureAwait(false);
        
        if (note != null)
        {
            if (!skipConcurrencyCheck && note.IntegrityVersion != model.Version)
                throw new OptimisticConcurrencyException($"Note was updated on the server: deleting {model.Version ?? 0} while server has {note.IntegrityVersion}. Review changes before deleting.");

            await Session.DeleteAsync(note).ConfigureAwait(false);
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        }
        
        return note;
    }

    /// <inheritdoc />
    public async Task<FileViewModel> DeleteAsync(FileViewModel model)
    {
        var file = await Session.GetAsync<File>(model.Id).ConfigureAwait(false);
        
        if (file == null)
            throw new ArgumentException($"File #{model.Id} does not exist.");
        
        var fullPath = GetFullFilePathInStorage(file.RelativePath);
        var fileInfo = new FileInfo(fullPath);
        var existedOnDisk = fileInfo.Exists;
        if (existedOnDisk)
            fileInfo.Delete();
        else
            Log.ErrorFormat("File #{0} ({1}) does not exist when being deleted", model.Id, fullPath);
        
        foreach(var note in file.Notes)
            note.Files.Remove(file);
        
        await Session.DeleteAsync(file).ConfigureAwait(false);
        await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        var result = CreateModel(file);
        result.ExistsOnDisk = existedOnDisk;
        
        return result;
    }

    /// <inheritdoc />
    public async Task<HomeViewModel> CreateNoteAsync(HomeViewModel model)
    {
        var created = !model.NewNoteText.IsNullOrWhiteSpace();
        if (created)
        {
            var newNote = Note.Create(model.NewNoteText);
            await Session.SaveAsync(newNote).ConfigureAwait(false);
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        }
        
        var result = await GetLatestNotesAsync().ConfigureAwait(false);
        
        if (!created)
            result.NewNoteText = model.NewNoteText;
        
        return result;
    }
    
    /// <inheritdoc />
    public async Task<FileUploadResultViewModel> UploadFileForNoteAsync(int noteId, string fileName, byte[] content)
    {
        var saveResult = await SaveFileAsync(fileName, content).ConfigureAwait(false);
        
        var note = await Query
            .Where(x => x.Id == noteId)
            //saving 1 roundtrip in this way prohibits the use of simple 'FirstOrDefault' as it applies to database rows rather than parent entities
            //.FetchMany(x => x.Files)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
        
        if (note == null)
            return new FileUploadResultViewModel
            {
                UploadedFile = CreateModel(saveResult.File, false),
                StatusMessage = $"Note #{noteId} does not exist."
            };
        
        if (Session.GetCurrentTransaction() == null)
            Session.BeginTransaction();
        
        note.Files.Add(saveResult.File);
        
        var files = note.Files
            .OrderBy(x => x.Title)
            .ThenBy(x => x.Id)
            .ToList();
        
        await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        var result = new FileUploadResultViewModel
        {
            UploadedFile = CreateModel(saveResult.File, false),
            Files = files.Select(x => CreateModel(x, false)).ToList(),
            Duplicate = saveResult.Duplicate,
            StatusSuccess = true,
            StatusMessage = saveResult.Duplicate
                ? $"Duplicate file detected; '{saveResult.File.Title}' (#{saveResult.File.Id}) linked instead"
                : $"File '{saveResult.File.Title}' uploaded and linked."
        };
        
        return result;
    }
    
    /// <inheritdoc />
    public async Task<FileNoteUnlinkResultViewModel> UnlinkNoteFromFile(int fileId, int noteId)
    {
        var noteFuture = Query.Where(x => x.Id == noteId).ToFutureValue();

        var file = await FileQuery.Where(x => x.Id == fileId)
            .FetchMany(x => x.Notes)
            .ToFutureValue()
            .GetValueAsync()
            .ConfigureAwait(false);
        
        if (file == null || noteFuture.Value == null)
        {
            return new FileNoteUnlinkResultViewModel
            {
                StatusMessage = noteFuture.Value == null
                    ? $"Note #{noteId} does not exist."
                    : $"File #{fileId} does not exist.",
            };
        }
        
        var unlinked = file.Notes.Remove(noteFuture.Value);
        
        if (unlinked)
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        var result = new FileNoteUnlinkResultViewModel
        {
            Note = CreateModel(noteFuture.Value, false),
            Notes = file.Notes.OrderBy(x => x.Name).ThenBy(x => x.Id).Select(x => CreateModel(x, false)).ToList(),
            StatusSuccess = true,
            StatusMessage = unlinked
                ? $"Association with file '{noteFuture.Value.Name}' removed."
                : $"Note '{noteFuture.Value.Name}' was not associated with file '{file.Title}' (#{file.Id})."
        };
        
        return result;
    }
    
    /// <inheritdoc />
    public async Task<NoteFileUnlinkResultViewModel> UnlinkFileFromNote(int noteId, int fileId)
    {
        var fileFuture = FileQuery.Where(x => x.Id == fileId).ToFutureValue();

        var note = await Query.Where(x => x.Id == noteId)
            .FetchMany(x => x.Files)
            .ToFutureValue()
            .GetValueAsync()
            .ConfigureAwait(false);
        
        if (note == null || fileFuture.Value == null)
        {
            return new NoteFileUnlinkResultViewModel
            {
                StatusMessage = note == null
                    ? $"Note #{noteId} does not exist."
                    : $"File #{fileId} does not exist.",
            };
        }
        
        var unlinked = note.Files.Remove(fileFuture.Value);
        
        if (unlinked)
            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        var result = new NoteFileUnlinkResultViewModel
        {
            File = CreateModel(fileFuture.Value, false),
            Files = note.Files.OrderBy(x => x.Title).ThenBy(x => x.Id).Select(x => CreateModel(x, false)).ToList(),
            StatusSuccess = true,
            StatusMessage = unlinked
                ? $"Association with file '{fileFuture.Value.Title}' removed."
                : $"File '{fileFuture.Value.Title}' was not associated with note '{note.Name}' (#{note.Id})."
        };
        
        return result;
    }
    
    public async Task<(File File, bool Duplicate)> SaveFileAsync(string fileName, byte[] content)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        if (content == null) throw new ArgumentNullException(nameof(content));
        if (content.Length == 0) throw new ArgumentException("File is empty", nameof(content));

        var existingFiles = await GetExistingFiles(content).ConfigureAwait(false);
        if (existingFiles.Files.Count > 0)
        {
            Log.WarnFormat("File '{0}' already exists as {1} (#{2})", fileName, existingFiles.Files[0].RelativePath, existingFiles.Files[0].Id);
            return (existingFiles.Files[0], true);
        }
        
        var hash = existingFiles.Hash;

        var now = DateTime.UtcNow;
        
        var relativeDir = GetFileStorageDirectoryRelativePath(now);
        var relativePath = Path.Combine(relativeDir, fileName);
        relativePath = MakeUniqueRelativeFileStoragePath(relativePath);
        
        var fullPath = GetFullFilePathInStorage(relativePath);
        
        Log.InfoFormat("Saving file '{0}'", fullPath);
        
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        
        await System.IO.File.WriteAllBytesAsync(fullPath, content)
            .ConfigureAwait(false);

        using var memoryStream = new MemoryStream(content, false);
        
        var file = new File
        {
            RelativePath = relativePath,
            Title = fileName,
            ContentHash = hash,
            ExtractedText = TextExtractor.Extract(fileName, memoryStream)
        };
       
        await Session.SaveAsync(file).ConfigureAwait(false);
        await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        
        return (file, false);
    }

    /// <inheritdoc />
    public async Task<FileViewModel> UpdateAsync(FileViewModel model)
    {
        var entity = await Session.GetAsync<File>(model.Id).ConfigureAwait(false);

        if (entity == null) throw new ArgumentException($"File #{model.Id} does not exist.");

        if (entity.IntegrityVersion != model.Version)
            throw new OptimisticConcurrencyException($"Version mismatch: updating {model.Version ?? 0} while server has {entity.IntegrityVersion}.");
        
        var newDescription = model.Description?.Trim();
        var newTitle = model.Title?.Trim();

        if (newDescription != entity.Description || newTitle != entity.Title)
        {
            entity.Title = newTitle;
            entity.Description = newDescription;
            entity.LastUpdateTime = DateTime.UtcNow;

            await Session.GetCurrentTransaction().CommitAsync().ConfigureAwait(false);
        }

        return CreateModel(entity);

    }
    
    private async Task<(List<File> Files, byte[] Hash)> GetExistingFiles(byte[] content)
    {
        var hash = await CalculateHashAsync(content)
            .ConfigureAwait(false);
        
        var filesWithSameHash = await Session.Query<File>()
            .Where(x => x.ContentHash == hash)
            .ToListAsync()
            .ConfigureAwait(false);
        
        var result = new List<File>();
        foreach (var file in filesWithSameHash)
        {
            var fullPath = GetFullFilePathInStorage(file.RelativePath);
            
            if (!System.IO.File.Exists(fullPath))
            {
                Log.ErrorFormat("File #{0} ({1}) does not exist", file.Id, fullPath);
            }
            else
            {
                var currentFileContent = await System.IO.File.ReadAllBytesAsync(fullPath).ConfigureAwait(false);
                if (content.Length == currentFileContent.Length && content.SequenceEqual(currentFileContent))
                    result.Add(file);
            }
        }
        
        return (result, hash);
    }
    
    private string GetFullFilePathInStorage(string relativePath) => Path.GetFullPath(Path.Combine(AppOptions.FileStoragePath, relativePath));
    
    private string MakeUniqueRelativeFileStoragePath(string path)
    {
        if (!System.IO.File.Exists(GetFullFilePathInStorage(path)))
            return path;
        
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        
        var i = 1;

        do
        {
            path = Path.Combine(dir, $"{name}-{i}{extension}");
            ++i;
        }
        while (System.IO.File.Exists(GetFullFilePathInStorage(path)));
        
        return path;
    }
    
    private async Task<byte[]> CalculateHashAsync(byte[] content)
    {
        using var hasher = SHA256.Create();
        return await hasher.ComputeHashAsync(new MemoryStream(content, false)).ConfigureAwait(false);
    }
    
    private string GetFileStorageDirectoryRelativePath(DateTime time) => $"{time:yyyy/MM-MMM}";

    private NoteViewModel CreateModel(Note note, bool populateFiles = true) => new ()
        {
            Id = note.Id,
            NoteText = note.Text,
            Caption = note.Name,
            Version = note.IntegrityVersion,
            CreateTime = note.CreateTime.ToLocalTime(),
            LastUpdateTime = note.LastUpdateTime.ToLocalTime(),
            Files = new FileListViewModel
            {
                Files = populateFiles ? note.Files.Select(x => CreateModel(x, false)).ToList(): new List<FileViewModel>(),
                StatusSuccess = true
            }
        };

    private NoteViewModel CreateModel(NoteSearchResult note) => new ()
    {
        Id = note.Id,
        NoteText = note.Text,
        Caption = Note.ExtractName(note.Text),
        SearchHeadline = note.Headline,
        Rank = note.Rank,
        CreateTime = note.CreateTime.ToLocalTime(),
        LastUpdateTime = note.LastUpdateTime.ToLocalTime(),
    };

    private FileViewModel CreateModel(FileSearchResult x)
    {
        var result = new FileViewModel
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            RelativePath = x.RelativePath,
            FullPath = GetFullFilePathInStorage(x.RelativePath),
            SearchHeadline = x.Headline,
            Rank = x.Rank,
            CreateTime = x.CreateTime.ToLocalTime(),
            LastUpdateTime = x.LastUpdateTime.ToLocalTime(),
            MimeType = GetMimeTypeFromFileName(x.RelativePath)
        };
        result.ExistsOnDisk = System.IO.File.Exists(result.FullPath);
        return result;
    }

    private FileViewModel CreateModel(File x, bool populateNotes = true)
    {
        if (x == null)
            return new ();
        
        var result = new FileViewModel
        {
            Id = x.Id
            , Title = x.Title
            , Description = x.Description
            , RelativePath = x.RelativePath
            , FullPath = GetFullFilePathInStorage(x.RelativePath)
            , CreateTime = x.CreateTime.ToLocalTime()
            , LastUpdateTime = x.LastUpdateTime.ToLocalTime()
            , ExtractedText = x.ExtractedText
            //, ContentHash = x.ContentHash
            , Version = x.IntegrityVersion
            , MimeType = GetMimeTypeFromFileName(x.RelativePath)
            , Notes = populateNotes ? x.Notes.Select(x => CreateModel(x, false)).ToList(): new List<NoteViewModel>()
        };
        result.ExistsOnDisk = System.IO.File.Exists(result.FullPath);
        return result;
    }

    private string GetMimeTypeFromFileName(string fileName)
    {
        if (FileExtensionContentTypeProvider.TryGetContentType(fileName, out var result))
            return result;
        return null;
    }
}