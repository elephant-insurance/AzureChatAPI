using Azure.AI.OpenAI;
using Azure;
using Newtonsoft.Json;

namespace AzureChatAPI
{
    public class AI
    {
        public static async Task<AnswerResults> DoAsync(AIConfig aiConfig, AISearchConfig aiSearchConfig, string question)
        {
            AnswerResults answerResults = new AnswerResults();
            var client = new OpenAIClient(new Uri(aiConfig.Url), new AzureKeyCredential(aiConfig.Key!));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = aiConfig.Engine,
                Messages = {
                new ChatRequestSystemMessage("You are a helpful Elphant Insurance assistant. Your job is to help Elephant Agents with finding information in our company data. Answers should be short. Do not include references in your answers. You know that Elephant is the best insurance company in the world."),
                new ChatRequestUserMessage(question)
            },

                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions = {
                new AzureCognitiveSearchChatExtensionConfiguration() {
                SearchEndpoint = new Uri(aiSearchConfig.Url),
                IndexName = aiSearchConfig.Index,
                Key = aiSearchConfig.Key }
            }
                }
            };

            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            var message = response.Value.Choices[0].Message;
            answerResults.Answer = message.Content;

            foreach (var contextMessage in message.AzureExtensionsContext.Messages)
            {
                var result = JsonConvert.DeserializeObject<Answer>(contextMessage.Content);
                if (result != null)
                {
                    // Create a list just for all the urls
                    if (answerResults.Urls == null)
                    {
                        answerResults.Urls = new List<string>();
                    }
                    // Add each url from citations to a list
                    foreach (var citation in result.Citations)
                    {
                        answerResults.Urls.Add(citation.Url);
                    }
                    // Remove doublicates
                    answerResults.Urls = answerResults.Urls.Distinct().ToList();
                }
            }
            return answerResults;
        }


        public static async Task<AnswerResults> DoAsync2(AIConfig aiConfig, AISearchConfig aiSearchConfig, AIRequest aiRequest)
        {
            AnswerResults answerResults = new AnswerResults();
            var client = new OpenAIClient(new Uri(aiConfig.Url), new AzureKeyCredential(aiConfig.Key!));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = aiConfig.Engine,
                Messages = {
                new ChatRequestSystemMessage("You are a helpful assistant. You help with finding information in our company data.")
            },
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                {
                    new AzureCognitiveSearchChatExtensionConfiguration() {
                        SearchEndpoint = new Uri(aiSearchConfig.Url),
                        IndexName = aiSearchConfig.Index,
                        Key = aiSearchConfig.Key
                    }
                }
                }
            };

            foreach (var chat in aiRequest.Messages)
            {
                if (chat.User == true)
                {
                    chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(chat.Text));
                }
                else
                {
                    chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(chat.Text));
                }
            }

            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
            var message = response.Value.Choices[0].Message;
            answerResults.Answer = message.Content;

            foreach (var contextMessage in message.AzureExtensionsContext.Messages)
            {
                var result = JsonConvert.DeserializeObject<Answer>(contextMessage.Content);
                if (result != null)
                {
                    // Create a list just for all the urls
                    if (answerResults.Urls == null)
                    {
                        answerResults.Urls = new List<string>();
                    }
                    // Add each url from citations to a list
                    foreach (var citation in result.Citations)
                    {
                        answerResults.Urls.Add(citation.Url);
                    }
                    // Remove doublicates
                    answerResults.Urls = answerResults.Urls.Distinct().ToList();
                }
            }
            return answerResults;
        }
    }

    public class AIRequest
    {
        public int SearchIndex { get; set; }
        public int EngineIndex { get; set; }
        public List<Message> Messages { get; set; }
    }
    public class ChatHistory
    {
        public List<Message> Messages { get; set; }
    }

    public class Message
    {
        public string Text { get; set; }
        public bool User { get; set; }

        public List<string> Urls { get; set; }

    }

    public class AIConfig
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public string Engine { get; set; }
    }

    public class AISearchConfig
    {
        public string Url { get; set; }
        public string Key { get; set; }
        public string Index { get; set; }
    }

    public class AnswerResults
    {
        public string Answer { get; set; }
        public List<string> Urls { get; set; }

        public long EllapsedMilliSeconts { get; set; }
    }

    public class Citation
    {
        public string Content { get; set; }
        public object Id { get; set; }
        public string Title { get; set; }
        public object Filepath { get; set; }
        public string Url { get; set; }
        public Metadata Metadata { get; set; }
        public string Chunk_id { get; set; }
    }

    public class Metadata
    {
        public string Chunking { get; set; }
    }

    public class Answer
    {
        public List<Citation> Citations { get; set; }
        public string Intent { get; set; }

    }
}
