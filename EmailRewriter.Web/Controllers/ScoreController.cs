using EmailRewriter.Web;
using EmailRewriter.Web.Models;
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
public class ScoreController([FromKeyedServices("phi35")] Kernel phi35Kernel, ITextEmbeddingGenerationService textEmbeddingGenerationService) : ControllerBase
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
{
    [HttpPost("thumbsup")]
    public async Task<IActionResult> ThumbsUp()
    {
        var redactedEmail = RewriteController.RedactedEmail;
        if (redactedEmail == null)
            return Ok();

        var tags = await TagExtractor.ExtractTags(phi35Kernel, redactedEmail.Content);

        var emailText = new EmailText
        {
            Key = Guid.NewGuid(),
            Tags = new (tags),
            Text = redactedEmail.Content
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
        // Create a Qdrant VectorStore object
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));
        var collection = vectorStore.GetCollection<Guid, EmailText>("emails");

        var sampleEmail = "Hi there, welcome at CloudBrew! I hope you enjoy the conference.";

        var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(sampleEmail);

        var vectorSearchOptions = new VectorSearchOptions<EmailText>
        {
            Filter = email => email.Tags.Contains("CloudBrew"),
            Top =2
        };

        // Do the search, passing an options object with a Top value to limit resulst to the single top match.
        var searchResult = await collection.VectorizedSearchAsync(searchVector, vectorSearchOptions);

        await foreach (var record in searchResult.Results)
        {
            Console.WriteLine("Email key " + record.Record.Key);
            Console.WriteLine("Email text: " + record.Record.Text);
        }

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

    // Tags is marked as filterable, since we want to filter using this property.
    [VectorStoreRecordData(IsFilterable = true)]
    public List<string> Tags { get; set; }

    [VectorStoreRecordVector(768, StoragePropertyName = "email_text_embedding")]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
