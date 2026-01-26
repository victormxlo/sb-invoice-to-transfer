using FluentAssertions;
using Moq;
using SB.InvoiceToTransfer.Application.Abstractions.External;
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

            _handler = new CreateInvoicesHandler(
                _starkBankClientMock.Object, _invoiceRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateInvoicesBatch_WhenClientSucceeds()
        {
            var command = new CreateInvoicesCommand();

            _starkBankClientMock
                .Setup(c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyCollection<Invoice> invoices, CancellationToken _) =>
                {
                    var fakeIds = invoices.Select(_ => Guid.NewGuid().ToString());
                    return StarkBankOperationResult<IEnumerable<string>>.Ok(fakeIds);
                });

            _invoiceRepositoryMock
                .Setup(r => r.ExistsByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Quantity.Should().BeInRange(8, 12);
            result.ExternalInvoiceIds.Should().HaveCount(result.Quantity);

            _starkBankClientMock.Verify(
                c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _invoiceRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
                Times.Exactly(result.Quantity));
        }

        [Fact]
        public async Task Handle_ShouldNotDuplicateInvoices_WhenAlreadyExists()
        {
            var command = new CreateInvoicesCommand();

            _starkBankClientMock
                .Setup(c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyCollection<Invoice> invoices, CancellationToken _) =>
                {
                    var fakeIds = invoices.Select(_ => Guid.NewGuid().ToString());
                    return StarkBankOperationResult<IEnumerable<string>>.Ok(fakeIds);
                });

            _invoiceRepositoryMock
                .Setup(r => r.ExistsByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Quantity.Should().BeInRange(8, 12);
            result.ExternalInvoiceIds.Should().HaveCount(result.Quantity);

            _invoiceRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _starkBankClientMock.Verify(
                c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenClientFails()
        {
            var command = new CreateInvoicesCommand();

            _starkBankClientMock
                .Setup(c => c.CreateInvoicesAsync(
                    It.IsAny<IReadOnlyCollection<Invoice>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(StarkBankOperationResult<IEnumerable<string>>.Fail("API_ERROR", "Simulated failure"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Quantity.Should().Be(0);
            result.ExternalInvoiceIds.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldCreateBetween8And12Invoices()
        {
            var command = new CreateInvoicesCommand();

            _starkBankClientMock
                .Setup(c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyCollection<Invoice> invoices, CancellationToken _) =>
                {
                    var fakeIds = invoices.Select(_ => Guid.NewGuid().ToString());
                    return StarkBankOperationResult<IEnumerable<string>>.Ok(fakeIds);
                });

            _invoiceRepositoryMock
                .Setup(r => r.ExistsByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Quantity.Should().BeInRange(8, 12);
            result.ExternalInvoiceIds.Should().HaveCount(result.Quantity);

            _invoiceRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()),
                Times.Exactly(result.Quantity));

            _starkBankClientMock.Verify(
                c => c.CreateInvoicesAsync(It.IsAny<IReadOnlyCollection<Invoice>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
