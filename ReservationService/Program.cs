using ReservationService;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.UseApiServices();

app.Run();
