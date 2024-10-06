using EmailRewriter.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

[ApiController]
[Route("api/[controller]")]
public class CalculateController(Kernel semanticKernel) : ControllerBase
{
    private readonly Kernel _semanticKernel = semanticKernel;

    public record CalculatedIndex(double Index);        
    
    

    [HttpPost]
    public async Task<CalculatedIndex> CalculateReadabilityIndex([FromBody] EmailContentModel model)
    {
        //Calculate the readability index
        var readabilityIndex = ReadabilityCalculator.CalculateGunningFogIndex(model.Content);
        //var readabilityIndex = await ReadabilityCalculator.CalculateGunningFogIndexThroughAI(_semanticKernel, model.Content);
        return new CalculatedIndex(readabilityIndex);
    }  
}
