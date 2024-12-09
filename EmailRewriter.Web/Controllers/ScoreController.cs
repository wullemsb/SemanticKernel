using EmailRewriter.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.VectorData;
using Microsoft.Net.Http.Headers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using System.Collections;
namespace EmailRewriter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class ScoreController(ITextEmbeddingGenerationService textEmbeddingGenerationService) : ControllerBase
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
    [HttpPost("thumbsup")]
    public async Task<IActionResult> ThumbsUp([FromBody] EmailContentModel model)
    {
        var emailText = new EmailText
        {
            Key = Guid.NewGuid(),
            Text = model.Content
        };

        // Create a Qdrant VectorStore object
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));
        var collection = vectorStore.GetCollection<Guid, EmailText>("emails");
        await collection.CreateCollectionIfNotExistsAsync();

        // Generate the text embedding.
        Console.WriteLine($"Generating embedding for paragraph: {emailText.Key}");
        emailText.TextEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(emailText.Text);

        // Upload the text paragraph.
        await collection.UpsertAsync(emailText);

        return Ok();
    }

    [HttpPost("thumbsdown")]
    public async Task<IActionResult> ThumbsDown([FromBody] EmailContentModel model)
    {
        return Ok();
    }
}

internal class EmailText
{
    /// <summary>A unique key for the email paragraph.</summary>
    [VectorStoreRecordKey]
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    [TextSearchResultName]
    public required Guid Key { get; init; }

    /// <summary>The text of the paragraph.</summary>
    [VectorStoreRecordData(IsFullTextSearchable = true, StoragePropertyName = "email_text")]
    [TextSearchResultValue]
    public required string Text { get; init; }

    [VectorStoreRecordVector(768, StoragePropertyName = "email_text_embedding")]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
