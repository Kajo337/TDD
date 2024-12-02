using TDDProgram;
using TransactionStatus = TDDProgram.TransactionStatus;
public class PaymentProcessorTests
{
    [Fact]
    public void ProcesujPlatnosc_PowinnoZwracacSukces_GdyPlatnoscZostalaPrzetworzonaPomyœlnie()
    {
        // Arrange
        var gateway = new StubPaymentGateway
        {
            ChargeResult = new TransactionResult(true, "trans123", "Obci¹¿enie zakoñczone sukcesem.")
        };
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.ProcessPayment("user123", 150.00);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("trans123", result.TransactionId);
        Assert.Equal("Obci¹¿enie zakoñczone sukcesem.", result.Message);
        Assert.Equal("P³atnoœæ przetworzona pomyœlnie.", logger.LastLog);
    }

    [Fact]
    public void ProcesujPlatnosc_PowinnoZwracacNiepowodzenie_GdyUserIdJestPusty()
    {
        // Arrange
        var gateway = new StubPaymentGateway();
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.ProcessPayment("", 150.00);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.TransactionId);
        Assert.Equal("Nieprawid³owy userId: pole jest puste.", result.Message);
        Assert.Equal("Nieprawid³owy userId: pole jest puste.", logger.LastLog);
    }

    [Fact]
    public void ProcesujPlatnosc_PowinnoZwracacNiepowodzenie_GdyKwotaJestUjemna()
    {
        // Arrange
        var gateway = new StubPaymentGateway();
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.ProcessPayment("user123", -50.00);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.TransactionId);
        Assert.Equal("Kwota musi byæ dodatnia.", result.Message);
        Assert.Equal("Kwota musi byæ dodatnia.", logger.LastLog);
    }

    [Fact]
    public void DokonajZwrotu_PowinnoZwracacSukces_GdyZwrotZostaniePrzetworzonyPomyœlnie()
    {
        // Arrange
        var gateway = new StubPaymentGateway
        {
            RefundResult = new TransactionResult(true, "trans123", "Zwrot zakoñczony sukcesem.")
        };
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.RefundPayment("trans123");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("trans123", result.TransactionId);
        Assert.Equal("Zwrot zakoñczony sukcesem.", result.Message);
        Assert.Equal("Zwrot przetworzony pomyœlnie.", logger.LastLog);
    }

    [Fact]
    public void PobierzStatusPlatnosci_PowinnoZwracacStatus_GdyTransakcjaIstnieje()
    {
        // Arrange
        var gateway = new StubPaymentGateway
        {
            GetStatusResult = TransactionStatus.COMPLETED
        };
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var status = processor.GetPaymentStatus("trans123");

        // Assert
        Assert.Equal(TransactionStatus.COMPLETED, status);
        Assert.Equal("Stan p³atnoœci: COMPLETED", logger.LastLog);
    }

    [Fact]
    public void PobierzStatusPlatnosci_PowinnoRzucacWyjatek_GdyTransactionIdJestPusty()
    {
        // Arrange
        var gateway = new StubPaymentGateway();
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => processor.GetPaymentStatus(""));
        Assert.Equal("Nieprawid³owy transactionId: pole jest puste.", ex.Message);
    }
}

public class StubPaymentGateway : PaymentGateway
{
    public TransactionResult ChargeResult { get; set; }
    public TransactionResult RefundResult { get; set; }
    public TransactionStatus GetStatusResult { get; set; }

    public TransactionResult Charge(string userId, double amount)
    {
        return ChargeResult;
    }

    public TransactionResult Refund(string transactionId)
    {
        return RefundResult;
    }

    public TransactionStatus GetStatus(string transactionId)
    {
        return GetStatusResult;
    }
}

public class StubLogger : ILogger
{
    public string LastLog { get; private set; }

    public void Log(string message)
    {
        LastLog = message;
    }
}