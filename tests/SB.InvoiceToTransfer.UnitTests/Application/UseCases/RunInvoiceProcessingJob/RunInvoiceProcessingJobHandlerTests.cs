using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SB.InvoiceToTransfer.Application.Interfaces.Repositories;
using SB.InvoiceToTransfer.Application.Models;
using SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit;
using SB.InvoiceToTransfer.Application.UseCases.RunInvoiceProcessingJob;
using SB.InvoiceToTransfer.Domain;
using SB.InvoiceToTransfer.Domain.Enums;
using SB.InvoiceToTransfer.UnitTests.Domain.Factories;

namespace SB.InvoiceToTransfer.UnitTests.Application.UseCases.RunInvoiceProcessingJob
{
    public class RunInvoiceProcessingJobHandlerTests
    {
        private readonly Mock<IInvoiceProcessingJobStateRepository> _stateRepositoryMock;
        private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<RunInvoiceProcessingJobHandler>> _loggerMock;

        private readonly RunInvoiceProcessingJobHandler _handler;

        public RunInvoiceProcessingJobHandlerTests()
        {
            _stateRepositoryMock = new Mock<IInvoiceProcessingJobStateRepository>();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<RunInvoiceProcessingJobHandler>>();

            _handler = new RunInvoiceProcessingJobHandler(
                _stateRepositoryMock.Object,
                _invoiceRepositoryMock.Object,
                _mediatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSkipExecution_WhenSchedulerIsAlreadyRunning()
        {
            var command = new RunInvoiceProcessingJobCommand();

            _stateRepositoryMock
                .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InvoiceProcessingJobState());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Executed.Should().BeFalse();
            result.Reason.Should().Be("Scheduler already running");

            _stateRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _invoiceRepositoryMock.Verify(
                r => r.GetByStatusAsync(It.IsAny<InvoiceStatus>(), It.IsAny<CancellationToken>()),
                Times.Never);

            _mediatorMock.Verify(
                m => m.Send<ProcessInvoiceCreditResult>(
                    It.IsAny<ProcessInvoiceCreditCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldProcessInvoices_WhenNoActiveSchedulerExists()
        {
            var command = new RunInvoiceProcessingJobCommand();

            var invoiceOne = InvoiceTestFactory.Processing();
            invoiceOne.AssignExternalId("inv-1");
            invoiceOne.AssignAmountPaid(10m);

            var invoiceTwo = InvoiceTestFactory.Processing();
            invoiceTwo.AssignExternalId("inv-2");
            invoiceTwo.AssignAmountPaid(20m);

            var invoices = new List<Invoice> { invoiceOne, invoiceTwo };

            InvoiceProcessingJobState? capturedState = null;

            _stateRepositoryMock
                .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((InvoiceProcessingJobState?)null);

            _stateRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()))
                .Callback<InvoiceProcessingJobState, CancellationToken>((state, _) =>
                    capturedState = state)
                .Returns(Task.CompletedTask);

            _invoiceRepositoryMock
                .Setup(r => r.GetByStatusAsync(InvoiceStatus.Processing, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invoices);

            _mediatorMock
                .Setup(m => m.Send(
                    It.IsAny<ProcessInvoiceCreditCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(ProcessInvoiceCreditResult.Ok("tx-1"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Executed.Should().BeTrue();
            result.ProcessedInvoices.Should().Be(2);

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<ProcessInvoiceCreditCommand>(c => c.InvoiceExternalId == "inv-1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(
                m => m.Send(
                    It.Is<ProcessInvoiceCreditCommand>(c => c.InvoiceExternalId == "inv-2"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _stateRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _stateRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Once);

            capturedState.Should().NotBeNull();
            capturedState!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldFinalizeSchedulerState_EvenWhenExceptionOccurs()
        {
            var command = new RunInvoiceProcessingJobCommand();

            InvoiceProcessingJobState? capturedState = null;

            _stateRepositoryMock
                .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((InvoiceProcessingJobState?)null);

            _stateRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()))
                .Callback<InvoiceProcessingJobState, CancellationToken>((state, _) =>
                    capturedState = state)
                .Returns(Task.CompletedTask);

            _invoiceRepositoryMock
                .Setup(r => r.GetByStatusAsync(InvoiceStatus.Processing, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database failure"));

            Func<Task> act = async () =>
                await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();

            _stateRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _stateRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Once);

            capturedState.Should().NotBeNull();
            capturedState!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldReturnZeroProcessed_WhenNoInvoicesFound()
        {
            var command = new RunInvoiceProcessingJobCommand();

            _stateRepositoryMock
                .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((InvoiceProcessingJobState?)null);

            _invoiceRepositoryMock
                .Setup(r => r.GetByStatusAsync(InvoiceStatus.Processing, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Invoice>());

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Executed.Should().BeTrue();
            result.ProcessedInvoices.Should().Be(0);

            _mediatorMock.Verify(
                m => m.Send<ProcessInvoiceCreditResult>(
                    It.IsAny<ProcessInvoiceCreditCommand>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _stateRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<InvoiceProcessingJobState>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
