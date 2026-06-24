using Microsoft.Extensions.AI;
using System.Text.Json;
using WebLibrary.AgenticApi.Models;

namespace WebLibrary.AgenticApi.Agents
{
    public class MetadataExtractorAgent : IMetadataExtractorAgent
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<MetadataExtractorAgent> _logger;

        private const string SystemPrompt = """
        You are a professional book metadata extraction specialist.
        You are given extracted text from the first pages of a book.
        Your job is to identify and extract key metadata from this text.

        Rules:
        - Extract only what is clearly present in the text
        - Do not guess or hallucinate information not present
        - For ConfidenceScore: 1.0 means completely certain, 0.0 means pure guess
        - Description must be a concise summary under 500 characters
        - If you cannot find a field with confidence, set it to empty string
          and lower the ConfidenceScore accordingly

        Always respond with valid JSON only.
        No explanation, no markdown, no backticks. Only the JSON object.

        JSON schema to follow exactly:
        {
            "Title": "string",
            "Author": "string",
            "Description": "string (max 500 chars)",
            "ConfidenceScore": number between 0.0 and 1.0
        }
        """;

        public MetadataExtractorAgent(
            IChatClient chatClient,
            ILogger<MetadataExtractorAgent> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<MetadataAgentOutput> ExtractAsync(
            string extractedText,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("MetadataExtractorAgent started");

            var userMessage = $"""
            Here is the extracted text from the first pages of the book.
            Extract the metadata as instructed:

            {extractedText}
            """;

            var messages = new List<ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, userMessage)
        };

            var response = await _chatClient.GetResponseAsync(
                messages,
                cancellationToken: cancellationToken);

            var responseText = response.Text?.Trim()
                ?? throw new InvalidOperationException(
                    "MetadataExtractorAgent received empty response from LLM");

            _logger.LogInformation(
                "MetadataExtractorAgent received response, length: {Length}",
                responseText.Length);

            var output = JsonSerializer.Deserialize<MetadataAgentOutput>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })
                ?? throw new InvalidOperationException(
                    "MetadataExtractorAgent could not deserialize LLM response");

            _logger.LogInformation(
                "Metadata extracted. Title: {Title}, Confidence: {Confidence}",
                output.Title,
                output.ConfidenceScore);

            return output;
        }
    }
}
