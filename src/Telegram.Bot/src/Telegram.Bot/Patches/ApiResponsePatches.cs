using System.Net;
using Telegram.Bot.Exceptions;

namespace Telegram.Bot.Patches
{
    internal static class ApiResponsePatches
    {
        private static readonly string NotEnoughRights =
            "Bad Request: not enough rights to send text messages to the chat";

        public static ApiResponse IfNotEnoughRights(this ApiResponse failedApiResponse)
        {
            if (string.Equals(failedApiResponse.Description, NotEnoughRights, StringComparison.Ordinal))
            {
                return new ApiResponse(
                    (int)HttpStatusCode.Forbidden,
                    NotEnoughRights,
                    failedApiResponse.Parameters);
            }

            return failedApiResponse;
        }
    }
}
