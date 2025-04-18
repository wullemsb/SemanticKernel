using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using OllamaSharp.Models.Chat;
using YamlDotNet.Core.Tokens;

namespace EmailRewriter.Web
{
    public class TagExtractor
    {
        public static async Task<IList<string>> ExtractTags(Kernel kernel, string emailContent)
        {
            var result = await kernel.InvokeAsync<string?>("ConversationSummaryPlugin", "GetConversationTopics", arguments: new() { { "input", emailContent } });

            return [.. System.Text.Json.JsonSerializer.Deserialize<Summary>(result)?.topics];
        }
    }

    public class Summary
    {
        public string[] topics { get; set; }
    }
}
