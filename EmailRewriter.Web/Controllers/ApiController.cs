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
    public async Task Rewrite([FromBody] EmailContentModel model)
    {
        this.Response.StatusCode = 200;
        this.Response.Headers.Append(HeaderNames.ContentType, "application/octet-stream");
        var outputStream = this.Response.Body;
        await foreach (var part in RewriteEmailContent(model.Content))
        {
           var data = new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(part));

            await outputStream.WriteAsync(data);
        }

        await outputStream.FlushAsync();
    }


    private async IAsyncEnumerable<string> RewriteEmailContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        var prompt = @$"You are an editor-in-chief and responsible to redact a text before it is published. 
                       When redacting you strictly apply the following rules:
            1. Keep the text concise and to the point.
            2. Use shorter and more common words.
            3. Use shorter and simpler sentences.
            4. Put the most important information at the top in a tl;dr";
            //6. Include headings where applicable
            //7. Use bullet points where applicable
            //8. Split long paragraphs into shorter ones
            //9. Use enough formatting but no more
            //10. Tell readers why they should care
            //11. Make responding easy";

        //var result= await _semanticKernel.InvokePromptAsync(prompt);
        IChatCompletionService chatCompletionService = _semanticKernel.GetRequiredService<IChatCompletionService>();

        //return result.ToString();
        var chatMessages = new ChatHistory(prompt);
        chatMessages.AddUserMessage($"Can you edit the following content? {content}");
        //chatMessages.AddSystemMessage("Format the answer as html.");
        // Get the chat completions
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        var result=chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatMessages,
            executionSettings: openAIPromptExecutionSettings,
            kernel: _semanticKernel);

        await foreach (var part in result)
        {
            yield return part.Content;
        }
    }
}
