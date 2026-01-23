using FluentAssertions;
using Moq;
using SB.InvoiceToTransfer.Application.Interfaces.External;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.UseCases.CreateInvoices;
using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.UnitTests.UseCases.CreateInvoices
{
    public class CreateInvoicesHandlerTests
    {
        private readonly Mock<IStarkBankClient> _starkBankClientMock;
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly CreateInvoicesHandler _handler;

        public CreateInvoicesHandlerTests()
        {
            _starkBankClientMock = new Mock<IStarkBankClient>();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            _starkBankClientMock
                .Setup(client =>
                    client.CreateInvoiceAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<decimal>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Guid.NewGuid().ToString());

            _handler = new CreateInvoicesHandler(
                _starkBankClientMock.Object, _invoiceRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateBetween8And12Invoices()
        {
            var command = new CreateInvoicesCommand();

            var result = await _handler.Handle(
                command, CancellationToken.None);

            result.Quantity.Should().BeInRange(8, 12);

            result.ExternalInvoiceIds.Should().HaveCount(result.Quantity);

            _starkBankClientMock.Verify(
                client => client.CreateInvoiceAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(result.Quantity));

            _invoiceRepositoryMock.Verify(
                repo => repo.AddAsync(
                    It.IsAny<Invoice>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(result.Quantity));
        }
    }
}
