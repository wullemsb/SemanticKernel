using EmailRewriter.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

[ApiController]
[Route("api/[controller]")]
public class RewriteController(Kernel semanticKernel) : ControllerBase
{
    private readonly Kernel _semanticKernel = semanticKernel;

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

            Bonus: {reward}
            """;

        var chatMessageContent = new ChatMessageContent(AuthorRole.System, prompt);
        //6. Include headings where applicable
        //7. Use bullet points where applicable
        //8. Split long paragraphs into shorter ones
        //9. Use enough formatting but no more
        //10. Tell readers why they should care
        //11. Make responding easy";

        //Just invoke a prompt
        //var result= await _semanticKernel.InvokePromptAsync(prompt);
        //return result.ToString();

        IChatCompletionService chatCompletionService = _semanticKernel.GetRequiredService<IChatCompletionService>();

       
        var chatMessages = new ChatHistory(new List<ChatMessageContent>{ chatMessageContent });
        chatMessages.AddUserMessage($"Can you edit the following content? {content}");
        // Get the chat completions
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = null//ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var result=chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatMessages,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _semanticKernel,cancellationToken:token);

        var message = string.Empty;

        await foreach (var part in result)
        {
            message += part.Content;
            yield return part.Content;
        }
    }
}
