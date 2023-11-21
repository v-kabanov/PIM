namespace PimWeb.AppCode.Map;

public class FileMap : ClassMapVersionedBase<File>
{
    public FileMap()
    {
        Id(x => x.Id).GeneratedBy.Sequence("file_id_seq");
        
        Map(x => x.Title)
            .Not.Nullable()
            .Length(100);
        
        Map(x => x.RelativePath)
            .Not.Nullable()
            .Length(8000);
        
        Map(x => x.Description);
        Map(x => x.ExtractedText);
        
        Map(x => x.CreateTime);
        Map(x => x.LastUpdateTime);
        
        Map(x => x.ContentHash);
        
        HasManyToMany(x => x.Notes)
            .Table("note_file")
            .ParentKeyColumn("file_id")
            .ChildKeyColumn("note_id")
            .Inverse()
            .NotFound.Exception()
            .Cascade.None();
    }
}