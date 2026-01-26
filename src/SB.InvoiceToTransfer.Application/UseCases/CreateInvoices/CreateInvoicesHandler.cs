using Bogus;
using Bogus.Extensions.Brazil;
using MediatR;
using Microsoft.Extensions.Logging;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Application.UseCases.CreateInvoices
{
    public class CreateInvoicesHandler : IRequestHandler<CreateInvoicesCommand, CreateInvoicesResult>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IStarkBankClient _starkBankClient;
        private ILogger<CreateInvoicesHandler> _logger;

        public CreateInvoicesHandler(
            IInvoiceRepository invoiceRepository,
            IStarkBankClient starkBankClient,
            ILogger<CreateInvoicesHandler> logger)
        {
            _invoiceRepository = invoiceRepository;
            _starkBankClient = starkBankClient;
            _logger = logger;
        }

        public async Task<CreateInvoicesResult> Handle(
            CreateInvoicesCommand request,
            CancellationToken cancellationToken)
        {
            var executionId = Guid.NewGuid();

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Operation"] = "CreateInvoices",
                ["ExecutionId"] = executionId
            }))
            {
                _logger.LogInformation("Starting invoice creation process");

                var random = new Random();
                var faker = new Faker("pt_BR");
                var quantity = random.Next(8, 13);

                _logger.LogInformation(
                    "Generating {Quantity} invoices",
                    quantity);

                var invoices = Enumerable.Range(0, quantity)
                    .Select(_ => new Invoice(
                        recipientName: faker.Name.FullName(),
                        amount: random.Next(1000, 10000),
                        taxId: faker.Person.Cpf(),
                        email: faker.Person.Email,
                        dueDate: DateTime.UtcNow.AddDays(3)))
                    .ToList();

                _logger.LogInformation(
                    "Invoices generated locally. Sending to Stark Bank");

                var result = await _starkBankClient
                    .CreateInvoicesAsync(invoices, cancellationToken);

                if (!result.Success)
                {
                    _logger.LogError(
                        "Failed to create invoices in Stark Bank. ErrorCode: {ErrorCode}, Message: {Message}",
                        result.ErrorCode,
                        result.ErrorMessage);

                    return new CreateInvoicesResult(
                        Quantity: 0,
                        ExternalInvoiceIds: Array.Empty<string>(),
                        ExecutedAt: DateTime.UtcNow);
                }

                var externalIds = result.Data!.ToList();

                _logger.LogInformation(
                    "Stark Bank returned {Count} external invoice IDs",
                    externalIds.Count);

                if (externalIds.Count != invoices.Count)
                {
                    _logger.LogCritical(
                        "Mismatch between invoices ({InvoicesCount}) and external IDs ({ExternalIdsCount})",
                        invoices.Count,
                        externalIds.Count);

                    throw new InvalidOperationException(
                        $"Mismatch between invoices ({invoices.Count}) and external IDs ({externalIds.Count}) returned by StarkBankClient.");
                }

                for (int i = 0; i < invoices.Count; i++)
                {
                    invoices[i].AssignExternalId(externalIds[i]);

                    if (await _invoiceRepository
                        .ExistsByExternalIdAsync(externalIds[i], cancellationToken))
                    {
                        _logger.LogWarning(
                            "Invoice with ExternalId {ExternalId} already exists locally. Skipping persistence",
                            externalIds[i]);

                        continue;
                    }

                    await _invoiceRepository
                        .AddAsync(invoices[i], cancellationToken);

                    _logger.LogInformation(
                        "Invoice persisted locally. ExternalId: {ExternalId}",
                        externalIds[i]);
                }

                _logger.LogInformation(
                    "Invoice creation process completed successfully");

                return new CreateInvoicesResult(
                    Quantity: invoices.Count,
                    ExternalInvoiceIds: externalIds,
                    ExecutedAt: DateTime.UtcNow);
            }
        }
    }
}
