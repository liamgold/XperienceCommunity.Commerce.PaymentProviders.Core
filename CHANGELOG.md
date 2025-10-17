# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0-beta] - 2025-10-17

### Added
- Initial pre-release of Core abstractions package
- `IPaymentGateway` interface for payment provider implementations
- `IOrderPayments` interface for host application order state management
- `OrderSnapshot` record for payment session creation
- `CreateSessionResult` record for session creation results
- `WebhookResult` record for webhook processing results
- `PaymentState` enum with support for Pending, Processing, Succeeded, Failed, Refunded, and PartiallyRefunded states
- Comprehensive README with usage examples and architecture documentation
- XML documentation for all public APIs
- MIT License

### Notes
- This is a pre-release version intended for early adopters and feedback
- All monetary amounts use minor currency units (cents/pence)
- Package contains only abstractions with zero implementation dependencies
- Compatible with .NET 8.0+ and Xperience by Kentico 29.0.0+
