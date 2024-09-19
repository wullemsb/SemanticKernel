using EmailRewriter.Web;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

#pragma warning disable SKEXP0010,SKEXP0060,SKEXP0050

HttpClient client = new HttpClient();
client.Timeout = TimeSpan.FromMinutes(2);


var gpt4oBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName: "gpt-4o", endpoint: builder.Configuration["OpenAI:apiUrl"], apiKey: builder.Configuration["OpenAI:apiKey"]);
gpt4oBuilder
    .Plugins
        .AddFromType<ConversationSummaryPlugin>()
        .AddFromType<EmailReadabilityPlugin>("ReadabilityPlugin");



var phi35Builder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "phi3.5:latest",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"));// With Ollama OpenAI API endpoint


builder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());

var llama31Builder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "llama3.1:latest",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"));// With Ollama OpenAI API endpoint

using var loggerFactory = LoggerFactory.Create(builder =>
{
    // Add OpenTelemetry as a logging provider
    builder.AddOpenTelemetry(options =>
    {
        options.AddOtlpExporter();
        // Format log messages. This defaults to false.
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Trace);
});

phi35Builder.Services.AddSingleton(loggerFactory);
llama31Builder.Services.AddSingleton(loggerFactory);
gpt4oBuilder.Services.AddSingleton(loggerFactory);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("Microsoft.SemanticKernel*")
    .AddOtlpExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter("Microsoft.SemanticKernel*")
    .AddOtlpExporter()
    .Build();

phi35Builder.Plugins.AddFromType<ConversationSummaryPlugin>();
phi35Builder.Plugins.AddFromType<EmailReadabilityPlugin>("ReadabilityPlugin");
llama31Builder.Plugins.AddFromType<ConversationSummaryPlugin>();
llama31Builder.Plugins.AddFromType<EmailReadabilityPlugin>("ReadabilityPlugin");

builder.Services.AddKeyedSingleton("phi35", phi35Builder.Build());
builder.Services.AddKeyedSingleton("llama31",llama31Builder.Build());
builder.Services.AddKeyedSingleton("gpt4o", gpt4oBuilder.Build());

#pragma warning restore SKEXP0010,SKEXP0060,SKEXP0050

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
