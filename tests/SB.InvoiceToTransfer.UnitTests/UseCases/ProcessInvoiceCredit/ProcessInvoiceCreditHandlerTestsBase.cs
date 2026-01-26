using Microsoft.Extensions.Logging;
using Moq;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit;

namespace SB.InvoiceToTransfer.UnitTests.UseCases.ProcessInvoiceCredit
{
    public abstract class ProcessInvoiceCreditHandlerTestsBase
    {
        protected readonly Mock<IInvoiceRepository> InvoiceRepository = new();
        protected readonly Mock<IStarkBankClient> StarkBankClient = new();
        protected readonly Mock<ILogger<ProcessInvoiceCreditHandler>> Logger = new();

        protected ProcessInvoiceCreditHandler CreateHandler()
            => new(
                InvoiceRepository.Object,
                StarkBankClient.Object,
                Logger.Object);
    }
}
