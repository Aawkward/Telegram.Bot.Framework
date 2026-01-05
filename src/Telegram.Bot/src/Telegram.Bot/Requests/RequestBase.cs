using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Requests;

/// <summary>
/// Represents an API request
/// </summary>
/// <typeparam name="TResponse">Type of result expected in result</typeparam>
[JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public abstract class RequestBase<TResponse> : IRequest<TResponse>
{
    /// <inheritdoc />
    [JsonIgnore]
    public HttpMethod Method { get; }

    /// <inheritdoc />
    [JsonIgnore]
    public string MethodName { get; }

    /// <summary>
    /// Initializes an instance of request
    /// </summary>
    /// <param name="methodName">Bot API method</param>
    protected RequestBase(string methodName)
        : this(methodName, HttpMethod.Post)
    { }

    /// <summary>
    /// Initializes an instance of request
    /// </summary>
    /// <param name="methodName">Bot API method</param>
    /// <param name="method">HTTP method to use</param>
    protected RequestBase(string methodName, HttpMethod method) =>
        (MethodName, Method) = (methodName, method);

    /// <summary>
    /// Generate content of HTTP message
    /// </summary>
    /// <returns>Content of HTTP request</returns>
    public virtual HttpContent? ToHttpContent() =>
        new StringContent(
            content: JsonConvert.SerializeObject(this),
            encoding: Encoding.UTF8,
            mediaType: "application/json"
        );

    /// <inheritdoc />
    [JsonIgnore]
    public bool IsWebhookResponse { get; set; }

    /// <summary>
    /// If <see cref="IsWebhookResponse"/> is set to <see langword="true"/> is set to the method
    /// name, otherwise it won't be serialized
    /// </summary>
    [JsonProperty("method", DefaultValueHandling = DefaultValueHandling.Ignore)]
    internal string? WebHookMethodName => IsWebhookResponse ? MethodName : default;

    /// <inheritdoc />
    public void Reset()
    {
        ResetAllStreams(this, new HashSet<int>());

        static void ResetAllStreams(object? request, HashSet<int> visited)
        {
            if (request == null) return;

            var type = request.GetType();

            if (!type.IsValueType && request is not string)
            {
                var id = RuntimeHelpers.GetHashCode(request);
                if (!visited.Add(id)) return;
            }

            if (request is Stream selfStream)
            {
                try
                {
                    if (selfStream.CanSeek) selfStream.Position = 0;
                }

                catch { /* ignore */ }
                return;
            }

            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0) continue;

                object? value;

                try { value = prop.GetValue(request); } catch { continue; }

                if (value == null) continue;

                if (value is Stream directStream)
                {
                    try
                    {
                        if (directStream.CanSeek) directStream.Position = 0;
                    }
                    catch { /* ignore */ }
                }
                else if (value is IEnumerable collection && value is not string)
                {
                    try
                    {
                        foreach (var item in collection)
                        {
                            ResetAllStreams(item, visited);
                        }
                    }
                    catch { /* ignore */ }
                }
                else
                {
                    var contentProp = value.GetType().GetProperty("Content");

                    if (contentProp != null && typeof(Stream).IsAssignableFrom(contentProp.PropertyType))
                    {
                        try
                        {
                            if (contentProp.GetValue(value) is Stream stream && stream.CanSeek) stream.Position = 0;
                        }
                        catch { /* ignore */ }
                    }

                    ResetAllStreams(value, visited);
                }
            }
        }
    }
}
