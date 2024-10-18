using CardanoSharp.Wallet.Models.Addresses;

namespace Argus_BAVer2.Data.Extension;



public static class AddressExtension
{
    /// <summary>
    ///  This method uses the Address class in CardanoSharp.Wallet.Models.Addresses 
    ///  to convert the hex/byte addres into a Bech32 encoded (human-readable) address
    /// </summary>
    public static string? ToBech32(this byte[] address)
    {
        try
        {
            return new Address(address).ToString();
        }
        catch
        {
            return null;
        }
    }

    public static Address? ToAddress(this byte[] address)
    {
        try
        {
            return new Address(address);
        }
        catch
        {
            return null;
        }
    }
}