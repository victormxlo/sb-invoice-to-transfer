using FluentAssertions;
using Moq;
using SB.InvoiceToTransfer.Application.Abstractions.External;
using SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit;
using SB.InvoiceToTransfer.Domain;
using SB.InvoiceToTransfer.Domain.Enums;
using SB.InvoiceToTransfer.UnitTests.Domain.Factories;

namespace SB.InvoiceToTransfer.UnitTests.Application.UseCases.ProcessInvoiceCredit
{
    public class ProcessInvoiceCreditHandlerTests
        : ProcessInvoiceCreditHandlerTestsBase
    {
        [Fact]
        public async Task Handle_ShouldReturnNotProcessed_WhenInvoiceNotFound()
        {
            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Invoice)null!);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = "inv_123",
                    Amount = 100,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Invoice not found");

            StarkBankClient.Verify(
                c => c.CreateTransferAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldIgnore_WhenInvoiceAlreadyPaid()
        {
            var invoice = InvoiceTestFactory.Paid();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 100,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Already processed");

            StarkBankClient.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ShouldReturnNotProcessed_WhenAmountIsInvalid()
        {
            var invoice = InvoiceTestFactory.Created();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 0,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Invalid financial values");

            StarkBankClient.Verify(
                c => c.CreateTransferAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotProcessed_WhenNetAmountIsInvalid()
        {
            var invoice = InvoiceTestFactory.Created();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 10,
                    Fee = 20
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Invalid net amount");

            StarkBankClient.Verify(
                c => c.CreateTransferAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldIgnore_WhenInvoiceIsNotInCreatedStatus()
        {
            var invoice = InvoiceTestFactory.Processing();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 100,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Already processed");

            StarkBankClient.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_ShouldNotMarkAsPaid_WhenTransferFails()
        {
            var invoice = InvoiceTestFactory.Created();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            StarkBankClient
                .Setup(c => c.CreateTransferAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    StarkBankOperationResult<string>.Fail("ERR", "error"));

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 100,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Reason.Should().Be("Transfer failed");

            invoice.Status.Should().Be(InvoiceStatus.Processing);
        }

        [Fact]
        public async Task Handle_ShouldProcessInvoiceSuccessfully()
        {
            var invoice = InvoiceTestFactory.Created();

            InvoiceRepository
                .Setup(r => r.GetByExternalIdAsync(invoice.ExternalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoice);

            StarkBankClient
                .Setup(c => c.CreateTransferAsync(90, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    StarkBankOperationResult<string>.Ok("tr_123"));

            var handler = CreateHandler();

            var result = await handler.Handle(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.ExternalId,
                    Amount = 100,
                    Fee = 10
                },
                CancellationToken.None);

            result.Success.Should().BeTrue();
            result.TransferId.Should().Be("tr_123");

            invoice.Status.Should().Be(InvoiceStatus.Paid);
        }
    }
}
