using Microsoft.AspNetCore.Identity;

namespace PimWeb.AppCode.Identity;

public class AppUserLogin : IdentityUserLogin<int>
{
    protected bool Equals(AppUserLogin other) => LoginProvider == other.LoginProvider && ProviderKey == other.ProviderKey;

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        if (this == obj)
            return true;
        return !(obj.GetType() != GetType()) && Equals((AppUserLogin) obj);
    }

    public override int GetHashCode() => LoginProvider.GetHashCode() * 397 ^ ProviderKey.GetHashCode();
}