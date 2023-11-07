using System;

namespace PimWeb.Models;
/// <summary>
///     Entity which exposes <see cref="Code"/> string property.
/// </summary>
public interface IHaveCode
{
    string Code { get; }
}

public interface IHaveName
{
    string Name { get; }
}

/// <summary>
///     Entity which exposes <see cref="IHaveCode.Code"/> string property which is its unique business key.
/// </summary>
public interface IHaveUniqueCode : IHaveCode
{
}

public interface IHaveCodeAndName : IHaveCode, IHaveName
{
}
    
public interface IHaveCodeNameAndDescription : IHaveCodeAndName
{
    /// <summary>
    ///     Potentially long multipline text that is not suitable for putting into a caption.
    /// </summary>
    public string Description { get; }
}

public interface IHaveOptionalIntegerId : IHaveSomeId
{
    new int? Id { get; }
}

public interface IHaveIntegerId : IHaveSomeId
{
    new int Id { get; }
}

public interface IHaveSomeId
{
    object Id { get; }
}

public class ListItem
{
    public object Value { get; set; }
    public string Code { get; set; }
    public string Text { get; set; }
}

/// <summary>
///     Generic model suitable for any entity with string business key (<see cref="Code"/>) and user-friendly <see cref="Name"/>.
/// </summary>
public class EntityWithCodeAndNameViewModel : IHaveCodeAndName, IEquatable<EntityWithCodeAndNameViewModel>, IHaveOptionalIntegerId, IHaveSomeId
{
    /// <summary>
    ///     Compact key valid only on the specific MES node. <see cref="Code"/> on the other hand is invariant across different MES nodes.
    /// </summary>
    public int? Id { get; set; }

    /// <inheritdoc />
    object IHaveSomeId.Id => Id;

    /// <summary>
    ///     Business key.
    /// </summary>
    public string Code { get; set; }

    public string Name { get; set; }

    public EntityWithCodeAndNameViewModel()
    {
    }

    public EntityWithCodeAndNameViewModel(IHaveCodeAndName other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        Code = other.Code;
        Name = other.Name;
            
        if (other is IHaveOptionalIntegerId oid)
            Id = oid.Id;
        else if (other is IHaveIntegerId iid)
            Id = iid.Id;
    }

    public EntityWithCodeAndNameViewModel(EntityWithCodeAndNameViewModel other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));

        Id = other.Id;
        Code = other.Code;
        Name = other.Name;
    }

    /// <inheritdoc />
    public bool Equals(EntityWithCodeAndNameViewModel other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (Id > 0 && other.Id > 0)
            return Id == other.Id;
            
        return string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((EntityWithCodeAndNameViewModel) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Code?.GetHashCode() ?? 0;

    /// <inheritdoc />
    public override string ToString() => string.IsNullOrWhiteSpace(Name)
        ? Code
        : $"{Name}, {Code}";
}

public class EntityWithCodeNameAndDescriptionViewModel : EntityWithCodeAndNameViewModel, IHaveCodeNameAndDescription
{
    /// <inheritdoc />
    public string Description { get; set; }
}
