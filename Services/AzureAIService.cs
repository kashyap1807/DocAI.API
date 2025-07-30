using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace DocAI.API.Services
{
    public class AzureAIService
    {
        private string? endPoint;
        private string? key;

        public AzureAIService(IConfiguration configuration)
        {
            endPoint = new Uri(configuration["Azure:OpenAIEndpoint"]).ToString();
            key = configuration["Azure:OpenAIKey"].ToString();
        }

        public async Task<string> AskQuestionAsync(string context,string question)
        {
            if(string.IsNullOrEmpty(endPoint))
            {
                Console.WriteLine("Please set the Azure OpenAPI EndPoint environment variable");
                return null;
            }
            if(string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Please set the Azure OpenAPI Key environment variable");
                return null;
            }

            AzureKeyCredential credential = new AzureKeyCredential(key);

            AzureOpenAIClient azureClient = new(new Uri(endPoint), credential);

            ChatClient chatClient = azureClient.GetChatClient("gpt-4o");

            ChatCompletion completion1 = chatClient.CompleteChat(
                    new List<ChatMessage>()
                    {
                        new UserChatMessage(question)
                    },
                    new ChatCompletionOptions
                    {
                        Temperature = (float)0.7,
                        TopP = (float)0.95,
                        FrequencyPenalty = (float)0.0,
                        PresencePenalty = (float)0.0
                    }
                );

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are a helpful assistant that answers questions based on the provided context."),
                new UserChatMessage($"Document Content:\n{context}"),
                new UserChatMessage($"Question: {question}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = (float)0.7,
                MaxOutputTokenCount = 800,
                TopP = (float)0.95,
                FrequencyPenalty = (float)0.0,
                PresencePenalty = (float)0.0
            };

            try
            {
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

                if(completion != null)
                {
                    Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
                    return JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true });
                }
                else
                {
                    Console.WriteLine("No response received.");
                    return "No response received.";
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return "";
            }
        }
    }
}
