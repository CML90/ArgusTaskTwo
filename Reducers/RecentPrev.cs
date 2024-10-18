// using Cardano.Sync.Reducers;
// using Microsoft.EntityFrameworkCore;
// using PallasDotnet.Models;
// using Argus_BAVer2.Data.Models;
// using Argus_BAVer2.Data.Extension;
// using System.Linq.Expressions;
// using Microsoft.VisualBasic;
// using CardanoSharp.Wallet.Extensions.Models;

// namespace Argus_BAVer2.Data.Reducers;

// public class RecentPrev
// (
//     IDbContextFactory<BalanceByAddressDbContext> dbContextFactory,
//     IConfiguration configuration,
//     ILogger<RecentPrev> logger
// ) : IReducer<BalanceByAddress> 
// {
//     public async Task RollForwardAsync(NextResponse response)
//     {
//         await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();

//         await ProcessOutputsAsync(response.Block.Slot, response.Block, _dbContext);
//         await ProcessInputsAsync(response.Block.Slot, response.Block, _dbContext);

//         await _dbContext.SaveChangesAsync();
//         await _dbContext.DisposeAsync();
//     }

//     private async Task ProcessInputsAsync(ulong slot, Block block, BalanceByAddressDbContext _dbContext)
//     {
//         //get all the block's input hash and index
//         var txInputs = block.TransactionBodies
//             .SelectMany(tx => tx.Inputs.Select(i => i.Id.ToHex().ToLowerInvariant() + i.Index))
//             .ToList();

//         //match those hashes and indices with outputs in the DB (from previous blocks)
//         var matchedDbOutputs = await _dbContext.OutputsBySlot
//             .Where(o => txInputs.Contains(o.TxHash + o.TxIndex)) 
//             .ToListAsync();

//         // or in Local (from preceding ProcessingOutputs Call) - maybe an output in the same block will be used as input in the tx right after it
//         //depending on which reducer runs first? - check after creation of input and outputs by slots
//         var matchedOutputsFromLocal = _dbContext.OutputsBySlot.Local
//             .Where(o => txInputs.Contains(o.TxHash + o.TxIndex))
//             .ToList();

//         //merge
//         var allMatched = matchedDbOutputs.Concat(matchedOutputsFromLocal).ToList();
    
//         //get the addresses for the input
//         var relevantAddresses = allMatched
//             .Select(o => o.Address)
//             .Distinct()
//             .ToList();

//         //Check the DB and local for those addresses
        

//         //Get those addresses and subtract their balance

//         foreach(TransactionBody tx in block.TransactionBodies)
//         {
//             foreach(TransactionInput input in tx.Inputs)
//             {

//             }
//         }

        
//     }

//     private async Task ProcessOutputsAsync(ulong slot, Block block, BalanceByAddressDbContext _dbContext)
//     {
//         //get all the output's addresses within the block
//         var blockAddresses = block.TransactionBodies
//             .SelectMany(tx => tx.Outputs.Select(o => o.Address.Raw.ToBech32()))
//             .Distinct()
//             .ToList();

//         //get all the addresses in the DB that match blockAddresses
//         var existingAddresses = await _dbContext.BalanceByAddress
//             .AsNoTracking()
//             .Where(ba => blockAddresses.Contains(ba.Address))
//             .Select(ba => ba.Address)
//             .ToListAsync();

//         foreach(TransactionBody tx in block.TransactionBodies)
//         {
//             foreach(TransactionOutput output in tx.Outputs)
//             {
//                 //for each transaction output in the block's transactions

//                 string? Bech32Addr = output.Address.Raw.ToBech32();
//                 if (Bech32Addr is null || !Bech32Addr.StartsWith("addr")) continue;

//                 var localAddress = _dbContext.BalanceByAddress.Local
//                     .FirstOrDefault(ba => ba.Address == Bech32Addr);

//                 //not in Local - has not been added yet
//                 if(localAddress == null)
//                 {
//                     BalanceByAddress newBba = new(){
//                         Address = Bech32Addr,
//                         Balance = output.Amount.Coin
//                     };

//                     _dbContext.BalanceByAddress.Add(newBba);
//                 }
//                 else
//                 {
//                     localAddress.Balance += output.Amount.Coin;
//                 }
//             }
            
//         }
//     }

//     public async Task RollBackwardAsync(NextResponse response)
//     {
//         await using BalanceByAddressDbContext _dbContext = await dbContextFactory.CreateDbContextAsync();
//     }
// }