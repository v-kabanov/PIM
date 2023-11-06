//using FluentNHibernate.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using PimWeb.AppCode.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityRoleMap : ClassMapBase<AppRole>
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

        HasMany(x => x.Claims)
            .Inverse()
            .Cascade.AllDeleteOrphan()
            .KeyColumn("role_id")
            .Cache.ReadWrite();
    }
}