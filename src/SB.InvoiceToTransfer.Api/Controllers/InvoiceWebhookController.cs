using MediatR;
using Microsoft.AspNetCore.Mvc;
using SB.InvoiceToTransfer.Application.UseCases.ProcessInvoiceCredit;
using SB.InvoiceToTransfer.Application.Webhooks;

[ApiController]
[Route("api/webhooks/invoices")]
public sealed class InvoiceWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<InvoiceWebhookController> _logger;

    public InvoiceWebhookController(
        IMediator mediator,
        ILogger<InvoiceWebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("credit")]
    public async Task<IActionResult> ReceiveInvoiceCredit(
    [FromBody] InvoiceCreditWebhookRequest request,
    CancellationToken cancellationToken)
    {
        if (request?.Data?.Invoice is null)
        {
            _logger.LogWarning("Invalid webhook payload received");
            return BadRequest();
        }

        if (request.Event != "invoice.paid")
        {
            _logger.LogWarning("Invalid event type received for invoice {InvoiceId} credit processing: {Event} ", request.Data.Invoice.Id, request.Event);
            return Ok();
        }

        var invoice = request.Data.Invoice;

        using (_logger.BeginScope(
            "InvoiceExternalId: {ExternalId}",
            invoice.Id))
        {
            var result = await _mediator.Send(
                new ProcessInvoiceCreditCommand
                {
                    InvoiceExternalId = invoice.Id,
                    Amount = invoice.Amount,
                    Fee = invoice.Fee
                },
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Invoice credit processing finished with status: {Reason}",
                    result.Reason);
            }
        }

        return Ok();
    }
}