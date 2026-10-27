**English** | [Русский](README.md)
# README

# Practical Work No. 3: Implementing the Structural "Adapter" Pattern in C#

## Objective of the Work

To consolidate theoretical knowledge on the topic "Structural Design Patterns" and to gain practical skills in implementing the **"Adapter"** pattern in a console application in C#.

## Project Description

A console application **"Payment System Integration System"** demonstrating the operation of the "Adapter" pattern for reconciling incompatible interfaces. The application allows working with two different payment systems — an **old one (XML)** and a **new one (JSON)** — through a single `IPaymentProcessor` interface.

---

## Architecture and Class Structure

### 1. `IPaymentProcessor` — Target Interface

A single interface that the client code works with:

```csharp
void Pay(decimal amount, string purpose);
string GetPaymentInfo();
```

### 2. Incompatible Payment Systems (Adaptees)

| Class | Data Format | Method |
|-------|---------------|-------|
| `OldPaymentSystem` | XML | `string? MakePaymentXml(string xmlData)` |
| `NewPaymentSystem` | JSON | `string? ProcessPaymentJson(string jsonData)` |

Each system:
- Deserializes the input data (XML/JSON) into a `PaymentDto`.
- Validates the payment via `Validate.Payment`.
- Generates a `transactionId` (GUID) on success.
- Returns `null` on error.

### 3. Adapter Classes

| Adapter | Adaptee System | Format |
|---------|---------------------|--------|
| `OldPaymentAdapter` | `OldPaymentSystem` | XML |
| `NewPaymentAdapter` | `NewPaymentSystem` | JSON |

Each adapter:
- Implements `IPaymentProcessor`.
- Stores the state of the last payment (`_amount`, `_purpose`, `_transactionId`).
- In the `Pay` method, serializes the `PaymentDto` into the required format and calls the corresponding system.
- In `GetPaymentInfo`, returns information about the payment result.

### 4. `PaymentDto` — Payment DTO

A common data class with serialization attributes:

```csharp
[JsonPropertyName("amount")]  [XmlElement("amount")]     public decimal Amount
[JsonPropertyName("purpose")] [XmlElement("purpose")]    public string? Purpose
[JsonPropertyName("currency")][XmlElement("currency")]   public string? Currency
```

### 5. `Validate` — Static Validation Class

Checks:
- `Amount > 0` and `Amount <= 1_000_000_000`.
- `Purpose` is not empty and is no longer than 200 characters.
- `Currency` is not empty.

### 6. `PaymentManager` — Payment Manager

- Stores the list of completed payments (`List<string>`).
- `ProcessPayment(IPaymentProcessor processor, decimal amount, string purpose)` — performs the payment and saves the information.
- `ShowAllPayments()` — displays all payments with separators.
- `ClearAllPayments()` — clears the list.

### 7. `Print` — Auxiliary Output Class

- `Introduction()` — main screen.
- `Menu()` — interactive menu.

### 8. `Program` — Entry Point

- Displays information about the authors and the work.
- Runs the **pattern demonstration** (`DemonstratePattern`).
- Implements an **interactive menu** of 5 items.
- Auxiliary methods `ReadAmount()` and `ReadPurpose()` with input validation and the option to cancel (empty string).

---

## Functional Capabilities

### Main Screen (Entry Point)

On startup, the following is displayed:
- Last name and first name of the students.
- Group name (9IS-435).
- Name of the practical work (PW No. 3: "Adapter" Pattern).
- Brief description of the work.

### Interactive Menu

| No. | Menu Item | Action |
|---|-----------|----------|
| **1** | Pay via the old system | Creates an `OldPaymentAdapter`, prompts for the amount and purpose, performs the payment |
| **2** | Pay via the new system | Creates a `NewPaymentAdapter`, prompts for the amount and purpose, performs the payment |
| **3** | Show all payments | Calls `ShowAllPayments()` |
| **4** | Clear the payment list | Calls `ClearAllPayments()` |
| **5** | Exit | Terminates the application |

### Action Confirmation

After each payment, a message with the current count is displayed:

```
Success! Total payments: 3
```

### Demonstration of the Pattern's Operation

