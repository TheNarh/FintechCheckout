# FintechCheckout

A production-oriented **.NET payment checkout microservice** that integrates with **Paystack** to initialize, verify, and securely process payment transactions.

The project demonstrates how a backend service can manage its own transaction state while communicating with an external payment gateway.

---

## Overview

FintechCheckout provides a backend checkout API that:

* Accepts customer payment requests
* Validates checkout information
* Creates a local transaction
* Initializes the transaction with Paystack
* Returns a Paystack checkout URL
* Verifies completed payments
* Processes Paystack webhooks
* Validates webhook signatures
* Validates payment amount and currency
* Prevents duplicate transaction processing
* Stores transaction and payment audit information in SQLite

The application is designed around a simple principle:

> **The application owns the transaction state; Paystack acts as the payment gateway.**

---

## Architecture

```text
                    ┌─────────────────┐
                    │   React Client  │
                    │   / API Client  │
                    └────────┬────────┘
                             │
                             ▼
                  ┌──────────────────────┐
                  │ CheckoutController   │
                  │                      │
                  │ Validate request     │
                  │ Create transaction   │
                  │ Verify payment       │
                  └──────────┬───────────┘
                             │
                             ▼
                  ┌──────────────────────┐
                  │   PaystackService    │
                  │                      │
                  │ Initialize payment  │
                  │ Verify payment       │
                  └──────────┬───────────┘
                             │
                             ▼
                  ┌──────────────────────┐
                  │      Paystack       │
                  │   Payment Gateway   │
                  └──────────────────────┘


        Paystack Webhook
               │
               ▼
   ┌──────────────────────────┐
   │ PaystackWebhookController │
   │                          │
   │ Verify signature         │
   │ Validate amount          │
   │ Validate currency        │
   │ Process payment status   │
   └─────────────┬────────────┘
                 │
                 ▼
        ┌─────────────────┐
        │   SQLite DB     │
        │                 │
        │ Transactions    │
        └─────────────────┘
```

---

## Payment Flow

### 1. Create Checkout

The client sends:

```http
POST /api/checkout
Content-Type: application/json
```

Example:

```json
{
  "email": "customer@example.com",
  "amount": 150
}
```

The application:

1. Validates the request.
2. Generates a unique transaction reference.
3. Creates a local transaction with `Pending` status.
4. Sends the transaction to Paystack.
5. Returns the Paystack authorization URL.

---

### 2. Customer Pays

The customer is redirected to the Paystack checkout page.

Paystack handles the payment interaction with the customer.

---

### 3. Payment Verification

The application can verify the payment through:

```http
GET /api/checkout/{reference}/verify
```

The service asks Paystack for the current transaction status.

If successful, the local transaction becomes:

```text
Pending → Success
```

If the payment fails:

```text
Pending → Failed
```

---

### 4. Webhook Processing

Paystack can notify the application when a payment event occurs.

Webhook endpoint:

```http
POST /api/webhooks/paystack
```

The webhook handler:

1. Reads the Paystack signature.
2. Calculates an HMAC-SHA512 signature using the Paystack secret key.
3. Compares the signatures using a constant-time comparison.
4. Validates the transaction reference.
5. Validates the payment amount.
6. Validates the currency.
7. Processes the payment status.
8. Updates the local transaction.

---

## Security

The project implements several payment-security controls.

### Webhook Signature Verification

Incoming Paystack webhooks are verified using HMAC-SHA512.

```text
Paystack payload
       ↓
HMAC-SHA512 + secret key
       ↓
Generated signature
       ↓
Compare with x-paystack-signature
       ↓
Accept / Reject
```

Invalid or missing signatures are rejected.

### Constant-Time Comparison

Webhook signatures are compared using:

```csharp
CryptographicOperations.FixedTimeEquals(...)
```

This avoids using a normal string comparison for security-sensitive signature validation.

### Amount Validation

The amount received from Paystack is compared against the amount stored in the local transaction.

### Currency Validation

The webhook currency must match the currency stored for the transaction.

### Unique Transaction References

Transaction references are protected by a unique database index.

Example:

```text
TXN-61e62c86309d44cfafd9d4844fb4aad6
```

---

## Idempotency

Payment webhooks can potentially be delivered more than once.

The application therefore checks whether a transaction has already been successfully processed.

For example:

```text
First webhook:
Pending → Success

Duplicate webhook:
Success → ignored
```

This prevents the same payment event from being processed repeatedly.

---

## Database

The application uses:

* SQLite
* Entity Framework Core
* EF Core migrations

### Transaction

| Field           | Description                  |
| --------------- | ---------------------------- |
| Id              | Local transaction identifier |
| Reference       | Unique transaction reference |
| Email           | Customer email               |
| Amount          | Transaction amount           |
| Currency        | Transaction currency         |
| Status          | Pending, Success, or Failed  |
| CreatedAt       | Transaction creation time    |
| PaidAt          | Payment completion time      |
| GatewayResponse | Payment gateway response     |
| UpdatedAt       | Last transaction update time |

Transaction status is represented in C# using an enum:

