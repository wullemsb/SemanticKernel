using EmailRewriter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using System;
using System.Net;
using System.Net.Http;

#pragma warning disable SKEXP0010,SKEXP0060,SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

HttpClient client = new HttpClient();
client.Timeout= TimeSpan.FromMinutes(2);


var builder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "phi3:medium",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"),
        httpClient: client);// With Ollama OpenAI API endpoint

// Create the kernel
builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
builder.Plugins.AddFromType<AuthorEmailPlanner>();
builder.Plugins.AddFromType<MathPlugin>();
builder.Plugins.AddFromType<EmailPlugin>();
Kernel kernel = builder.Build();

var options = new FunctionCallingStepwisePlannerOptions
{
    MaxIterations = 5,
    MaxTokens = 4000,
};
var planner = new FunctionCallingStepwisePlanner(options);

string[] questions =
       [
           "What is the current hour number, plus 5?",
            "What is 387 minus 22? Email the solution to John and Mary.",
            "Write a limerick, translate it to Spanish, and send it to Jane",
        ];

foreach (var question in questions)
{
    FunctionCallingStepwisePlannerResult result = await planner.ExecuteAsync(kernel, question);
    Console.WriteLine($"Q: {question}\nA: {result.FinalAnswer}");

    // You can uncomment the line below to see the planner's process for completing the request.
    Console.WriteLine($"Chat history:\n{System.Text.Json.JsonSerializer.Serialize(result.ChatHistory)}");
}


// Retrieve the chat completion service from the kernel
IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create the chat history
ChatHistory chatMessages = new ChatHistory("""
You are a friendly assistant who likes to follow the rules. You will complete required steps
and request approval before taking any consequential actions. If the user doesn't provide
enough information for you to complete a task, you will keep asking questions until you have
enough information to complete the task.
""");

// Start the conversation
while (true)
{
    // Get user input
    System.Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    // Get the chat completions
    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Stream the results
    string fullMessage = "";
    bool roleWritten = false;
    await foreach (var content in result)
    {
        if (content.Role.HasValue && !roleWritten)
        {
            System.Console.Write("Assistant > ");
            roleWritten = true;
        }
        System.Console.Write(content.Content);
        fullMessage += content.Content;
    }
    System.Console.WriteLine();

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.