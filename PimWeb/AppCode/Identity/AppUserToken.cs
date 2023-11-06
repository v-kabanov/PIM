using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Identity;

public class AppUserToken : IdentityUserToken<int>
{
    protected bool Equals(AppUserToken other) => UserId == other.UserId && LoginProvider == other.LoginProvider && Name == other.Name;

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (this == obj)
            return true;
        return !(obj.GetType() != GetType()) && Equals((AppUserToken) obj);
    }

    public override int GetHashCode() => (UserId.GetHashCode() * 397 ^ LoginProvider.GetHashCode()) * 397 ^ Name.GetHashCode();
}