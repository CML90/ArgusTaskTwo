using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Argus_BAVer2.Data.Models;
using Argus_BAVer2.Data.Extension;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

namespace Argus_BAVer2.Data.Reducers;

public class OutputsBySlotReducer
(
    IDbContextFactory<BalanceByAddressDbContext> dbContextFactory/*,
    IConfiguration configuration,
    ILogger<OutputsBySlotReducer> logger*/
) : IReducer<OutputsBySlot>
{
    public async Task RollForwardAsync(NextResponse response)
    {
        Console.WriteLine("OUTPUTS RUNNING");
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

        IEnumerable<TransactionBody> transactions = response.Block.TransactionBodies;

        foreach (TransactionBody tx in transactions)
        {
            ProcessOutputsAsync(response.Block.Slot, tx, _dbContext);
            //await ProcessInputsAsync(response.Block.Slot, tx, _dbContext);
        }

        await _dbContext.SaveChangesAsync();
        await _dbContext.DisposeAsync();
    }

    /*private async Task ProcessInputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {

    }*/

    private void ProcessOutputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {
        //Insert Outputs
        string txHash = tx.Id.ToHex().ToLowerInvariant();

        foreach (TransactionOutput output in tx.Outputs)
        {
            string? Bech32Addr = output.Address.Raw.ToBech32();
            if (Bech32Addr is null || !Bech32Addr.StartsWith("addr")) continue;

            OutputsBySlot NewOutput = new()
            {
                TxHash = txHash,
                TxIndex = output.Index,
                Address = Bech32Addr,
                Value = output.Amount.Coin,
                Slot = block
            };

            _dbContext.OutputsBySlot.Add(NewOutput);
        }
    }

    public async Task RollBackwardAsync(NextResponse response)
    {
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

        ulong rollbackSlot = response.Block.Slot;

        List<OutputsBySlot> outputRollbackEntries = await _dbContext.OutputsBySlot
            .AsNoTracking()
            .Where(tr => tr.Slot > rollbackSlot)
            .ToListAsync();

        _dbContext.OutputsBySlot.RemoveRange(outputRollbackEntries);

        await _dbContext.SaveChangesAsync();
    }
}