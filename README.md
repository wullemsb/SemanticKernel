# EmailRewriter

Sample web app showing how to use Semantic Kernel and Ollama OpenAI integration to build an email rewriter.

## Features
- Rewrite emails with improved readability and tone.
- Integration with Semantic Kernel for advanced AI capabilities.
- Support for multiple AI models, including OpenAI and Ollama.

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Qdrant](https://qdrant.tech/) installed and running locally.
- Ollama API running locally on `http://localhost:11434`.
- Playwright MCP server running locally on `http://localhost:8931/sse`.
## Setup Instructions
1. Clone the repository:
~~~~
git clone
~~~~
   
2. Restore dependencies:
~~~~
dotnet restore
~~~~
   
3. Configure user secrets:

In case you want to use your own (Azure) OpenAI API key, you can set it up using the following command:
~~~~
dotnet user-secrets set "OpenAI:apiUrl" "12345"
dotnet user-secrets set "OpenAI:apiKey" "12345"
~~~~

4. Ensure Qdrant and Ollama APIs are running locally:
   - Qdrant should be accessible at `localhost`.
   - Ollama API should be accessible at `http://localhost:11434`.

## Running the Application Locally
1. Build the project:
~~~~
dotnet build
~~~~
2. Run the application:
~~~~
dotnet run
~~~~
3. Open your browser and navigate to `https://localhost:5001` (or the URL specified in the console output).

## Additional Notes
- The application uses Semantic Kernel plugins for email readability and summarization.
- Static files and views are served from the `wwwroot` and `Views` directories, respectively.
- For more details on configuring Semantic Kernel, refer to the [official documentation](https://learn.microsoft.com/semantic-kernel/).
