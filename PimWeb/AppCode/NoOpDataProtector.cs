using Microsoft.AspNetCore.DataProtection;

namespace PimWeb.AppCode;

/// <summary>
///     Protector offering no protection and just passing data through.
/// </summary>
public class NoOpDataProtector : IDataProtector
{
    public IDataProtector CreateProtector(string purpose) => new NoOpDataProtector();

    public byte[] Protect(byte[] plaintext) => plaintext;

    public byte[] Unprotect(byte[] protectedData) => protectedData;
}