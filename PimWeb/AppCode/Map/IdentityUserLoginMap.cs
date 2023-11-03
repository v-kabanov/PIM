using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Map;

public class IdentityUserLoginMap : ClassMapBase<IdentityUserLogin<int>>
{
    /// <inheritdoc />
    public IdentityUserLoginMap()
    {
        Table("aspnet_user_logins");
        CompositeId().KeyProperty(x => x.LoginProvider).KeyProperty(x => x.ProviderKey);
        
        Map(x => x.ProviderDisplayName)
            .Length(32)
            .Not.Nullable();
        
        Map(x => x.UserId)
            .Not.Nullable();
    }
}