using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace PaymentSystemIntegrationSystem;

class Program
{
    public static void Main(string[] args)
    {
        Print.Introduction();

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

                        decimal oldAmount = ReadAmount();
                        if (oldAmount <= 0)
                        {
                            Console.WriteLine("Платёж отменён.\n");
                            break;
                        }

                        string purpose = ReadPurpose();
                        if (purpose == null)
                        {
                            Console.WriteLine("Платёж отменён.\n");
                            break;
                        }

                        manager.ProcessPayment(oldAdapter, oldAmount, purpose);
                    }
                    break;
                case "2":
                    {
                        IPaymentProcessor newAdapter = new NewPaymentAdapter();

                        decimal newAmount = ReadAmount();
                        if (newAmount <= 0)
                        {
                            Console.WriteLine("Платёж отменён.\n");
                            break;
                        }

                        string purpose = ReadPurpose();
                        if (purpose == null)
                        {
                            Console.WriteLine("Платёж отменён.\n");
                            break;
                        }

                        manager.ProcessPayment(newAdapter, newAmount, purpose);
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
    private static string? ReadPurpose()
    {
        while (true)
        {
            Console.Write("Введите назначение платежа: ");
            string input = Console.ReadLine()?.Trim() ?? "";


            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Ошибка: назначение не может быть пустым.");
                continue;
            }

            if (input.Length > 200)
            {
                Console.WriteLine("Ошибка: назначение слишком длинное (максимум 200 символов).");
                continue;
            }

            return input;
        }
    }
    private static decimal ReadAmount()
    {
        while (true)
        {
            Console.Write("Введите сумму платежа: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            input = input.Replace('.', ',');

            if (!decimal.TryParse(input, out decimal amount))
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
    List<string> payment = new();
    public void ProcessPayment(IPaymentProcessor processor, decimal amount, string purpose)
    {
        processor.Pay(amount, purpose);

        string info = processor.GetPaymentInfo();
        payment.Add(info);

        Console.WriteLine($"Успешно! Всего платежей: {payment.Count}\n");
    }
    public void ShowAllPayments()
    {
        if (payment.Count == 0)
            Console.WriteLine("Список платежей пуст.");
        else
        {
            Console.WriteLine("Список платежей:");
            foreach (var item in payment)
            {
                Console.WriteLine(item);
                Console.WriteLine("-------------------------------------------------------------");
            }
        }
        Console.WriteLine();
    }
    public void ClearAllPayments()
    {
        payment.Clear();
        Console.WriteLine("Список платежей успешно очищен.");
    }

}

public class Validate
{
    static public bool Payment(PaymentDto payment, out string error)
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

// Интрефейс
interface IPaymentProcessor
{
    void Pay(decimal amount, string purpose);
    string GetPaymentInfo();
}

// Классы существующих платёжных систем
class OldPaymentSystem
{
    XmlSerializer serializer = new XmlSerializer(typeof(PaymentDto));
    public string? MakePaymentXml(string xmlData)
    {
        Console.WriteLine($"[OldPaymentSystem] Обработка XML: {xmlData}");

        PaymentDto? payment;
        try
        {
            payment = (PaymentDto?)serializer.Deserialize(new StringReader(xmlData));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"[OldPaymentSystem] Ошибка синтаксиса XML: {ex.Message}");
            return null;
        }
        if (payment is null)
        {
            Console.WriteLine($"[OldPaymentSystem] XML пустой или null");
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
    private readonly OldPaymentSystem _oldSystem = new();
    private decimal _amount;
    private string _purpose = "";
    private string? _transactionId;

    public void Pay(decimal amount, string purpose)
    {
        _amount = amount;
        _purpose = purpose;

        var paymentData = new PaymentDto
        {
            Amount = amount,
            Currency = "RUB",
            Purpose = purpose
        };

        var serializer = new XmlSerializer(typeof(PaymentDto));
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, paymentData);
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

        var paymentData = new
        {
            amount = amount,
            currency = "RUB",
            purpose = purpose
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
