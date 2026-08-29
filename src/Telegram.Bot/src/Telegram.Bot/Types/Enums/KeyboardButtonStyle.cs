namespace Telegram.Bot.Types.Enums;

/// <summary>
/// Visual style of a keyboard button.
/// </summary>
[JsonConverter(typeof(KeyboardButtonStyleConverter))]
public enum KeyboardButtonStyle
{
    /// <summary>
    /// Red button.
    /// </summary>
    Danger = 1,

    /// <summary>
    /// Green button.
    /// </summary>
    Success,

    /// <summary>
    /// Blue button.
    /// </summary>
    Primary,
}
