using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;
/// <summary>
/// This entity is where the API gets the Balance of a given Address
/// <remarks>
/// Address is a primary key, retrieved from a TransactionOutput.
/// </remarks>
/// <remarks>
/// Balance is updated according to the Input and Outputs in the block.
/// It only considers Lovelace
/// </remarks>
/// </summary>
public record BalanceByAddress() : IReducerModel
{
    public string Address { get; set; } = default!;
    public ulong Balance { get; set; } = default!;
}