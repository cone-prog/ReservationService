using ReservationService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAllServices(builder.Configuration);

var app = builder.Build();

app.UseAppServices();

app.Run();
