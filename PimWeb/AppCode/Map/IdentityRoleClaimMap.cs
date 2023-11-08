using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityRoleClaimMap : ClassMapBase<IdentityRoleClaim<int>>
{
    /// <inheritdoc />
    public IdentityRoleClaimMap()
    {
        Id(x => x.Id).GeneratedBy.Sequence("aspnet_identity_id_seq");
        Table("aspnet_role_claims");
        
        Map(x => x.ClaimType)
            .Length(1024)
            .Not.Nullable();
        
        Map(x => x.ClaimValue)
            .Length(1024)
            .Not.Nullable();
        
        Map(x => x.RoleId)
            .Not.Nullable();
    }
}