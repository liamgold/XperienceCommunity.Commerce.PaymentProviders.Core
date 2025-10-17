# XperienceCommunity.Commerce.PaymentProviders.Core

[![NuGet](https://img.shields.io/nuget/v/XperienceCommunity.Commerce.PaymentProviders.Core.svg)](https://www.nuget.org/packages/XperienceCommunity.Commerce.PaymentProviders.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Core abstractions and contracts for building payment provider integrations in Xperience by Kentico commerce solutions.

## Overview

This package provides a clean, provider-agnostic interface for integrating third-party payment gateways (such as Stripe, PayPal, Square, etc.) with Kentico Xperience commerce implementations. It defines the contracts that payment providers must implement and the contracts that host applications must fulfill.

**This package contains only abstractions** - no concrete implementations or third-party SDK dependencies.

## Installation

```bash
dotnet add package XperienceCommunity.Commerce.PaymentProviders.Core
```

## Public API

### Core Interfaces

#### `IPaymentGateway`
The primary interface that payment provider implementations must implement.

```csharp
public interface IPaymentGateway
{
    /// <summary>
    /// Creates a new checkout session or reuses an existing one for the given order.
    /// </summary>
    Task<CreateSessionResult> CreateOrReuseSessionAsync(
        OrderSnapshot order,
        CancellationToken ct = default);

    /// <summary>
    /// Handles incoming webhook requests from the payment provider.
    /// </summary>
    Task<WebhookResult> HandleWebhookAsync(
        HttpRequest request,
        CancellationToken ct = default);
}
```

**Responsibilities:**
- Create payment checkout sessions
- Generate redirect URLs for customer payment
- Process webhook callbacks from payment providers
- Extract order identifiers from webhook payloads
- Validate webhook signatures (provider-specific)

#### `IOrderPayments`
The interface that host applications must implement to persist payment state changes.

```csharp
public interface IOrderPayments
{
    /// <summary>
    /// Updates the payment state for a given order.
    /// </summary>
    Task SetStateAsync(
        string orderNumber,
        PaymentState state,
        string? providerRef = null,
        CancellationToken ct = default);
}
```

**Responsibilities:**
- Receive payment state updates from webhooks
- Update order status in your system (Kentico `OrderInfo`)
- Trigger business workflows (send emails, fulfill orders, etc.)
- Map `PaymentState` to your order status model

### Data Contracts

#### `OrderSnapshot`
Immutable snapshot of order data required for payment session creation.

```csharp
public sealed record OrderSnapshot(
    string OrderNumber,           // Unique order identifier
    long AmountMinor,             // Amount in minor currency units (e.g., 1299 = £12.99)
    string Currency,              // ISO 4217 currency code (e.g., "GBP", "USD")
    string CustomerEmail,         // Customer email for receipts
    Uri SuccessUrl,              // Redirect URL after successful payment
    Uri CancelUrl);              // Redirect URL if payment cancelled
```

**Note on amounts:** All monetary values use **minor currency units** (also called "smallest currency unit" or "cents"):
- £12.99 → `1299`
- $100.00 → `10000`
- ¥500 → `500` (yen has no subunits)

This approach eliminates floating-point precision issues and matches most payment provider APIs.

#### `CreateSessionResult`
Result of creating a payment session.

```csharp
public sealed record CreateSessionResult(
    Uri RedirectUrl,    // URL to redirect customer to complete payment
    string ProviderRef  // Provider's unique session/payment ID
);
```

#### `WebhookResult`
Result of processing a webhook event.

```csharp
public sealed record WebhookResult(
    bool Handled,         // True if webhook was successfully processed
    string? OrderNumber   // Order number extracted from webhook, if any
);
```

#### `PaymentState`
Enumeration of possible payment states.

```csharp
public enum PaymentState
{
    Pending,              // Payment awaiting processing
    Processing,           // Payment currently being processed
    Succeeded,            // Payment successfully completed
    Failed,               // Payment failed
    Refunded,             // Payment fully refunded
    PartiallyRefunded     // Payment partially refunded
}
```

## Usage

### For Payment Provider Implementers

Implement `IPaymentGateway` to integrate a new payment provider:

```csharp
public class MyPaymentGateway : IPaymentGateway
{
    public async Task<CreateSessionResult> CreateOrReuseSessionAsync(
        OrderSnapshot order,
        CancellationToken ct = default)
    {
        // 1. Call your payment provider's API to create a checkout session
        // 2. Include order.OrderNumber in session metadata
        // 3. Return redirect URL and session ID
    }

    public async Task<WebhookResult> HandleWebhookAsync(
        HttpRequest request,
        CancellationToken ct = default)
    {
        // 1. Validate webhook signature
        // 2. Parse webhook payload
        // 3. Extract order number from metadata
        // 4. Return result
    }
}
```

**See also:** [XperienceCommunity.Commerce.PaymentProviders.Stripe](https://github.com/liamgold/XperienceCommunity.Commerce.PaymentProviders.Stripe) for a complete implementation example.

### For Host Application Developers

Implement `IOrderPayments` to handle payment state changes:

```csharp
public class OrderPaymentsService : IOrderPayments
{
    private readonly IInfoProvider<OrderInfo> orderInfoProvider;
    private readonly IInfoProvider<OrderStatusInfo> orderStatusInfoProvider;

    public async Task SetStateAsync(
        string orderNumber,
        PaymentState state,
        string? providerRef = null,
        CancellationToken ct = default)
    {
        // 1. Find order by orderNumber
        var order = await orderInfoProvider
            .Get()
            .WhereEquals(nameof(OrderInfo.OrderNumber), orderNumber)
            .TopN(1)
            .GetEnumerableTypedResultAsync();

        // 2. Map PaymentState to OrderStatusInfo
        var orderStatus = await orderStatusInfoProvider.GetAsync(
            MapPaymentStateToStatus(state), ct);

        // 3. Update order status
        order.OrderOrderStatusID = orderStatus.OrderStatusID;
        await orderInfoProvider.SetAsync(order, ct);

        // 4. Trigger notifications, workflows, etc.
    }
}
```

**See also:** DancingGoat sample implementation in the [Stripe package repository](https://github.com/liamgold/XperienceCommunity.Commerce.PaymentProviders.Stripe).

## Architecture

This package follows the **Dependency Inversion Principle**:

```
┌─────────────────────────────────────┐
│   Host Application (ASP.NET Core)  │
│                                     │
│  ┌───────────────────────────────┐ │
│  │   Implements IOrderPayments   │ │
│  └───────────────────────────────┘ │
└─────────────────────────────────────┘
                 ▲
                 │ depends on
                 │
┌─────────────────────────────────────┐
│      Core Abstractions Package      │
│   (this package - no dependencies)  │
│                                     │
│  • IPaymentGateway                  │
│  • IOrderPayments                   │
│  • Records & Enums                  │
└─────────────────────────────────────┘
                 ▲
                 │ depends on
                 │
┌─────────────────────────────────────┐
│   Payment Provider Implementation   │
│     (e.g., Stripe, PayPal)          │
│                                     │
│  ┌───────────────────────────────┐ │
│  │   Implements IPaymentGateway  │ │
│  └───────────────────────────────┘ │
└─────────────────────────────────────┘
```

Both the host application and provider implementations depend on the core abstractions, not on each other.

## Design Principles

1. **Provider-agnostic:** Works with any payment provider (Stripe, PayPal, Square, etc.)
2. **Immutable data:** Uses C# records for thread-safe, immutable data structures
3. **Minor currency units:** Eliminates floating-point precision issues
4. **Async-first:** All operations are asynchronous with cancellation token support
5. **Separation of concerns:** Payment providers handle external APIs; host apps handle order state
6. **No magic strings:** Strongly-typed enums and records
7. **No dependencies:** Zero external package dependencies

## Common Implementation Patterns

### Idempotency
Payment webhooks may be delivered multiple times. Implement idempotency in your `IOrderPayments` implementation:

```csharp
public async Task SetStateAsync(string orderNumber, PaymentState state, ...)
{
    // Check if this state transition has already been processed
    // (e.g., using a unique constraint on webhook event ID)
}
```

### Order Number Tracking
Payment providers should store `order.OrderNumber` in session metadata:

```csharp
// Stripe example
Metadata = new Dictionary<string, string>
{
    ["orderNumber"] = order.OrderNumber
}
```

### Webhook Security
Always validate webhook signatures in your `HandleWebhookAsync` implementation:

```csharp
public async Task<WebhookResult> HandleWebhookAsync(HttpRequest request, ...)
{
    // 1. Validate signature using provider's SDK
    // 2. Only process if signature is valid
    // 3. Return Handled=false for invalid signatures
}
```

## Compatibility

- **.NET:** 8.0+
- **Xperience by Kentico:** 29.0.0+
- **Dependencies:** `Microsoft.AspNetCore.Http.Abstractions` 2.3.0

## Contributing

This is a community package. Contributions are welcome!

## License

MIT License - see LICENSE file for details.

## Related Packages

- [XperienceCommunity.Commerce.PaymentProviders.Stripe](https://www.nuget.org/packages/XperienceCommunity.Commerce.PaymentProviders.Stripe) - Stripe implementation

## Resources

- [Xperience by Kentico Commerce Documentation](https://docs.kentico.com/documentation/developers-and-admins/digital-commerce-setup)
- [GitHub Repository](https://github.com/liamgold/XperienceCommunity.Commerce.PaymentProviders.Core)
- [Report Issues](https://github.com/liamgold/XperienceCommunity.Commerce.PaymentProviders.Core/issues)
