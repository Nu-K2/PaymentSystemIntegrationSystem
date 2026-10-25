using System.Text.Json;
using System.Text.Json.Serialization;

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
                    break;
                case "2":
                    // 1. Создаём адаптер новой системы
                    IPaymentProcessor newAdapter = new NewPaymentAdapter();

                    // 2. Запрашиваем сумму (используем ваш безопасный ReadAmount)
                    decimal newAmount = ReadAmount();
                    if (newAmount <= 0)
                    {
                        Console.WriteLine("Платёж отменён.\n");
                        break;
                    }

                    // 3. Запрашиваем назначение платежа
                    string purpose = ReadPurpose();
                    if (purpose == null)
                    {
                        Console.WriteLine("Платёж отменён.\n");
                        break;
                    }

                    // 4. Отдаём в менеджер — он вызовет Pay у адаптера
                    manager.ProcessPayment(newAdapter, newAmount, purpose);
                    break;
                case "3":
                    break;
                case "4":
                    break;
                case "5":
                    Console.WriteLine("Программа успешно завершена!");
                    return;
                default:
                    Console.WriteLine("Ошибка ввода. Команда не распознана.");
                    break;
            }
        }
    }
    private static string? ReadPurpose()
    {
        while (true)
        {
            Console.Write("Введите назначение платежа (или '0' для отмены): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input == "0")
                return null;

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
            Console.Write("Введите сумму платежа (или '0' для отмены): ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (input == "0")
                return -1; // сигнал отмены

            input = input.Replace('.', ','); // удобство для пользователя

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
            "1 - Оплатить через старую систему\n" + //создаёт OldPaymentAdapter, запрашивает сумму и выполняет платёж.
            "2 - Оплатить через новую систему\n" + //создаёт NewPaymentAdapter, запрашивает сумму и выполняет платёж.
            "3 - Показать все платежи\n" + //вызывает метод ShowAllPayments().
            "4 - Очистить список платежей\n" + // вызывает метод ClearAllPayments().
            "5 - Выход из программы\n"
        );
    }

    public static void Message(string info)
    {
        Console.WriteLine(info);
    }
}
public class PaymentDto
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

class PaymentManager
{
    List<string> payment = new();
    public void ProcessPayment(IPaymentProcessor processor, decimal amount, string purpose)
    {
        // Тело вы доделаете сами — здесь просто вызов адаптера:
        processor.Pay(amount, purpose);

        string info = processor.GetPaymentInfo();
        payment.Add(info);

        Console.WriteLine($"Успешно! Всего платежей: {payment.Count}\n");
    }
    void ShowAllPayments() { }// — выводит информацию обо всех выполненных платежах
    void ClearAllPayments() { } //очищает список платежей

}

// Интрефейс
interface IPaymentProcessor
{
    void Pay(decimal amount, string purpose); //выполнить платёж
    string GetPaymentInfo(); //получить информацию о платеже
}

// Классы существующих платёжных систем
class OldPaymentSystem
{
    void MakePaymentXml(string xmlData)
    {
        Console.WriteLine($"[OldPaymentSystem] Обработка XML: {xmlData}");
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

        if (!ValidatePayment(payment, out string errorMessage))
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

    private bool ValidatePayment(PaymentDto payment, out string error)
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

// Классы-адаптеры
class OldPaymentAdapter : IPaymentProcessor
{
    public string GetPaymentInfo()
    {
        throw new NotImplementedException();
    }

    public void Pay(decimal amount, string purpose)
    {
        throw new NotImplementedException();
    }
}


class NewPaymentAdapter : IPaymentProcessor
{
    private readonly NewPaymentSystem _newSystem = new();
    private decimal _lastAmount;
    private string _lastPurpose = "";
    private string? _lastTransactionId;

    public void Pay(decimal amount, string purpose)
    {
        _lastAmount = amount;
        _lastPurpose = purpose;

        var paymentData = new
        {
            amount = amount,
            currency = "RUB",
            purpose = purpose
        };

        string jsonData = JsonSerializer.Serialize(paymentData);
        _lastTransactionId = _newSystem.ProcessPaymentJson(jsonData);
    }

    public string GetPaymentInfo()
    {
        return $"Платёж на сумму {_lastAmount} руб. ({_lastPurpose}) выполнен через новую систему (JSON). ID: {_lastTransactionId ?? "—"}";
    }
}
