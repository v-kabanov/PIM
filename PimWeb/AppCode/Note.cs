﻿using System.Diagnostics.CodeAnalysis;

namespace PimWeb.AppCode;

public class Note : IEquatable<Note>
{
    private string _text;

    private int _hashCode;

    public int Id { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    ///     0 for unsaved, incremented every time it's updated in the storage
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    ///     Register update
    /// </summary>
    /// <returns>
    ///     New version
    /// </returns>
    public int IncrementVersion()
    {
        return ++Version;
    }

    /// <summary>
    ///     Name is just cached first line of text; should not be persisted separately
    /// </summary>
    public string Name { get; private set; }

    public string Text
    {
        get { return _text; }
        set
        {
            Name = ExtractName(value);
            _text = value;
        }
    }

    /// <summary>
    ///     Id is assigned after note is saved
    /// </summary>
    public bool IsTransient => Version == 0;

    public static Note Create(string text)
    {
        var result = new Note()
        {
            Text = text,
            CreateTime = DateTime.Now,
            LastUpdateTime = DateTime.Now,
            Version = 0
        };

        if (string.IsNullOrEmpty(result.Name))
            return null;

        return result;
    }

    /// <returns>
    ///     null if text contains any valid text (non-blank-space)
    /// </returns>
    private static string ExtractName(string text)
    {
        if (text == null)
            return null;

        var firstLine = StringHelper.ExtractFirstLine(text);

        if (firstLine?.Length > 150)
            return StringHelper.GetTextWithLimit(firstLine, 0, 100, false);

        return firstLine;
    }

    public static string CreateShortGuid()
    {
        var encoded = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        encoded = encoded
            .Replace("/", "_")
            .Replace("+", "-");

        return encoded.Substring(0, 22);
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

    public bool Equals(Note other)
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