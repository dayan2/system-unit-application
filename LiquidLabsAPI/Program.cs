using LiquidLabs.Dto;
using LiquidLabs.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ConnectionStringsOptions>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<ConfigurationSettings>(builder.Configuration.GetSection("ConfigurationSettings"));
builder.Services.AddHttpClient<SystemUnitService>();
builder.Services.AddSingleton<ISystemUnitService, SystemUnitService>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

