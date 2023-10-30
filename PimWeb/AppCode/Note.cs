using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using NpgsqlTypes;

namespace PimWeb.AppCode;

public class Note : IEquatable<Note>
{
    [NotMapped]
    private string _text;

    [NotMapped]
    private int _hashCode;

    public Note()
    {
        CreateTime = LastUpdateTime = DateTime.UtcNow;
    }

    public virtual int Id { get; set; }

    /// <summary>
    ///     UTC
    /// </summary>
    public virtual DateTime CreateTime { get; set; }

    /// <summary>
    ///     UTC
    /// </summary>
    public virtual DateTime LastUpdateTime { get; set; }

    /// <summary>
    ///     0 for unsaved, incremented every time it's updated in the storage
    /// </summary>
    public virtual int IntegrityVersion { get; set; } = 1;
    
    //[Timestamp]
    //public byte[] Version { get; set; }

    /// <summary>
    ///     Name is just cached first line of text; should not be persisted separately
    /// </summary>
    //[NotMapped]
    public virtual string Name { get; protected set; }

    public virtual string Text
    {
        get => _text;
        set
        {
            Name = ExtractName(value);
            _text = value;
        }
    }
    
    //public virtual NpgsqlTsVector SearchVector { get; set; }

    /// <summary>
    ///     Id is assigned after note is saved
    /// </summary>
    public virtual bool IsTransient => IntegrityVersion == 0;

    public static Note Create(string text)
    {
        var result = new Note()
        {
            Text = text,
            IntegrityVersion = 0
        };

        if (string.IsNullOrEmpty(result.Name))
            return null;

        return result;
    }

    /// <returns>
    ///     null if text contains any valid text (non-blank-space)
    /// </returns>
    public static string ExtractName(string text)
    {
        if (text == null)
            return null;

        var firstLine = StringHelper.ExtractFirstLine(text);

        if (firstLine?.Length > 150)
            return firstLine.GetTextWithLimit(0, 100, false);

        return firstLine;
    }

    public override string ToString()
    {
        return $"{Name}#{Id}";
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Note);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        if (_hashCode != 0)
            return _hashCode;

        _hashCode = IsTransient
            ? HashHelper.GetHashCode(Text)
            : HashHelper.GetHashCode(Id);

        return _hashCode;
    }

    public virtual bool Equals(Note other)
    {
        if (other == null)
            return false;

        if (!IsTransient && !other.IsTransient)
            return Id == other.Id;

        if (IsTransient && other.IsTransient)
            return Text == other.Text;

        return false;
    }
}