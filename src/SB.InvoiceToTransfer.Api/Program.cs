using SB.InvoiceToTransfer.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:8080");

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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
