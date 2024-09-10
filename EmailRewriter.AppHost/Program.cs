var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.EmailRewriter_Web>("emailrewriter-web");
https://www.nuget.org/packages/Raygun.Aspire.Hosting.Ollama
//TODO: Add ollama endpoint

builder.Build().Run();
