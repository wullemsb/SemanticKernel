using Microsoft.SemanticKernel;
using System.Net;

Console.Write("Your request: ");
string request = Console.ReadLine()!;
string prompt = $$"""
         ## Instructions
         Provide the intent of the request using the following format:
         
         ```json
         {
             "intent": {intent}
         }
         ```
         
         ## Choices
         You can choose between the following intents:
         
         ```json
         ["SendEmail", "SendMessage", "CompleteTask", "CreateDocument"]
         ```
         
         ## User Input
         The user input is:
         
         ```json
         {
             "request": "{{request}}"
         }
         ```
         
         ## Intent
         """;
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "phi3",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434")) // With Ollama OpenAI API endpoint
    .Build();

Console.WriteLine(await kernel.InvokePromptAsync(prompt));
Console.ReadLine();

#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.