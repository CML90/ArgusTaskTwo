using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Argus_BAVer2.Data.Models;
using Argus_BAVer2.Data.Extension;
using System.Linq.Expressions;
namespace Argus_BAVer2.Data.Reducers;

public class BalanceByAddressReducer
(
    IDbContextFactory<BalanceByAddressDbContext> dbContextFactory,
    IConfiguration configuration,
    ILogger<BalanceByAddressReducer> logger
) : IReducer<BalanceByAddress> //but there are other models now as well
{

    private readonly ILogger<BalanceByAddressReducer> _logger = logger;

    public async Task RollForwardAsync(NextResponse response)
    {
        _logger.LogInformation("Processing new block at slot {slot}", response.Block.Slot);


        using BalanceByAddressDbContext _dbContext = dbContextFactory.CreateDbContext();
        IEnumerable<TransactionBody> transactions = response.Block.TransactionBodies;

        foreach (TransactionBody tx in transactions)
        {
            await ProcessOutputsAsync(response.Block.Slot, tx, _dbContext);
            await ProcessInputsAsync(response.Block.Slot, tx, _dbContext);
        }

        await _dbContext.SaveChangesAsync();
        await _dbContext.DisposeAsync();
    }

    private async Task ProcessInputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {
        /*
            Subtracts Balance of Address Using the Output as Input
            Adds to the InputsBySlot Table
        */

        string txHash = tx.Id.ToHex().ToLowerInvariant();
        //Create a list to use when checking inputs
        List<OutputsBySlot> ExistingOutputs = await _dbContext.OutputsBySlot
            .AsNoTracking()
            .ToListAsync();

        foreach (TransactionInput input in tx.Inputs)
        {
            try
            {
                var Match = ExistingOutputs
                                .FirstOrDefault(eo => eo.TxHash == input.Id.ToHex().ToLowerInvariant() && eo.TxIndex == input.Index);

                if (Match != null)
                {
                    var AffectedAddress = await _dbContext.OutputsBySlot
                                            .Where(os => os.TxHash == Match.TxHash && os.TxIndex == Match.TxIndex)
                                            .Select(os => os.Address)
                                            .FirstOrDefaultAsync();

                    if (AffectedAddress != null)
                    {
                        var existingBalance = await _dbContext.BalanceByAddress
                                                .FirstOrDefaultAsync(ba => ba.Address == AffectedAddress);
                        if (existingBalance != null)
                        {
                            existingBalance.Balance -= Match.Value;
                        }
                    }

                    InputsBySlot InsertInput = new()
                    {
                        TxHash = input.Id.ToHex().ToLowerInvariant(),
                        OutputIndex = input.Index,
                        Slot = block
                    };

                    _dbContext.InputsBySlot.Add(InsertInput);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error: {e}");
            }
        }
    }

    private async Task ProcessOutputsAsync(ulong block, TransactionBody tx, BalanceByAddressDbContext _dbContext)
    {
        /*
            Adds to the OutputsBySlot Table

            Adds to the BalanceByAddress Table
                - Check if the address exists (Grab all the addresses in the table)
                    -> Exists: add balance
                    -> Does Not:  add new row (address, balance)
        */

        string txHash = tx.Id.ToHex().ToLowerInvariant();
        //Create a List - so we don't query to check for the address per transaction
        List<string> ExistingAddresses = await _dbContext.BalanceByAddress
            .AsNoTracking()
            .Select(ba => ba.Address)
            .ToListAsync();

        foreach (TransactionOutput output in tx.Outputs)
        {
            string? Bech32Addr = output.Address.Raw.ToBech32();
            if (Bech32Addr is null || !Bech32Addr.StartsWith("addr")) continue;
            try
            {

                OutputsBySlot NewOutput = new()
                {
                    TxHash = txHash,
                    TxIndex = output.Index,
                    Address = Bech32Addr,
                    Value = output.Amount.Coin,
                    Slot = block
                };

                _dbContext.OutputsBySlot.Add(NewOutput);

                if (ExistingAddresses.Contains(Bech32Addr)) //address exists in DB
                {
                    var existingEntry = await _dbContext.BalanceByAddress
                                            .FirstOrDefaultAsync(ba => ba.Address == Bech32Addr);
                    if (existingEntry != null)
                    {
                        existingEntry.Balance += output.Amount.Coin;
                    }

                }
                else if (_dbContext.BalanceByAddress.Local.FirstOrDefault(ba => ba.Address == Bech32Addr) != null) //address is not in DB but is in Local
                {
                    var LocalAddress = _dbContext.BalanceByAddress.Local.FirstOrDefault(ba => ba.Address == Bech32Addr);

                    if (LocalAddress != null)
                    {
                        LocalAddress.Balance += output.Amount.Coin;
                    }
                }
                else //address is in neither DB or Local
                {
                    BalanceByAddress InsertAddress = new()
                    {
                        Address = Bech32Addr,
                        Balance = output.Amount.Coin
                    };

                    _dbContext.BalanceByAddress.Add(InsertAddress);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error: {e}");
            }
        }
    }


    public async Task RollBackwardAsync(NextResponse response)
    {
        _logger.LogInformation("Rollback at slot {slot}", response.Block.Slot);
        using BalanceByAddressDbContext _dbContext = dbContextFactory.CreateDbContext();
        /*
            Checks the Rollback Slot
            Checks Slot Numbers in Output and Input Table & Undo Input and Output Operations
                - Input Slot > : Get the Value and Address from the Output Table and Add
                - Output Slot > : Subtract using the Address

            Remove All Inputs and Outputs > Slot
        */

        ulong rollbackSlot = response.Block.Slot;
        try
        {
            List<OutputsBySlot> outputRollbackEntries = await _dbContext.OutputsBySlot
            .AsNoTracking()
            .Where(tr => tr.Slot > rollbackSlot)
            .ToListAsync();

            List<InputsBySlot> inputRollbackEntries = await _dbContext.InputsBySlot
                .AsNoTracking()
                .Where(tr => tr.Slot > rollbackSlot)
                .ToListAsync();

            foreach (OutputsBySlot output in outputRollbackEntries)
            {
                //find the address
                var Match = await _dbContext.BalanceByAddress
                                .FirstOrDefaultAsync(ba => ba.Address == output.Address);

                //subtract balance
                if (Match != null)
                {
                    Match.Balance -= output.Value;
                }
            }

            foreach (InputsBySlot input in inputRollbackEntries)
            {
                //find the output
                var findOutput = await _dbContext.OutputsBySlot
                                    .FirstOrDefaultAsync(os => os.TxHash == input.TxHash && os.TxIndex == input.OutputIndex);

                //find the address
                if (findOutput != null)
                {
                    var Match = await _dbContext.BalanceByAddress
                                .FirstOrDefaultAsync(ba => ba.Address == findOutput.Address);
                    //add the balance
                    if (Match != null)
                    {
                        Match.Balance += findOutput.Value;
                    }
                }

            }

            _dbContext.OutputsBySlot.RemoveRange(outputRollbackEntries);
            _dbContext.InputsBySlot.RemoveRange(inputRollbackEntries);

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error: {e}");
        }
    }
}