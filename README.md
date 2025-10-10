# XperienceCommunity.Commerce.PaymentProviders.Core
Abstractions for payment providers in Xperience by Kentico.
Public API:
- IPaymentGateway (create checkout session, handle webhook)
- IOrderPayments (persist order state)
- PaymentState enum
- OrderSnapshot, CreateSessionResult, WebhookResult records
Notes: Amounts use minor currency units. No provider SDKs here.
