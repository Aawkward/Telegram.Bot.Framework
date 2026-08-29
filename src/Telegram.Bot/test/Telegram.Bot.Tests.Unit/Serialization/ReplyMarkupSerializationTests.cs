using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit;

namespace Telegram.Bot.Tests.Unit.Serialization;

public class ReplyMarkupSerializationTests
{
    [Theory(DisplayName = "Should serialize request poll keyboard button")]
    [InlineData(null)]
    [InlineData("regular")]
    [InlineData("quiz")]
    public void Should_Serialize_Request_Poll_Keyboard_Button(string type)
    {
        IReplyMarkup replyMarkup = new ReplyKeyboardMarkup(
            KeyboardButton.WithRequestPoll("Create a poll", type)
        );

        string serializedReplyMarkup = JsonConvert.SerializeObject(replyMarkup);

        string formattedType = string.IsNullOrEmpty(type)
            ? "{}"
            : $@"{{""type"":""{type}""}}";

        string expectedString = $@"""request_poll"":{formattedType}";

        Assert.Contains(expectedString, serializedReplyMarkup);
    }

    [Fact(DisplayName = "Should serialize custom emoji and style for a reply keyboard button")]
    public void Should_Serialize_Custom_Emoji_Reply_Keyboard_Button()
    {
        var button = new KeyboardButton("Subscription")
        {
            IconCustomEmojiId = "5372917041193828849",
            Style = KeyboardButtonStyle.Primary,
        };

        string serializedButton = JsonConvert.SerializeObject(button);

        Assert.Contains(@"""icon_custom_emoji_id"":""5372917041193828849""", serializedButton);
        Assert.Contains(@"""style"":""primary""", serializedButton);
    }

    [Fact(DisplayName = "Should serialize custom emoji and style for an inline keyboard button")]
    public void Should_Serialize_Custom_Emoji_Inline_Keyboard_Button()
    {
        var button = InlineKeyboardButton.WithCallbackData("Subscription", "subscription:open");
        button.IconCustomEmojiId = "5372917041193828849";
        button.Style = KeyboardButtonStyle.Success;

        string serializedButton = JsonConvert.SerializeObject(button);

        Assert.Contains(@"""icon_custom_emoji_id"":""5372917041193828849""", serializedButton);
        Assert.Contains(@"""style"":""success""", serializedButton);
        Assert.Contains(@"""callback_data"":""subscription:open""", serializedButton);
    }
}
