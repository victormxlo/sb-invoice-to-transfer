using SB.InvoiceToTransfer.Domain;

namespace SB.InvoiceToTransfer.UnitTests.Domain.Factories
{
    public static class InvoiceTestFactory
    {
        public static Invoice Created()
            => new Invoice(
                recipientName: "Test User",
                amount: 100,
                taxId: "12345678900",
                email: "test@test.com",
                dueDate: DateTime.UtcNow.AddDays(3));

        public static Invoice Processing()
        {
            var invoice = Created();
            invoice.MarkAsProcessing();
            return invoice;
        }

        public static Invoice Paid()
        {
            var invoice = Created();
            invoice.MarkAsProcessing();
            invoice.MarkAsPaid(100, 10, "tr_test");
            return invoice;
        }
    }
}
