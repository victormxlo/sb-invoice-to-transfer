namespace SB.InvoiceToTransfer.Infrastructure.Configuration
{
    public static class Secrets
    {
        public static string Require(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Missing required environment variable: {name}");

            return value;
        }
    }
}
