using FintechCheckout.Configuration;
using FintechCheckout.Data;
using Microsoft.EntityFrameworkCore;
using FintechCheckout.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PaystackSettings>(
    builder.Configuration.GetSection("Paystack"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=fintech.db"));

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