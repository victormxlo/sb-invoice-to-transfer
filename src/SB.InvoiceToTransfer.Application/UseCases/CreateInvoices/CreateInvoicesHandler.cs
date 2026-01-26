using Bogus;
using Bogus.Extensions.Brazil;
using MediatR;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.Application.UseCases.CreateInvoices
{
    public class CreateInvoicesHandler : IRequestHandler<CreateInvoicesCommand, CreateInvoicesResult>
    {
        private readonly IStarkBankClient _starkBankClient;
        private readonly IInvoiceRepository _invoiceRepository;

        public CreateInvoicesHandler(
            IStarkBankClient starkBankClient,
            IInvoiceRepository invoiceRepository)
        {
            _starkBankClient = starkBankClient;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<CreateInvoicesResult> Handle(
            CreateInvoicesCommand request, CancellationToken cancellationToken)
        {
            var random = new Random();
            var faker = new Faker("pt_BR");

            var quantity = random.Next(8, 13);

            var invoices = Enumerable.Range(0, quantity)
                .Select(_ => new Invoice(
                    recipientName: faker.Name.FullName(),
                    amount: random.Next(1000, 10000),
                    taxId: faker.Person.Cpf(),
                    email: faker.Person.Email,
                    dueDate: DateTime.UtcNow.AddDays(3)))
                .ToList();

            var result = await _starkBankClient.CreateInvoicesAsync(invoices, cancellationToken);

            if (!result.Success)
            {
                return new CreateInvoicesResult(
                    Quantity: 0,
                    ExternalInvoiceIds: Array.Empty<string>(),
                    ExecutedAt: DateTime.UtcNow
                );
            }

            var externalIds = result.Data!.ToList();

            if (externalIds.Count != invoices.Count)
            {
                throw new InvalidOperationException(
                    $"Mismatch between invoices ({invoices.Count}) and external IDs ({externalIds.Count}) returned by StarkBankClient.");
            }

            for (int i = 0; i < invoices.Count; i++)
            {
                invoices[i].AssignExternalId(externalIds[i]);

                // Protection against local reprocessing
                if (!await _invoiceRepository.ExistsByExternalIdAsync(externalIds[i], cancellationToken))
                {
                    await _invoiceRepository.AddAsync(invoices[i], cancellationToken);
                }
            }

            return new CreateInvoicesResult(
                Quantity: invoices.Count,
                ExternalInvoiceIds: externalIds,
                ExecutedAt: DateTime.UtcNow
            );
        }
    }
}
