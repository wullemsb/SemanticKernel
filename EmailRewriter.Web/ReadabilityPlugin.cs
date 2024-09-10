using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace EmailRewriter.Web
{
    public class EmailReadabilityPlugin
    {
        [KernelFunction]
        [Description("Calculates the readability of an email.")]
        [return: Description("A calculated number representing the readability of an email.")]
        public async Task<double> CalculateReadabilityAsync(
            Kernel kernel,
            [Description("The body of an email")] string body
        )
        {
            return ReadabilityCalculator.CalculateGunningFogIndex(body);
        }
    }
}
