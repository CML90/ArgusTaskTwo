using Cardano.Sync.Extensions;
using Microsoft.EntityFrameworkCore;
using Cardano.Sync.Reducers;
using Cardano.Sync.Data.Models;
using Argus_BAVer2.Data.Reducers;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCardanoIndexer<BalanceByAddressDbContext>(builder.Configuration);
builder.Services.AddSingleton<IReducer<IReducerModel>, BalanceByAddressReducer>(/*["BalanceByAddress","OutputsBySlot","InputsBySlot"]*/);
builder.Services.AddSingleton<IReducer<IReducerModel>, InputsBySlotReducer>();
builder.Services.AddSingleton<IReducer<IReducerModel>, OutputsBySlotReducer>();

var app = builder.Build();

using IServiceScope scope = app.Services.CreateScope();
BalanceByAddressDbContext dbContext = scope.ServiceProvider.GetRequiredService<BalanceByAddressDbContext>();
dbContext.Database.Migrate();

app.MapGet("/api/balance/{address}", async (string address, BalanceByAddressDbContext dbContext) =>
{
    var balanceEntry = await dbContext.BalanceByAddress
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ba => ba.Address == address);

    if (balanceEntry == null)
    {
        return Results.NotFound(new { Message = "Address not found." });
    }

    return Results.Ok(balanceEntry);
});

app.Run();
