using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Argus_BAVer2.Data.Models;
using Argus_BAVer2.Data.Extension;
using System.Linq.Expressions;
using Microsoft.VisualBasic;

namespace Argus_BAVer2.Data.Reducers;

public class InputsBySlotReducer
(
    IDbContextFactory<BalanceByAddressDbContext> dbContextFactory//,
    //IConfiguration configuration,
    //ILogger<InputsBySlotReducer> logger
) : IReducer<InputsBySlot>
{
    public async Task RollForwardAsync(NextResponse response)
    {
        Console.WriteLine("INPUTS RUNNING");
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();
        IEnumerable<TransactionBody> transactions = response.Block.TransactionBodies;

        foreach (TransactionBody tx in transactions)
        {
            //await ProcessOutputsAsync(response.Block.Slot, tx, _dbContext);
            ProcessInputsAsync(response.Block.Slot, tx, _dbContext);
        }

        await _dbContext.SaveChangesAsync();
        await _dbContext.DisposeAsync();
    }

    private void ProcessInputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {
        //insert Inputs
        string txHash = tx.Id.ToHex().ToLowerInvariant();

        foreach (TransactionInput input in tx.Inputs)
        {
            InputsBySlot InsertInput = new()
            {
                TxHash = input.Id.ToHex().ToLowerInvariant(),
                TxIndex = input.Index,
                Slot = block
            };

            _dbContext.InputsBySlot.Add(InsertInput);
        }
    }

    /*private async Task ProcessOutputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {

    }*/

    public async Task RollBackwardAsync(NextResponse response)
    {
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

        ulong rollbackSlot = response.Block.Slot;

        List<InputsBySlot> inputRollbackEntries = await _dbContext.InputsBySlot
            .AsNoTracking()
            .Where(tr => tr.Slot > rollbackSlot)
            .ToListAsync();

        _dbContext.InputsBySlot.RemoveRange(inputRollbackEntries);

        await _dbContext.SaveChangesAsync();
    }
}