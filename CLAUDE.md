# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library package that provides **core abstractions only** for payment provider integrations in Xperience by Kentico commerce solutions. It contains no concrete implementations, no third-party SDK dependencies, and is designed following the Dependency Inversion Principle.

**Key concept:** Both host applications (Xperience implementations) and payment provider implementations (Stripe, PayPal, etc.) depend on these abstractions, but not on each other.

## Build Commands

```bash
# Build entire solution
dotnet build

# Build in Release mode (generates NuGet package)
dotnet build -c Release

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Clean build artifacts
dotnet clean
```

## Project Structure

- **src/XperienceCommunity.Commerce.PaymentProviders.Core/** - Main library project containing all abstractions
  - **Abstractions/Contracts.cs** - Single file containing all interfaces, records, and enums
- **tests/Core.UnitTests/** - xUnit test project
- **Directory.Build.props** - Centralized build configuration (targets .NET 8.0, nullable reference types enabled)
- **Directory.Packages.props** - Centralized package version management (CPM enabled)

## Architecture

### Core Interfaces

The entire public API is defined in a single file: `src/XperienceCommunity.Commerce.PaymentProviders.Core/Abstractions/Contracts.cs`

1. **IPaymentGateway** - Implemented by payment provider packages (Stripe, PayPal, etc.)
   - `CreateOrReuseSessionAsync()` - Creates payment sessions with provider APIs
   - `HandleWebhookAsync()` - Processes webhook callbacks from providers

2. **IOrderPayments** - Implemented by host applications (Xperience sites)
   - `SetStateAsync()` - Updates order status when payment state changes

### Data Contracts

- **OrderSnapshot** - Immutable order details for payment session creation
  - **CRITICAL:** `AmountMinor` uses minor currency units (£12.99 = 1299 cents)
- **CreateSessionResult** - Payment session redirect URL and provider reference
- **WebhookResult** - Webhook processing outcome with order number
- **PaymentState** enum - Payment lifecycle states (Pending, Processing, Succeeded, Failed, Refunded, PartiallyRefunded)

## Design Constraints

When modifying this codebase, maintain these principles:

1. **Zero dependencies** - Only Microsoft.AspNetCore.Http.Abstractions is allowed (for HttpRequest type)
2. **Abstractions only** - No concrete implementations or business logic
3. **Immutable data** - Use C# records with init-only properties
4. **Minor currency units** - All monetary amounts must be `long` representing smallest currency unit
5. **Async-first** - All operations async with CancellationToken support
6. **XML documentation** - All public APIs must have XML docs (enforced by build)
7. **Nullable enabled** - Nullable reference types with warnings as errors

## Testing

Tests use xUnit. The test project includes:
- **ContractSmokeTests.cs** - Basic construction and usage validation

When adding new contracts, add corresponding smoke tests to verify basic construction and usage.

## Package Configuration

This project is configured to:
- Generate NuGet package on build (`GeneratePackageOnBuild=true`)
- Include README.md in package
- Target .NET 8.0 only
- Use Central Package Management (versions in Directory.Packages.props)

Version is defined in the .csproj file: `src/XperienceCommunity.Commerce.PaymentProviders.Core/XperienceCommunity.Commerce.PaymentProviders.Core.csproj`

## Related Resources

- **Example implementation:** XperienceCommunity.Commerce.PaymentProviders.Stripe (https://github.com/liamgold/XperienceCommunity.Commerce.PaymentProviders.Stripe)
- **Host application example:** DancingGoat sample in Stripe package repository
- **Xperience documentation:** https://docs.kentico.com/documentation/developers-and-admins/digital-commerce-setup
