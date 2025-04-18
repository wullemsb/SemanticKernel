var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container (if needed for Razor Pages or other features)
builder.Services.AddRazorPages();

var app = builder.Build();

app.MapDefaultEndpoints();

// Enable serving static files
app.UseStaticFiles();

// Map a route to serve the HTML file
app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("outlook-interface.html");
});

app.Run();
