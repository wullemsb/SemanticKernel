using EmailRewriter.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class RewriteController([FromKeyedServices("phi35")]Kernel phi35Kernel, [FromKeyedServices("llama31")] Kernel llama31Kernel, [FromKeyedServices("gpt4o")] Kernel gpt4oKernel) : ControllerBase
{
    private readonly Kernel _phi35Kernel = phi35Kernel;
    private readonly Kernel _llama31Kernel = llama31Kernel;
    private readonly Kernel _gpt4oKernel = gpt4oKernel;

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

        var copywriter = $"""
            You are a copywriter and responsible to redact a text before it is published. 
            When redacting you apply the following rules:
            1. Keep the text concise and to the point.
            2. Don't waste words.
            3. Use short and common words.
            4. Use short, clear, complete sentences. 
            5. Split long paragraphs into shorter ones.

            Only provide a single proposal per response.
            You're laser focused on the goal at hand.
            Don't waste time with chit chat.
            Consider suggestions when refining an idea.

            Also calculate the readibility index of the text and provide a summary.
            """;

        string spellingCorrector = """
   You are a spelling corrector. You review a text and correct any spelling mistakes before handing it over to a copywriter. Ensure all words are spelled correctly without altering the meaning or structure of the original text.
""";

        string reviewer = """
    You are an art director who has opinions about copywriting born of a love for David Ogilvy.
The goal is to determine if the given copy is acceptable to print.
If so, state that it is approved.
If not, provide insight on how to refine suggested copy without example.
"""
        ;

       

#pragma warning disable SKEXP0110, SKEXP0010, SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


        ChatCompletionAgent copywriterAgent =
                   new()
                   {
                       Instructions = copywriter,
                       Name = "CopywriterAgent",                    
                       Kernel = _gpt4oKernel,
                       Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions }),
                   };

        ChatCompletionAgent spellingAgent =
                   new()
                   {
                       Instructions = spellingCorrector,
                       Name = "SpellingAgent",
                       Kernel = _gpt4oKernel
                   };

        ChatCompletionAgent reviewerAgent =
                   new()
                   {
                       Instructions = reviewer,
                       Name = "ReviewerAgent",
                       Kernel = _gpt4oKernel
                   };

        AgentGroupChat groupChat =
                    new(spellingAgent,copywriterAgent, reviewerAgent) // order matters!
                    {
                        ExecutionSettings =
                            new()
                            {
                                TerminationStrategy =

                                    new ApprovalTerminationStrategy()
                                    {
                                        Agents = [reviewerAgent],
                                        MaximumIterations = 3,
                                    }                  
                            }
                    };

        groupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User,$"Can you edit the following content to fix spelling errors and make it more readible? {content}"));

        var message = string.Empty;

        await foreach (ChatMessageContent response in groupChat.InvokeAsync(cancellationToken:token))
        {
            // Print the results
            yield return $"<br /># {response.Role} - {response.AuthorName ?? "*"}: '{response.Content}'";
            
            message +=response.Content;
            //groupChat.AddChatMessage(response);
        }

        var kernelArguments = new KernelArguments()
        {
            {"input", content }
        };

        //yield return "<br /><br />";

        //var readability = await _phi35Kernel.InvokeAsync<double>("ReadabilityPlugin", "CalculateReadability", arguments: new() { { "body", message } },cancellationToken:token);
        //yield return "<b>Readability index</b>: " + readability;

        //yield return "<br /><br />";

        ////https://github.com/microsoft/semantic-kernel/tree/main/dotnet/src/Plugins/Plugins.Core
        //var summary = await _phi35Kernel.InvokeAsync<string>("ConversationSummaryPlugin", "SummarizeConversation", kernelArguments, cancellationToken: token);
        //yield return "tl;dr "+summary;

        //yield return "<br /><br />";

        //var actions = await _phi35Kernel.InvokeAsync<string>("ConversationSummaryPlugin", "GetConversationActionItems", kernelArguments, cancellationToken: token);
        //yield return "Actions:" + actions;
#pragma warning restore SKEXP0110,SKEXP0010,SKEXP0001
    }
}
