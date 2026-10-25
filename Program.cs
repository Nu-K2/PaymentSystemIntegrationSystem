using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace PaymentSystemIntegrationSystem;

class Program
{
    public static void Main(string[] args)
    {
        Print.Introduction();

        DemonstratePattern();

        PaymentManager manager = new();
        while (true)
        {
            Print.Menu();
            string input = Console.ReadLine()?.Trim() ?? "";

            switch (input)
            {
                case "1":
                    {
                        IPaymentProcessor oldAdapter = new OldPaymentAdapter();
                        ProcessUserPayment(manager, oldAdapter);
                    }
                    break;
                case "2":
                    {
                        IPaymentProcessor newAdapter = new NewPaymentAdapter();
                        ProcessUserPayment(manager, newAdapter);
                    }
                    break;
                case "3":
                    manager.ShowAllPayments();
                    break;
                case "4":
                    manager.ClearAllPayments();
                    break;
                case "5":
                    Console.WriteLine("Программа успешно завершена!");
                    return;
                default:
                    Console.WriteLine("Ошибка ввода. Команда не распознана.\n");
                    break;
            }
        }
    }

    private static void DemonstratePattern()
    {
        Console.WriteLine("Демонстрация работы паттерна «Адаптер»");

        IPaymentProcessor processor1 = new OldPaymentAdapter();
        IPaymentProcessor processor2 = new NewPaymentAdapter();

        decimal demoAmount = 1500m;
        string demoPurpose = "Демонстрационный платёж";

        Console.WriteLine($"--- Платёж #1 через {processor1.GetType().Name} ---");
        Console.WriteLine($"Тип переменной: {nameof(IPaymentProcessor)}, " +
                          $"фактический тип: {processor1.GetType().Name}");
        processor1.Pay(demoAmount, demoPurpose);

        Console.WriteLine($"[Результат] {processor1.GetPaymentInfo()}");
        Console.WriteLine();

        Console.WriteLine($"--- Платёж #12 через {processor2.GetType().Name} ---");
        Console.WriteLine($"Тип переменной: {nameof(IPaymentProcessor)}, " +
                          $"фактический тип: {processor2.GetType().Name}");
        processor2.Pay(demoAmount, demoPurpose);

        Console.WriteLine($"[Результат] {processor2.GetPaymentInfo()}");
        Console.WriteLine();
    }

    /// Общий сценарий оплаты: ввод суммы и назначения
    private static void ProcessUserPayment(PaymentManager manager, IPaymentProcessor processor)
    {
        decimal amount = ReadAmount();
        if (amount <= 0)
        {
            Console.WriteLine("Платёж отменён.\n");
            return;
        }

        string? purpose = ReadPurpose();
        if (purpose is null)
        {
            Console.WriteLine("Платёж отменён.\n");
            return;
        }

        manager.ProcessPayment(processor, amount, purpose);
    }

    /// Читает назначение платежа. Возвращает null, если пользователь отменил ввод (пустая строка).
    private static string? ReadPurpose()
    {
        while (true)
        {
            Console.Write("Введите назначение платежа (пустая строка — отмена): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input.Length == 0)
            {
                return null;
            }

            if (input.Length > 200)
            {
                Console.WriteLine("Ошибка: назначение слишком длинное (максимум 200 символов).");
                continue;
            }

            return input;
        }
    }

    /// Читает сумму платежа. Возвращает 0, если пользователь отменил ввод (пустая строка).
    private static decimal ReadAmount()
    {
        while (true)
        {
            Console.Write("Введите сумму платежа (пустая строка — отмена): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input.Length == 0)
            {
                return 0;
            }

            input = input.Replace('.', ',');

            if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal amount))
            {
                Console.WriteLine("Ошибка: введено не число. Попробуйте снова.");
                continue;
            }

            if (amount <= 0)
            {
                Console.WriteLine("Ошибка: сумма должна быть больше нуля.");
                continue;
            }

            if (amount > 1_000_000_000m)
            {
                Console.WriteLine("Ошибка: сумма слишком велика (максимум 1 000 000 000).");
                continue;
            }

            return amount;
        }
    }
}

