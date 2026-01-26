namespace SB.InvoiceToTransfer.Application.Abstractions.External
{
    public sealed class StarkBankOperationResult<T>
    {
        public bool Success { get; }
        public T? Data { get; }
        public string? ErrorCode { get; }
        public string? ErrorMessage { get; }

        private StarkBankOperationResult(bool success, T? data, string? errorCode, string? errorMessage)
        {
            Success = success;
            Data = data;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public static StarkBankOperationResult<T> Ok(T data)
            => new(true, data, null, null);

        public static StarkBankOperationResult<T> Fail(string code, string message)
            => new(false, default, code, message);
    }
}
