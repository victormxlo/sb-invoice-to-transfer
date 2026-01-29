using SB.InvoiceToTransfer.Infrastructure.Configuration;
using SB.InvoiceToTransfer.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers(opts =>
{
    opts.Filters.Add<ExceptionHandlingFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(
        typeof(SB.InvoiceToTransfer.Application.AssemblyReference).Assembly);
});

var dbPath = Secrets.Require("SB_DB_CONNECTION");
var directory = Path.GetDirectoryName(dbPath);

if (!string.IsNullOrEmpty(directory))
{
    Directory.CreateDirectory(directory);
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
