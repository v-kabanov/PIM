using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserClaimMap : ClassMapBase<IdentityUserClaim<int>>
{
    /// <inheritdoc />
    public IdentityUserClaimMap()
    {
        Id(x => x.Id).GeneratedBy.Sequence("aspnet_identity_id_seq");
        Table("aspnet_user_claims");
        
        Map(x => x.ClaimType)
            .Length(1024)
            .Not.Nullable();
        
        Map(x => x.ClaimValue)
            .Length(1024)
            .Not.Nullable();
        
        Map(x => x.UserId)
            .Not.Nullable();
    }
}