using Microsoft.AspNetCore.Http;

namespace XperienceCommunity.Commerce.PaymentProviders.Core
{
    /// <summary>
    /// Defines operations for integrating with a payment gateway provider.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Creates a new checkout session or reuses an existing one for the given order.
        /// </summary>
        /// <param name="order">The order snapshot containing payment details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A result containing the redirect URL and provider reference.</returns>
        Task<CreateSessionResult> CreateOrReuseSessionAsync(
            OrderSnapshot order,
            CancellationToken ct = default);

        /// <summary>
        /// Handles incoming webhook requests from the payment provider.
        /// </summary>
        /// <param name="request">The HTTP request containing webhook payload.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A result indicating whether the webhook was handled and the related order number.</returns>
        Task<WebhookResult> HandleWebhookAsync(
            HttpRequest request,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Defines operations for persisting order payment state.
    /// </summary>
    public interface IOrderPayments
    {
        /// <summary>
        /// Updates the payment state for a given order.
        /// </summary>
        /// <param name="orderNumber">The unique order number.</param>
        /// <param name="state">The new payment state.</param>
        /// <param name="providerRef">Optional provider reference identifier.</param>
        /// <param name="ct">Cancellation token.</param>
        Task SetStateAsync(
            string orderNumber,
            PaymentState state,
            string? providerRef = null,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Represents the state of a payment transaction.
    /// </summary>
    public enum PaymentState
    {
        /// <summary>
        /// Payment is awaiting processing.
        /// </summary>
        Pending,

        /// <summary>
        /// Payment is currently being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// Payment has been successfully completed.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Payment has failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Payment has been fully refunded.
        /// </summary>
        Refunded,

        /// <summary>
        /// Payment has been partially refunded.
        /// </summary>
        PartiallyRefunded
    }

    /// <summary>
    /// Represents a snapshot of order details for payment processing.
    /// </summary>
    /// <param name="OrderNumber">The unique order number.</param>
    /// <param name="AmountMinor">The payment amount in minor currency units (e.g., 1299 = £12.99).</param>
    /// <param name="Currency">The three-letter ISO currency code.</param>
    /// <param name="CustomerEmail">The customer's email address.</param>
    /// <param name="SuccessUrl">The URL to redirect to upon successful payment.</param>
    /// <param name="CancelUrl">The URL to redirect to upon payment cancellation.</param>
    public sealed record OrderSnapshot(
        string OrderNumber,
        long AmountMinor,
        string Currency,
        string CustomerEmail,
        Uri SuccessUrl,
        Uri CancelUrl);

    /// <summary>
    /// Represents the result of creating a payment session.
    /// </summary>
    /// <param name="RedirectUrl">The URL to redirect the customer to complete payment.</param>
    /// <param name="ProviderRef">The provider's unique reference for this session.</param>
    public sealed record CreateSessionResult(
        Uri RedirectUrl,
        string ProviderRef);

    /// <summary>
    /// Represents the result of processing a webhook request.
    /// </summary>
    /// <param name="Handled">Indicates whether the webhook was successfully handled.</param>
    /// <param name="OrderNumber">The order number associated with the webhook, if any.</param>
    public sealed record WebhookResult(
        bool Handled,
        string? OrderNumber);
}
