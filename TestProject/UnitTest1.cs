using TDDProgram;
using TransactionStatus = TDDProgram.TransactionStatus;
public class PaymentProcessorTests
{
    [Fact]
    public void ProcesujPlatnosc_PowinnoZwracacSukces_GdyPlatnoscZostalaPrzetworzonaPomy�lnie()
    {
        // Arrange
        var gateway = new StubPaymentGateway
        {
            ChargeResult = new TransactionResult(true, "trans123", "Obci��enie zako�czone sukcesem.")
        };
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.ProcessPayment("user123", 150.00);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("trans123", result.TransactionId);
        Assert.Equal("Obci��enie zako�czone sukcesem.", result.Message);
        Assert.Equal("P�atno�� przetworzona pomy�lnie.", logger.LastLog);
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
        Assert.Equal("Nieprawid�owy userId: pole jest puste.", result.Message);
        Assert.Equal("Nieprawid�owy userId: pole jest puste.", logger.LastLog);
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
        Assert.Equal("Kwota musi by� dodatnia.", result.Message);
        Assert.Equal("Kwota musi by� dodatnia.", logger.LastLog);
    }

    [Fact]
    public void DokonajZwrotu_PowinnoZwracacSukces_GdyZwrotZostaniePrzetworzonyPomy�lnie()
    {
        // Arrange
        var gateway = new StubPaymentGateway
        {
            RefundResult = new TransactionResult(true, "trans123", "Zwrot zako�czony sukcesem.")
        };
        var logger = new StubLogger();
        var processor = new PaymentProcessor(gateway, logger);

        // Act
        var result = processor.RefundPayment("trans123");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("trans123", result.TransactionId);
        Assert.Equal("Zwrot zako�czony sukcesem.", result.Message);
        Assert.Equal("Zwrot przetworzony pomy�lnie.", logger.LastLog);
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
        Assert.Equal("Stan p�atno�ci: COMPLETED", logger.LastLog);
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
        Assert.Equal("Nieprawid�owy transactionId: pole jest puste.", ex.Message);
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