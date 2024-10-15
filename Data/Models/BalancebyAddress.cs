using Cardano.Sync.Data.Models;

namespace Argus_BAVer2.Data.Models;

public record BalanceByAddress() : IReducerModel
{
    public string Address { get; set; } = default!;
    public ulong Balance { get; set; } = default!;
}