In the `Main` method, a separate `DemonstratePattern()` block:
- Creates instances of `OldPaymentAdapter` and `NewPaymentAdapter`.
- Through variables of type `IPaymentProcessor`, performs one payment with each adapter.
- Shows the variable type (`IPaymentProcessor`) and the actual type (`OldPaymentAdapter` / `NewPaymentAdapter`).
- Demonstrates that the client code works identically with different systems.

---

## Example of the Program's Operation

```
Practical Work No. 3: Implementing the Structural "Adapter" Pattern in C#
Demonstration of the payment system integration system's operation

Demonstration of the "Adapter" pattern's operation
--- Payment #1 via OldPaymentAdapter ---
Variable type: IPaymentProcessor, actual type: OldPaymentAdapter
[OldPaymentSystem] Processing XML: <?xml version="1.0" encoding="utf-8"?>...
[OldPaymentSystem] Payment successfully processed:
   - Amount:        1500 RUB
   - Purpose:       Demonstration payment
   - Transaction ID: a1b2c3d4e5f6...
[Result] Payment of 1500 RUB (Demonstration payment) completed via the old system (XML). ID: a1b2c3d4e5f6...

--- Payment #2 via NewPaymentAdapter ---
Variable type: IPaymentProcessor, actual type: NewPaymentAdapter
[NewPaymentSystem] JSON data received: {"amount":1500,"purpose":"Demonstration payment","currency":"RUB"}
[NewPaymentSystem] Payment successfully processed:
   - Amount:        1500 RUB
   - Purpose:       Demonstration payment
   - Transaction ID: f6e5d4c3b2a1...
[Result] Payment of 1500 RUB (Demonstration payment) completed via the new system (JSON). ID: f6e5d4c3b2a1...

Choose an action
1 - Pay via the old system
2 - Pay via the new system
3 - Show all payments
4 - Clear the payment list
5 - Exit the program
```

---

## Compliance with the Assignment Requirements

| No. | Requirement | Status | Implementation |
|---|------------|--------|------------|
| **1** | "Adapter" pattern | ✅ | `IPaymentProcessor` interface, two incompatible systems (`OldPaymentSystem`, `NewPaymentSystem`), two adapters |
| **2** | Business logic (`PaymentManager`) | ✅ | List storage, `ProcessPayment`, `ShowAllPayments`, `ClearAllPayments` |
| **3** | Handling of all menu items | ✅ | 5 items, each uses adapters |
| **4** | Action confirmation | ✅ | Message with the current number of payments after the operation |
| **5** | Pattern demonstration | ✅ | `DemonstratePattern()` block in `Main` showing types and results |

---

## Additional Implementation Features

- **Input validation:** checking the amount (`decimal.TryParse`, `> 0`, `<= 1_000_000_000`) and the purpose (not empty, `<= 200` characters).
- **Input cancellation:** an empty string cancels the payment.
- **Replacing a dot with a comma:** support for decimal separators (`input.Replace('.', ',')`).
- **Exception handling:** `try/catch` for `InvalidOperationException` (XML) and `JsonException` (JSON).
- **Single DTO with two sets of attributes:** `PaymentDto` is used for both JSON and XML.
- **`Validate` class:** centralized payment validation for both systems.
- **`transactionId` generation:** a unique identifier via `Guid.NewGuid().ToString("N")`.
- **Separators in output:** `ShowAllPayments` separates payments with a line.

---

## How to Run

1. Open the project in Visual Studio / Rider / VS Code.
2. Make sure the .NET SDK is installed.
3. Build and run the project:
   ```bash
   dotnet run
   ```
4. Follow the instructions in the console menu.

---

## Technologies and Principles Used

- **Language:** C#
- **Platform:** .NET (Console Application)
- **Pattern:** Adapter (structural)
- **Serialization:** `System.Text.Json` (JSON), `System.Xml.Serialization` (XML)
- **OOP principles:** abstraction, encapsulation, polymorphism
- **SOLID principles:**
  - *Single Responsibility* — each class is responsible for its own task.
  - *Open/Closed* — a new payment system is added via a new adapter without changing the client code.
  - *Liskov Substitution* — both adapters can be correctly substituted for `IPaymentProcessor`.
  - *Dependency Inversion* — the client code (`PaymentManager`) depends on the interface, not on concrete systems.