```csharp
public enum TransactionStatus
{
    Pending,
    Success,
    Failed
}
```

The enum is stored as text in the database.

---

## API Endpoints

### Create Checkout

```http
POST /api/checkout
```

Creates a transaction and initializes it with Paystack.

### Verify Payment

```http
GET /api/checkout/{reference}/verify
```

Verifies a transaction with Paystack and updates the local transaction state.

### Paystack Webhook

```http
POST /api/webhooks/paystack
```

Receives and securely processes Paystack payment events.

---

## Example Response

A successful checkout initialization returns information similar to:

```json
{
  "id": 5,
  "reference": "TXN-...",
  "amount": 150,
  "currency": "GHS",
  "status": "Pending",
  "authorizationUrl": "https://checkout.paystack.com/...",
  "accessCode": "...",
  "paystackReference": "TXN-..."
}
```

After successful payment verification:

```json
{
  "id": 5,
  "reference": "TXN-...",
  "amount": 150,
  "currency": "GHS",
  "status": "Success",
  "paystackStatus": "success",
  "gatewayResponse": "Approved"
}
```

---

## Technology Stack

### Backend

* C#
* .NET 10
* ASP.NET Core Web API
* Entity Framework Core
* SQLite

### Payment

* Paystack API
* Paystack Webhooks
* HMAC-SHA512

### Development

* Swagger / OpenAPI
* Entity Framework Core Migrations
* Git / GitHub
* .NET User Secrets

---

## Project Structure

```text
FintechCheckout/
│
├── Configuration/
│   └── PaystackSettings.cs
│
├── Controllers/
│   ├── CheckoutController.cs
│   └── PaystackWebhookController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Migrations/
│
├── Models/
│   ├── CheckoutRequest.cs
│   ├── PaystackInitializeResponse.cs
│   ├── PaystackVerifyResponse.cs
│   ├── PaystackWebhookRequest.cs
│   ├── Transaction.cs
│   └── TransactionStatus.cs
│
├── Services/
│   └── PaystackServices.cs
│
├── Program.cs
├── FintechCheckout.csproj
├── FintechCheckout.http
└── README.md
```

---

## Getting Started

### Prerequisites

Install:

* .NET 10 SDK
* Git
* A Paystack test account

---

### Clone the Repository

```bash
git clone <your-repository-url>
cd FintechCheckout
```

---

### Configure Paystack

The Paystack secret key is stored using .NET User Secrets rather than being committed to source control.

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Set the test secret key:

```bash
dotnet user-secrets set "Paystack:SecretKey" "YOUR_TEST_SECRET_KEY"
```

The application expects the Paystack configuration:

```json
{
  "Paystack": {
    "BaseUrl": "https://api.paystack.co"
  }
}
```

The secret key should **never be committed to Git**.

---

### Apply Database Migrations

```bash
dotnet ef database update
```

---

### Run the Application

```bash
dotnet run
```

The API can then be accessed through the configured HTTPS endpoint.

Swagger/OpenAPI is available in the development environment.

---

## Testing

The backend was tested against the Paystack test environment.

Test scenarios include:

* Valid checkout
* Invalid email
* Invalid amount
* Paystack transaction initialization
* Real Paystack test payment
* Payment verification
* Successful webhook processing
* Duplicate webhook processing
* Invalid webhook signature
* Missing webhook signature
* Unsupported webhook event
* Incorrect webhook amount
* Incorrect webhook currency
* Unique transaction references

A real test payment successfully completed the flow:

```text
Checkout Request
      ↓
Local Pending Transaction
      ↓
Paystack Initialization
      ↓
Paystack Test Checkout
      ↓
Successful Payment
      ↓
Payment Verification
      ↓
Local Success Transaction
```

---

## Design Decisions

### Why keep a local transaction record?

The application should maintain its own transaction state rather than relying entirely on the payment provider.

This allows the system to:

* Track transactions
* Query payment history
* Maintain audit information
* Associate payments with application data
* Handle webhook events
* Build reporting and dashboards

### Why use a service for Paystack?

`PaystackService` isolates communication with the external payment provider from the controller.

This keeps the controller focused on HTTP/API responsibilities while the service handles Paystack communication.

### Why use Entity Framework Core?

EF Core provides:

* Strongly typed database access
* Migrations
* LINQ queries
* Integration with ASP.NET Core dependency injection

---

## Future Improvements

Potential production enhancements include:

* React frontend
* PostgreSQL
* Redis-based idempotency
* Distributed event processing
* Structured logging
* Application metrics
* Retry policies for transient gateway failures
* Background payment reconciliation
* Authentication and authorization
* Automated integration tests
* CI/CD pipeline
* Docker containerization
* Production-grade secrets management
* Payment reconciliation dashboard

---

## Project Goal

This project was built as a practical demonstration of backend engineering concepts involved in financial technology systems, including:

* REST API development
* Payment gateway integration
* Transaction state management
* Database persistence
* Webhook security
* Idempotent processing
* Input validation
* Auditability
* External API communication
* Secure secret management

The next phase of the project is a **React-based frontend** that will consume this API and provide a complete checkout experience.
