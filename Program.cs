namespace PaymentSystemIntegrationSystem;

class Program
{
    public static void Main(string[] args)
    {
        Print.Introduction();
        while (true)
        {
            Print.Menu();
            string input = Console.ReadLine()?.Trim() ?? "";

            switch (input)
            {
                case "1":
                break;
                case "2":
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

    class PaymentManager
    {
        List<string> payment = new();
        void ProcessPayment(IPaymentProcessor processor, decimal amount) { } // выполняет платёж через переданный процессор и сохраняет информацию о нём в список
        void ShowAllPayments() { }// — выводит информацию обо всех выполненных платежах
        void ClearAllPayments() { } //очищает список платежей

    }

    // Интрефейс
    interface IPaymentProcessor
    {
        void Pay(decimal amount); //выполнить платёж
        string GetPaymentInfo(); //получить информацию о платеже
    }

    // Классы существующих платёжных систем
    // TODO понять как реализовать классы платежных систем
    class OldPaymentSystem
    {
        void MakePaymentXml(string xmlData) { }
    }

    class NewPaymentSystem
    {
        void ProcessPaymentJson(string jsonData) { }
    }

    class OldPaymentAdapter : IPaymentProcessor
    {
        public string GetPaymentInfo()
        {
            throw new NotImplementedException();
        }

        public void Pay(decimal amount)
        {
            throw new NotImplementedException();
        }
    }

    // Классы-адаптеры
    class NewPaymentAdapter : IPaymentProcessor
    {
        public string GetPaymentInfo()
        {
            throw new NotImplementedException();
        }

        public void Pay(decimal amount)
        {
            throw new NotImplementedException();
        }
    }