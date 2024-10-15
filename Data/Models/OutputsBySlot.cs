using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;

public record OutputsBySlot() : IReducerModel
{
    public string TxHash { get; set; } = default!;
    public ulong TxIndex { get; set; } = default!;
    public string Address { get; set; } = default!;
    public ulong Value { get; set; } = default!;
    public ulong Slot { get; set; } = default!;
}