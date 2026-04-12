using Microsoft.EntityFrameworkCore;
using TicketAPI.Data;
using TicketAPI.Interfaces;
using TicketAPI.Services;
using TicketAPI.Services.Processes; // Namespace del Manager

var builder = WebApplication.CreateBuilder(args);

// Connection strings desde appsetings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); ;

// Registro con Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();
builder.Services.AddScoped<TicketManager>(); 

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(); 
app.UseAuthorization();
app.MapControllers();

app.Run();