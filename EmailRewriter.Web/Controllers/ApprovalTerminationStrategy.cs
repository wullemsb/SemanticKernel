using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Chat;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.