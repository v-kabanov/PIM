namespace PimWeb.Models;

public class FileSearchViewModel : SearchModelBase
{
    public FileSearchViewModel()
    {
    }

    /// <summary>
    ///     Copies user input only
    /// </summary>
    /// <param name="other">
    ///     Mandatory
    /// </param>
    public FileSearchViewModel(SearchModelBase other)
        :base(other)
    {
    }

    public int FileId { get; set; }

    public FileListViewModel SearchResultPage { get; set; } = new ();
}