var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.EmailRewriter_Web>("emailrewriter-web");

builder.Build().Run();
