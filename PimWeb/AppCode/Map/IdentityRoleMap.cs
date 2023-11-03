//using FluentNHibernate.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityRoleMap : ClassMapBase<IdentityRole<int>>
{
    /// <inheritdoc />
    public IdentityRoleMap()
    {
        Table("aspnet_roles");
        Id(x => x.Id).GeneratedBy.Sequence("aspnet_identity_id_seq");
        Map(x => x.Name)
            .Length(64)
            .Not.Nullable()
            .Unique();
        
        Map(x => x.NormalizedName)
            .Length(64)
            .Not.Nullable()
            .Unique();
        
        Map(x => x.ConcurrencyStamp)
            .Length(36);
    }
}