using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;

public record InputsBySlot() : IReducerModel
{
    public string TxHash { get; set; } = default!;
    public ulong OutputIndex { get; set; } = default!;
    public ulong Slot { get; set; } = default!;
}
