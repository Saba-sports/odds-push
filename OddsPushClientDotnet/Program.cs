using Microsoft.EntityFrameworkCore;
using OddsPushClient.Consumers;
using OddsPushClient.Data;
using OddsPushClient.Services;
using OddsPushClient.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SQLite
builder.Services.AddDbContext<SportsbookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sportsbook.db"));

// Enable CORS
builder.Services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(
            corsPolicyBuilder => { corsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
    });

// Register Consumers and background services
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IBetTypeService, BetTypeService>();
builder.Services.AddSingleton<IHeartbeatMonitor, HeartbeatMonitor>();
builder.Services.AddSingleton<RawMessageConsumer>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.UseCors();

// IMPORTANT: Middleware order matters for CORS
// We run the heartbeat check AFTER Cors but before any endpoint processing
app.UseHeartbeatCheck();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Ensure Database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SportsbookDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
