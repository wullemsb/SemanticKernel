using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.SemanticKernel.Extensions;

namespace EmailRewriter.Web.Controllers
{
    [ApiController]
    [Route("api/outlook")]
    public class OutlookController([FromKeyedServices("gpt4o")] Kernel kernel) : ControllerBase
    {
        [HttpGet("copyfromoutlook")]
        public async Task<IActionResult> CopyFromOutlook()
        {
            // 💡 Add this line to enable MCP functions from a Stdio server named "Everything"
            await kernel.Plugins.AddMcpFunctionsFromSseServerAsync("Playwright", "http://localhost:8931/sse");

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var prompt = "Open a browser window and navigate to the email client running at the following URL: https://localhost:7252/ .";
            var result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

            prompt = "Now, please capture an accessibility snapshot of the current page and extract the email content out of it and return only the extracted email content as a string value and nothing else. Don't alter the found content in any way.";
            result = await kernel.InvokePromptAsync(prompt, new(executionSettings));

            var emailContent = new
            {
                Content = result.GetValue<string>()
            };

            return Ok(emailContent);
        }
    }
}
