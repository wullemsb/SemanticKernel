using EmailRewriter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using System;
using System.Net;

Console.Write("Your request: ");
//string request = Console.ReadLine()!;
//string history = """
//          <message role="user">I hate sending emails, no one ever reads them.</message>
//          <message role="assistant">I'm sorry to hear that. Messages may be a better way to communicate.</message>
//          """;

//string prompt = $"""
//         <message role="system">Instructions: What is the intent of this request?
//         If you don't know the intent, don't guess; instead respond with "Unknown".
//         Choices: SendEmail, SendMessage, CompleteTask, CreateDocument, Unknown.
//         Bonus: You'll get $20 if you get this right.</message>
        
//         <message role="user">Can you send a very quick approval to the marketing team?</message>
//         <message role="system">Intent:</message>
//         <message role="assistant">SendMessage</message>
        
//         <message role="user">Can you send the full update to the marketing team?</message>
//         <message role="system">Intent:</message>
//         <message role="assistant">SendEmail</message>
        
//         {history}
//         <message role="user">{request}</message>
//         <message role="system">Intent:</message>
//         """;
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.




var builder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "phi3",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"));// With Ollama OpenAI API endpoint

builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Debug));

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder.Plugins
        .AddFromType<Microsoft.SemanticKernel.Plugins.Core.MathPlugin>()
        .AddFromType<ConversationSummaryPlugin>()
        .AddFromType<TimePlugin>("time");
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var kernel = builder.Build();

var chat = kernel.CreateFunctionFromPrompt(
    new PromptTemplateConfig()
    {
        Name = "Chat",
        Description = "Chat with the assistant.",
        Template = @"{{ConversationSummaryPlugin.SummarizeConversation $history}}
                    User: {{$request}}
                    Assistant: ",
        TemplateFormat = "semantic-kernel",
        InputVariables =
        [
            new() { Name = "history", Description = "The history of the conversation.", IsRequired = false, Default = "" },
            new() { Name = "request", Description = "The user's request.", IsRequired = true }
        ],
        ExecutionSettings =
        {
            {
                "default",
                new OpenAIPromptExecutionSettings()
                {
                    MaxTokens = 1000,
                    Temperature = 0.9
                }
            }
        }
    }
);
// Enable auto function calling
//OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
//{
//    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
//};

//KernelArguments arguments = new KernelArguments(openAIPromptExecutionSettings);


//Console.WriteLine(await kernel.InvokePromptAsync(prompt, arguments));
//Console.ReadLine();



ChatHistory history = [];

// Start the chat loop
while (true)
{
    // Get user input
    Console.Write("User > ");
    var request = Console.ReadLine();

    // Get chat response
    var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
        chat,
        new()
        {
            { "request", request },
            { "history", string.Join("\n", history.Select(x => x.Role + ": " + x.Content)) }
        }
    );

    // Stream the response
    string message = "";
    bool rolePrinted = false;

    await foreach (var chunk in chatResult)
    {
        if (chunk.Role.HasValue && !rolePrinted)
        {
            Console.Write(chunk.Role + " > ");
            rolePrinted = true;
        }

        message += chunk;
        Console.Write(chunk);
    }
    Console.WriteLine();

    // Append to history
    history.AddUserMessage(request!);
    history.AddAssistantMessage(message);
}

// Test the math plugin
//double answer = await kernel.InvokeAsync<double>(
//"MathPlugin", "Sqrt",
//    new() {
//        { "number1", 12 }
//    }
//);
//Console.WriteLine($"The square root of 12 is {answer}.");

//const string promptTemplate = @"
//Today is: {{time.Date}}
//Current time is: {{time.Time}}

//Answer to the following questions using JSON syntax, including the data used.
//Is it morning, afternoon, evening, or night (morning/afternoon/evening/night)?
//Is it weekend time (weekend/not weekend)?
//What is the square root of 12?";

//var results = await kernel.InvokePromptAsync(promptTemplate);
//Console.WriteLine(results);

//// Get chat completion service
//var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
//var history = new ChatHistory();

//// Start the conversation
//Console.Write("User > ");
//string? userInput;
//while ((userInput = Console.ReadLine()) != null)
//{
//    history.AddUserMessage(userInput);

//    // Enable auto function calling
//    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
//    {
//        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
//    };

//    // Get the response from the AI
//    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
//                        history,
//                        executionSettings: openAIPromptExecutionSettings,
//                        kernel: kernel);

//    // Stream the results
//    string fullMessage = "";
//    var first = true;
//    await foreach (var content in result)
//    {
//        if (content.Role.HasValue && first)
//        {
//            Console.Write("Assistant > ");
//            first = false;
//        }
//        Console.Write(content.Content);
//        fullMessage += content.Content;
//    }
//    Console.WriteLine();

//    // Add the message from the agent to the chat history
//    history.AddAssistantMessage(fullMessage);

//    // Get user input again
//    Console.Write("User > ");
//}

#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.