// ReSharper disable once CheckNamespace
namespace Telegram.Bot.Requests;

/// <summary>
/// Use this method to refund star payment. On success, the sent <see cref="bool"/> is returned.
/// </summary>
[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class RefundStarPaymentRequest : RequestBase<bool>
{
    /// <summary>
    /// Unique identifier for the target chat or username of the target channel
    /// (in the format <c>@channelusername</c>)
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public long UserId { get; }

    /// <summary>
    /// Telegram payment charge id.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string TelegramPaymentChargeId { get; }

    /// <summary>
    /// Initializes a new request with chatId, telegramPaymentChargeId.
    /// </summary>
    /// <param name="userId">
    /// Unique identifier for the target chat or username of the target channel
    /// (in the format <c>@channelusername</c>)
    /// </param>
    /// <param name="telegramPaymentChargeId">Telegram payment charge id</param>
    public RefundStarPaymentRequest(
        long userId,
        string telegramPaymentChargeId) : base("refundStarPayment")
    {
        UserId = userId;
        TelegramPaymentChargeId = telegramPaymentChargeId;
    }
}
