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
            var quantity = random.Next(8, 13);

            var externalIds = new List<string>();

            var faker = new Faker("pt_BR");

            for (int i = 0; i < quantity; i++)
            {
                var name = faker.Name.FullName();
                var taxId = faker.Person.Cpf();
                var amount = random.Next(1000, 10000);
                var dueDate = DateTime.UtcNow.AddDays(3);

                var externalId = await _starkBankClient.CreateInvoiceAsync(
                    name,
                    taxId,
                    amount,
                    dueDate,
                    cancellationToken);

                var invoice = new Invoice(externalId, amount);

                await _invoiceRepository.AddAsync(invoice, cancellationToken);

                externalIds.Add(externalId);
            }

            return new CreateInvoicesResult(
                quantity,
                externalIds,
                DateTime.UtcNow
            );
        }
    }
}
