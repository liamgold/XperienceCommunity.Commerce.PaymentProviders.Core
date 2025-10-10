# Core Specification

## Project
- Name: XperienceCommunity.Commerce.PaymentProviders.Core
- Branch: feat/core-contracts
- Target Framework: .NET 8.0
- Nullable: enabled
- XML Docs: enabled
- External deps: Microsoft.AspNetCore.Http.Abstractions only

---

## 1) Solution Structure (create exactly)
- /src/XperienceCommunity.Commerce.PaymentProviders.Core
- /src/XperienceCommunity.Commerce.PaymentProviders.Core/Abstractions/Contracts.cs
- /tests/Core.UnitTests
- /.github/workflows/build.yml
- /.github/dependabot.yml
- /README.md
- /CHANGELOG.md
- /LICENSE

---

## 2) Public Contracts (put this exact code in Abstractions/Contracts.cs)
    using Microsoft.AspNetCore.Http;

    namespace XperienceCommunity.Commerce.PaymentProviders.Core
    {
        public interface IPaymentGateway
        {
            Task<CreateSessionResult> CreateOrReuseSessionAsync(
                OrderSnapshot order,
                CancellationToken ct = default);

            Task<WebhookResult> HandleWebhookAsync(
                HttpRequest request,
                CancellationToken ct = default);
        }

        public interface IOrderPayments
        {
            Task SetStateAsync(
                string orderNumber,
                PaymentState state,
                string? providerRef = null,
                CancellationToken ct = default);
        }

        public enum PaymentState
        {
            Pending,
            Processing,
            Succeeded,
            Failed,
            Refunded,
            PartiallyRefunded
        }

        public sealed record OrderSnapshot(
            string OrderNumber,
            long AmountMinor,
            string Currency,
            string CustomerEmail,
            Uri SuccessUrl,
            Uri CancelUrl);

        public sealed record CreateSessionResult(
            Uri RedirectUrl,
            string ProviderRef);

        public sealed record WebhookResult(
            bool Handled,
            string? OrderNumber);
    }

Notes:
- AmountMinor is in minor currency units (e.g., 1299 = £12.99).
- Core is abstractions only; no business logic or provider SDKs.

---

## 3) Unit Test (create tests/Core.UnitTests/ContractSmokeTests.cs)
    using System;
    using XperienceCommunity.Commerce.PaymentProviders.Core;
    using Xunit;

    public class ContractSmokeTests
    {
        [Fact]
        public void Should_Construct_Types()
        {
            var order = new OrderSnapshot(
                "ORD-001", 1299, "GBP", "test@example.com",
                new Uri("https://site/success"),
                new Uri("https://site/cancel"));

            var session = new CreateSessionResult(
                new Uri("https://stripe/session"), "cs_123");

            var webhook = new WebhookResult(true, "ORD-001");

            Assert.Equal("ORD-001", order.OrderNumber);
            Assert.Equal("cs_123", session.ProviderRef);
            Assert.True(webhook.Handled);
        }
    }

---

## 4) CI (create .github/workflows/build.yml)
    name: build
    on:
      push:
      pull_request:
    jobs:
      build:
        runs-on: ${{ matrix.os }}
        strategy:
          matrix:
            os: [ubuntu-latest, windows-latest]
        steps:
          - uses: actions/checkout@v4
          - uses: actions/setup-dotnet@v4
            with:
              dotnet-version: '8.0.x'
          - run: dotnet restore
          - run: dotnet build
          - run: dotnet test

---

## 5) Dependabot (create .github/dependabot.yml)
    version: 2
    updates:
      - package-ecosystem: "nuget"
        directory: "/"
        schedule: { interval: "weekly" }
      - package-ecosystem: "github-actions"
        directory: "/"
        schedule: { interval: "weekly" }

---

## 6) README.md (short)
    # XperienceCommunity.Commerce.PaymentProviders.Core
    Abstractions for payment providers in Xperience by Kentico.
    Public API:
    - IPaymentGateway (create checkout session, handle webhook)
    - IOrderPayments (persist order state)
    - PaymentState enum
    - OrderSnapshot, CreateSessionResult, WebhookResult records
    Notes: Amounts use minor currency units. No provider SDKs here.

---

## 7) CHANGELOG.md
    ## 1.0.0
    - Initial slim abstractions and smoke test.

---

## 8) LICENSE
    MIT

---

## 9) XML Documentation

- Add XML documentation comments to **all public** interfaces, enums, records, and their members in this project.
- Each public member must have at least a concise `<summary>` (1–2 lines).
- Keep the tone factual and neutral.

---

## 10) Actions (execute in order)
1. Create branch `feat/core-contracts`.
2. Scaffold solution + projects per structure above.
3. Add Contracts.cs with the exact code.
4. Add smoke test file.
5. Add CI and Dependabot files.
6. Add README, CHANGELOG, LICENSE.
7. Run: dotnet restore; dotnet build; dotnet test.
8. Add any missing XML documentation.
9. Commit all; open PR to main; return PR URL.