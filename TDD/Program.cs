namespace TDDProgram
{

    public interface PaymentGateway
    {
        TransactionResult Charge(string userId, double amount);
        TransactionResult Refund(string transactionId);
        TransactionStatus GetStatus(string transactionId);
    }

    public interface ILogger
    {
        void Log(string message);
    }

    public class PaymentProcessor
    {
        private readonly PaymentGateway _gateway;
        private readonly ILogger _logger;

        public PaymentProcessor(PaymentGateway gateway, ILogger logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        public TransactionResult ProcessPayment(string userId, double amount)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var message = "Nieprawidłowy userId: pole jest puste.";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }

            if (amount <= 0)
            {
                var message = "Kwota musi być dodatnia.";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }

            try
            {
                var result = _gateway.Charge(userId, amount);
                if (result.Success)
                {
                    _logger.Log("Płatność przetworzona pomyślnie.");
                }
                else
                {
                    _logger.Log($"Płatność nie powiodła się: {result.Message}");
                }
                return result;
            }
            catch (NetworkException ex)
            {
                var message = $"Błąd sieciowy: płatność nieudana. {ex.Message}";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }
            catch (PaymentException ex)
            {
                var message = $"Błąd płatności: {ex.Message}";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }
        }

        public TransactionResult RefundPayment(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                var message = "Nieprawidłowy transactionId: pole jest puste.";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }

            try
            {
                var result = _gateway.Refund(transactionId);
                if (result.Success)
                {
                    _logger.Log("Zwrot przetworzony pomyślnie.");
                }
                else
                {
                    _logger.Log($"Zwrot nie powiódł się: {result.Message}");
                }
                return result;
            }
            catch (NetworkException ex)
            {
                var message = $"Błąd sieciowy podczas zwrotu: {ex.Message}";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }
            catch (RefundException ex)
            {
                var message = $"Błąd zwrotu: {ex.Message}";
                _logger.Log(message);
                return new TransactionResult(false, null, message);
            }
        }

        public TransactionStatus GetPaymentStatus(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                throw new ArgumentException("Nieprawidłowy transactionId: pole jest puste.");
            }

            try
            {
                var status = _gateway.GetStatus(transactionId);
                _logger.Log($"Stan płatności: {status}");
                return status;
            }
            catch (NetworkException)
            {
                throw new Exception("Błąd sieciowy podczas pobierania statusu.");
            }
        }
    }

    public class TransactionResult
    {
        public bool Success { get; }
        public string TransactionId { get; }
        public string Message { get; }

        public TransactionResult(bool success, string transactionId, string message)
        {
            Success = success;
            TransactionId = transactionId;
            Message = message;
        }
    }

    public enum TransactionStatus
    {
        PENDING,
        COMPLETED,
        FAILED
    }

    public class NetworkException : Exception
    {
        public NetworkException(string message) : base(message) { }
    }

    public class PaymentException : Exception
    {
        public PaymentException(string message) : base(message) { }
    }

    public class RefundException : Exception
    {
        public RefundException(string message) : base(message) { }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }

}