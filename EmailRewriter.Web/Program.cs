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


var semanticKernelBuilder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(                        // We use Semantic Kernel OpenAI API
        modelId: "phi3",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"),
        httpClient: client);// With Ollama OpenAI API endpoint

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

// Create the kernel
//semanticKernelBuilder.Services.AddLogging(c => 
//        c.SetMinimumLevel(LogLevel.Trace)
//        .AddOpenTelemetry(configure =>
//        {
//            configure.IncludeFormattedMessage = true;
//            configure.IncludeScopes = true;
//        })
//        .AddDebug());

semanticKernelBuilder.Services.AddSingleton(loggerFactory);

using var traceProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("Microsoft.SemanticKernel*")
    .AddOtlpExporter()
    .Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter("Microsoft.SemanticKernel*")
    .AddOtlpExporter()
    .Build();

semanticKernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();

Kernel kernel = semanticKernelBuilder.Build();
builder.Services.AddSingleton(kernel);

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
