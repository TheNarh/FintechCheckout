using FintechCheckout.Configuration;
using FintechCheckout.Data;
using Microsoft.EntityFrameworkCore;
using FintechCheckout.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaystackSettings>(
    builder.Configuration.GetSection("Paystack"));

var databaseProvider =
    builder.Configuration["Database:Provider"] ?? "Sqlite";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider.Equals(
        "Postgres",
        StringComparison.OrdinalIgnoreCase))
    {
        var connectionString =
            $"Host={builder.Configuration["PGHOST"]};" +
            $"Port={builder.Configuration["PGPORT"]};" +
            $"Database={builder.Configuration["PGDATABASE"]};" +
            $"Username={builder.Configuration["PGUSER"]};" +
            $"Password={builder.Configuration["PGPASSWORD"]};" +
            $"SSL Mode=Require;" +
            $"Trust Server Certificate=true";

        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite("Data Source=fintech.db");
    }
});

if (databaseProvider.Equals(
    "Postgres",
    StringComparison.OrdinalIgnoreCase))
{
    var postgresConnectionString =
        $"Host={builder.Configuration["PGHOST"]};" +
        $"Port={builder.Configuration["PGPORT"]};" +
        $"Database={builder.Configuration["PGDATABASE"]};" +
        $"Username={builder.Configuration["PGUSER"]};" +
        $"Password={builder.Configuration["PGPASSWORD"]};" +
        $"SSL Mode=Require;" +
        $"Trust Server Certificate=true";

    builder.Services.AddDbContext<PostgresAppDbContext>(options =>
        options.UseNpgsql(postgresConnectionString));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins(
    "http://localhost:5173",
    "https://rind-faction-uncooked.ngrok-free.dev"
)
.AllowAnyHeader()
.AllowAnyMethod();
    });
});

builder.Services.AddControllers();

builder.Services.AddHttpClient<PaystackService>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (databaseProvider.Equals(
    "Postgres",
    StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<PostgresAppDbContext>();

    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("ReactPolicy");

app.MapControllers();
app.Run();