// Классы
class Print
{
    public static void Introduction()
    {
        Console.WriteLine(
            "Работу выполнили: Бедарева Софья и Сторожева Анна\n" +
            "Группа: 9ИС-435\n" +
            "Практическая работа №3: Реализация структурного паттерна «Адаптер» в C#\n" +
            "Демонстрация работы системы интеграции платёжных систем\n\n"
        );
    }

    public static void Menu()
    {
        Console.WriteLine(
            "Выберите действие\n" +
            "1 - Оплатить через старую систему\n" +
            "2 - Оплатить через новую систему\n" +
            "3 - Показать все платежи\n" +
            "4 - Очистить список платежей\n" +
            "5 - Выход из программы\n"
        );
    }
}

public class PaymentDto
{
    [JsonPropertyName("amount")]
    [XmlElement("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("purpose")]
    [XmlElement("purpose")]
    public string? Purpose { get; set; }

    [JsonPropertyName("currency")]
    [XmlElement("currency")]
    public string? Currency { get; set; }
}

class PaymentManager
{
    private readonly List<string> _payments = new();

    public void ProcessPayment(IPaymentProcessor processor, decimal amount, string purpose)
    {
        processor.Pay(amount, purpose);

        string info = processor.GetPaymentInfo();
        _payments.Add(info);

        Console.WriteLine($"Успешно! Всего платежей: {_payments.Count}\n");
    }

    public void ShowAllPayments()
    {
        if (_payments.Count == 0)
        {
            Console.WriteLine("Список платежей пуст.");
        }
        else
        {
            Console.WriteLine("Список платежей:");
            for (int i = 0; i < _payments.Count; i++)
            {
                Console.WriteLine(_payments[i]);
                if (i < _payments.Count - 1)
                {
                    Console.WriteLine("-------------------------------------------------------------");
                }
            }
        }
        Console.WriteLine();
    }

    public void ClearAllPayments()
    {
        _payments.Clear();
        Console.WriteLine("Список платежей успешно очищен.\n");
    }
}

public static class Validate
{
    public static bool Payment(PaymentDto payment, out string error)
    {
        if (payment.Amount <= 0)
        {
            error = $"Сумма должна быть больше нуля (получено: {payment.Amount})";
            return false;
        }
        if (payment.Amount > 1_000_000_000m)
        {
            error = $"Сумма слишком велика (максимум 1 000 000 000, получено: {payment.Amount})";
            return false;
        }
        if (string.IsNullOrWhiteSpace(payment.Purpose))
        {
            error = "поле 'purpose' (назначение платежа) обязательно.";
            return false;
        }
        if (payment.Purpose.Length > 200)
        {
            error = "поле 'purpose' слишком длинное (максимум 200 символов).";
            return false;
        }
        if (string.IsNullOrWhiteSpace(payment.Currency))
        {
            error = "поле 'currency' обязательно.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}

// Интерфейс
interface IPaymentProcessor
{
    void Pay(decimal amount, string purpose);
    string GetPaymentInfo();
}

// Классы существующих платёжных систем
class OldPaymentSystem
{
    private static readonly XmlSerializer Serializer = new(typeof(PaymentDto));

    public string? MakePaymentXml(string xmlData)
    {
        Console.WriteLine($"[OldPaymentSystem] Обработка XML: {xmlData}");

        PaymentDto? payment;
        try
        {
            payment = (PaymentDto?)Serializer.Deserialize(new StringReader(xmlData));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[OldPaymentSystem] Ошибка синтаксиса XML: {ex.Message}");
            return null;
        }

        if (payment is null)
        {
            Console.WriteLine("[OldPaymentSystem] XML пустой или null");
            return null;
        }

        if (!Validate.Payment(payment, out string errorMessage))
        {
            Console.WriteLine($"[OldPaymentSystem] Ошибка валидации: {errorMessage}");
            return null;
        }

        string transactionId = Guid.NewGuid().ToString("N");

        Console.WriteLine("[OldPaymentSystem] Платёж успешно обработан:");
        Console.WriteLine($"   - Сумма:         {payment.Amount} {payment.Currency}");
        Console.WriteLine($"   - Назначение:    {payment.Purpose}");
        Console.WriteLine($"   - ID транзакции: {transactionId}");

        return transactionId;
    }
}

public class NewPaymentSystem
{
    public string? ProcessPaymentJson(string jsonData)
    {
        Console.WriteLine($"[NewPaymentSystem] Получены JSON-данные: {jsonData}");

        PaymentDto? payment;
        try
        {
            payment = JsonSerializer.Deserialize<PaymentDto>(jsonData);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[NewPaymentSystem] Ошибка синтаксиса JSON: {ex.Message}");
            return null;
        }

        if (payment is null)
        {
            Console.WriteLine("[NewPaymentSystem] JSON пустой или null");
            return null;
        }

        if (!Validate.Payment(payment, out string errorMessage))
        {
            Console.WriteLine($"[NewPaymentSystem] Ошибка валидации: {errorMessage}");
            return null;
        }

        string transactionId = Guid.NewGuid().ToString("N");

        Console.WriteLine("[NewPaymentSystem] Платёж успешно обработан:");
        Console.WriteLine($"   - Сумма:         {payment.Amount} {payment.Currency}");
        Console.WriteLine($"   - Назначение:    {payment.Purpose}");
        Console.WriteLine($"   - ID транзакции: {transactionId}");

        return transactionId;
    }
}

// Классы-адаптеры
class OldPaymentAdapter : IPaymentProcessor
{
    private static readonly XmlSerializer Serializer = new(typeof(PaymentDto));
    private readonly OldPaymentSystem _oldSystem = new();

    private decimal _amount;
    private string _purpose = "";
    private string? _transactionId;

    public void Pay(decimal amount, string purpose)
    {
        _amount = amount;
        _purpose = purpose;
        _transactionId = null;

        var paymentData = new PaymentDto
        {
            Amount = amount,
            Currency = "RUB",
            Purpose = purpose
        };

        using var stringWriter = new StringWriter();
        Serializer.Serialize(stringWriter, paymentData);
        string xmlData = stringWriter.ToString();

        _transactionId = _oldSystem.MakePaymentXml(xmlData);
    }

    public string GetPaymentInfo()
    {
        if (_transactionId is null)
            return $"Платёж на сумму {_amount} руб. ({_purpose}) НЕ выполнен (отклонён старой системой).";

        return $"Платёж на сумму {_amount} руб. ({_purpose}) выполнен через старую систему (XML). ID: {_transactionId}";
    }
}

class NewPaymentAdapter : IPaymentProcessor
{
    private readonly NewPaymentSystem _newSystem = new();

    private decimal _amount;
    private string _purpose = "";
    private string? _transactionId;

    public void Pay(decimal amount, string purpose)
    {
        _amount = amount;
        _purpose = purpose;
        _transactionId = null;

        var paymentData = new PaymentDto
        {
            Amount = amount,
            Currency = "RUB",
            Purpose = purpose
        };

        string jsonData = JsonSerializer.Serialize(paymentData);
        _transactionId = _newSystem.ProcessPaymentJson(jsonData);
    }

    public string GetPaymentInfo()
    {
        if (_transactionId is null)
            return $"Платёж на сумму {_amount} руб. ({_purpose}) НЕ выполнен (отклонён новой системой).";

        return $"Платёж на сумму {_amount} руб. ({_purpose}) выполнен через новую систему (JSON). ID: {_transactionId}";
    }
}