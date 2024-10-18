using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;

/// <summary>
/// This entity is used to store the different TransactionOutputs.
/// It includes the details that allow for Balance updates in the BalancebyAddress entity.
/// <remarks>
/// TxHash and TxIndex form the primary key.
/// </remarks>
/// <remarks>
/// Address is who is receiving assets.
/// </remarks>
/// <remarks>
/// Value is the amount of Lovelace being sent to the Address
/// </remarks>
/// <remarks>
/// Slot is used to track the transactions and actions to be reverted
/// </remarks>
/// </summary>
public record OutputsBySlot() : IReducerModel
{
    public string TxHash { get; set; } = default!;
    public ulong TxIndex { get; set; } = default!; //inconsistent naming 
    public string Address { get; set; } = default!;
    public ulong Value { get; set; } = default!;
    public ulong Slot { get; set; } = default!;
}