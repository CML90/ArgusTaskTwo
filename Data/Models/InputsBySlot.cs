using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;

/// <summary>
/// This entity is used to store Input history that facilitates the RollBackward state.
/// <remarks>
/// TxHash and OutputIndex forms the primary key.
/// It is used to track the Value being sent by an Address
/// </remarks>
/// <remarks>
/// Slot is used to track the transactions and actions to be reverted
/// </remarks>
/// </summary>
public record InputsBySlot() : IReducerModel
{  
    public string TxHash { get; set; } = default!;
    public ulong TxIndex { get; set; } = default!;
    public ulong Slot { get; set; } = default!;
}
