using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOutputCache()
    .AddStackExchangeRedisCache(x =>
    {
        //x.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        x.InstanceName = "TestRedisCache";
        x.Configuration = "localhost:6379";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseOutputCache();

app.MapGet("/generate-guid-key", async (IDistributedCache cache) =>
{
    var key = "_Key1";

    var cachedData = await cache.GetStringAsync(key);

    if (cachedData is null)
    {
        await Task.Delay(3000);

        var id = Guid.NewGuid().ToString();

        await cache.SetStringAsync(key, id, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
        });

        cachedData = id;
    }

    return cachedData;
})
.Produces<string>()
.WithTags("Redis Cache");

app.UseAuthorization();

app.MapControllers();

app.Run();
