using EmailRewriter.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;

[ApiController]
[Route("api/[controller]")]
public class RewriteController([FromKeyedServices("phi35")]Kernel phi35Kernel, [FromKeyedServices("llama31")] Kernel llama31Kernel) : ControllerBase
{
    private readonly Kernel _phi35Kernel = phi35Kernel;
    private readonly Kernel _llama31Kernel = llama31Kernel;

    //[HttpPost]
    //public IAsyncEnumerable<string> Rewrite([FromBody] EmailContentModel model)
    //{
    //    return RewriteEmailContent(model.Content);
    //}
    [HttpPost]
    public async Task Rewrite([FromBody] EmailContentModel model, CancellationToken token)
    {
        this.Response.StatusCode = 200;
        this.Response.Headers.Append(HeaderNames.ContentType, "application/octet-stream");
        var outputStream = this.Response.Body;
        await foreach (var part in RewriteEmailContent(model.Content, token))
        {
           var data = new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(part));

            await outputStream.WriteAsync(data);
        }

        await outputStream.FlushAsync();
    }

    private async IAsyncEnumerable<string> RewriteEmailContent(string content, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        var reward = "You get a 100$ bonus if you can keep it short.";

        var prompt = $"""
            You are an editor-in-chief and responsible to redact a text before it is published. 
            When redacting you apply the following rules:
            1. Keep the text concise and to the point.
            2. Don't waste words.
            3. Use short and common words.
            4. Use short, clear, complete sentences. 
            5. Split long paragraphs into shorter ones.

            Bonus: {reward}
            """;
#pragma warning disable SKEXP0110, SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        // Define the agent
        ChatCompletionAgent agent =
            new()
            {
                Instructions = prompt,
                Name = "Editor",
                Kernel = _phi35Kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { ToolCallBehavior = null }),
            };

        var chatMessageContent = new ChatMessageContent(AuthorRole.System, prompt);
        var chatMessages = new ChatHistory(new List<ChatMessageContent>{ chatMessageContent });
        chatMessages.AddUserMessage($"Can you edit the following content to make it more readible? {content}");

        var message = string.Empty;

        await foreach (ChatMessageContent response in agent.InvokeAsync(chatMessages))
        {
            // Print the results
            yield return response.Content;
            
            message +=response.Content;
            // Add the message from the agent to the chat history
            chatMessages.Add(response);
        }

        var kernelArguments = new KernelArguments()
        {
            {"input", content }
        };

        yield return "<br /><br />";

        var readability = await _phi35Kernel.InvokeAsync<double>("ReadabilityPlugin", "CalculateReadability", arguments: new() { { "body", message } },cancellationToken:token);
        yield return "<b>Readability index</b>: " + readability;

        yield return "<br /><br />";

        //https://github.com/microsoft/semantic-kernel/tree/main/dotnet/src/Plugins/Plugins.Core
        var summary = await _phi35Kernel.InvokeAsync<string>("ConversationSummaryPlugin", "SummarizeConversation", kernelArguments, cancellationToken: token);
        yield return "tl;dr "+summary;

        yield return "<br /><br />";

        var actions = await _phi35Kernel.InvokeAsync<string>("ConversationSummaryPlugin", "GetConversationActionItems", kernelArguments, cancellationToken: token);
        yield return "Actions:" + actions;
#pragma warning restore SKEXP0110,SKEXP0010
    }
}
