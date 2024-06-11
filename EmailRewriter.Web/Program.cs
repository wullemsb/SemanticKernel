using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

var builder = WebApplication.CreateBuilder(args);

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

// Create the kernel
semanticKernelBuilder.Services.AddLogging(c => c.SetMinimumLevel(LogLevel.Trace).AddDebug());
//semanticKernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
semanticKernelBuilder.Plugins.AddFromType<MathPlugin>();
semanticKernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
//semanticKernelBuilder.Plugins.AddFromType<EmailPlugin>();
Kernel kernel = semanticKernelBuilder.Build();
builder.Services.AddSingleton(kernel);

#pragma warning restore SKEXP0010,SKEXP0060,SKEXP0050

var app = builder.Build();

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
