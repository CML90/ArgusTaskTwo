using Cardano.Sync.Reducers;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Argus_BAVer2.Data.Models;
using Argus_BAVer2.Data.Extension;
using System.Linq.Expressions;
using Microsoft.VisualBasic;
using CardanoSharp.Wallet.Extensions.Models;

namespace Argus_BAVer2.Data.Reducers;

public class BalanceByAddressReducer
(
    IDbContextFactory<BalanceByAddressDbContext> dbContextFactory/*,
    IConfiguration configuration,
    ILogger<BalanceByAddressReducer> logger*/
) : IReducer<BalanceByAddress>
{
    public async Task RollForwardAsync(NextResponse response)
    {
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

        //get all the output's addresses within the block
        var blockAddresses = response.Block.TransactionBodies
            .SelectMany(tx => tx.Outputs.Select(o => o.Address.Raw.ToBech32()))
            .Distinct()
            .ToList();

        //get all the addresses in the DB that match blockAddresses
        var existingAddresses = await _dbContext.BalanceByAddress
            .Where(ba => blockAddresses.Contains(ba.Address))
            .ToListAsync();

        //get all the inputs in the block
        var txInputs = response.Block.TransactionBodies
            .SelectMany(tx => tx.Inputs.Select(i => i.Id.ToHex().ToLowerInvariant() + i.Index))
            .ToList();

        //get all the outputs that it matches with
        var matchedDbOutputs = await _dbContext.OutputsBySlot
            .Where(o => txInputs.Contains(o.TxHash + o.TxIndex))
            .ToListAsync();

        IEnumerable<TransactionBody> transactions = response.Block.TransactionBodies;

        foreach (TransactionBody tx in transactions)
        {
            ProcessOutputsAsync(response.Block.Slot, tx, existingAddresses, _dbContext);
            ProcessInputsAsync(response.Block.Slot, tx, matchedDbOutputs, existingAddresses, _dbContext);
        }

        await _dbContext.SaveChangesAsync();
        await _dbContext.DisposeAsync();
    }

    private void ProcessInputsAsync(ulong slot, TransactionBody tx, List<OutputsBySlot> matchedDbOutputs, List<BalanceByAddress> existingAddresses, BalanceByAddressDbContext _dbContext)
    {

        //Get those addresses and subtract their balance
        foreach (TransactionInput input in tx.Inputs)
        {
            var matchedOutput = matchedDbOutputs
                .FirstOrDefault(o => o.TxHash + o.TxIndex == input.Id.ToHex().ToLowerInvariant() + input.Index);

            if (matchedOutput != null)
            {
                var outputAddress = matchedOutput.Address;
                var updateBalance = existingAddresses
                    .FirstOrDefault(ba => ba.Address == outputAddress);

                if (updateBalance != null)
                {
                    updateBalance.Balance -= matchedOutput.Value;
                }
            }
        }

    }

    private void ProcessOutputsAsync(ulong slot, TransactionBody tx, List<BalanceByAddress> existingAddresses, BalanceByAddressDbContext _dbContext)
    {

        foreach (TransactionOutput output in tx.Outputs)
        {
            //for each transaction output in the block's transactions

            string? Bech32Addr = output.Address.Raw.ToBech32();
            if (Bech32Addr is null || !Bech32Addr.StartsWith("addr")) continue;

            var localAddress = _dbContext.BalanceByAddress.Local
                .FirstOrDefault(ba => ba.Address == Bech32Addr);

            Console.WriteLine($"BECH2: {Bech32Addr}");
            if (existingAddresses != null && existingAddresses.Any())
            {
                foreach (var address in existingAddresses)
                {
                    Console.WriteLine($"Address: {address.Address}, Balance: {address.Balance}");
                }
            }
            else
            {
                Console.WriteLine("No addresses found.");
            }

            //in Local
            if (localAddress != null)
            {
                localAddress.Balance += output.Amount.Coin;
            }
            else if (existingAddresses?.FirstOrDefault(ba => ba.Address == Bech32Addr) != null)
            {
                //adds it in Local
                var dbAddress = existingAddresses.First(ba => ba.Address == Bech32Addr);

                dbAddress.Balance += output.Amount.Coin;
            }
            else //neither in DB nor local
            {
                //also adds to local
                BalanceByAddress newBba = new()
                {
                    Address = Bech32Addr,
                    Balance = output.Amount.Coin
                };

                _dbContext.BalanceByAddress.Add(newBba);
            }
        }

    }


    public async Task RollBackwardAsync(NextResponse response)
    {
        await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

        //undo inputs and outputs, but what if naguna ang Rollback sa Input or Output niya gi remove na ang history?
        ulong rollbackSlot = response.Block.Slot;

        List<OutputsBySlot> outputRollbackEntries = await _dbContext.OutputsBySlot
                .AsNoTracking()
                .Where(tr => tr.Slot > rollbackSlot)
                .ToListAsync();

        List<InputsBySlot> inputRollbackEntries = await _dbContext.InputsBySlot
            .AsNoTracking()
            .Where(tr => tr.Slot > rollbackSlot)
            .ToListAsync();

        var outputAddresses = outputRollbackEntries
            .Select(o => o.Address)
            .Distinct()
            .ToList();

        List<BalanceByAddress> balanceAddressEntries = await _dbContext.BalanceByAddress
            .Where(ba => outputAddresses.Contains(ba.Address))
            .ToListAsync();

        foreach (OutputsBySlot output in outputRollbackEntries)
        {
            //find the address
            var match = balanceAddressEntries
                .FirstOrDefault(ba => ba.Address == output.Address);

            //subtract balance
            if (match != null)
            {
                match.Balance -= output.Value;
            }
        }

        foreach (InputsBySlot input in inputRollbackEntries)
        {
            //find the output - cant use List sa because the outputs might be from blocks that wont be rolled back?
            var findOutput = await _dbContext.OutputsBySlot
                .FirstOrDefaultAsync(os => os.TxHash == input.TxHash && os.TxIndex == input.TxIndex);

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

        await _dbContext.SaveChangesAsync();
    }
}