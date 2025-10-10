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
