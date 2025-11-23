using System.ComponentModel;
using System.Text.Json;

namespace CourseAI.Services
{
    public class WebSearchService
    {
        private readonly HttpClient _httpClient;
        private const string TAVILY_SEARCH_ENDPOINT = "https://api.tavily.com/search";
        private const string TAVILY_API_KEY_CONFIG = "TavilyApiKey";
        private readonly string? _apiKey;

        public WebSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration[TAVILY_API_KEY_CONFIG];
        }

        [Description("Search the web for current information about learning topics, best practices, tutorials, and resources")]
        public async Task<string> SearchWeb(
            [Description("The search query to find relevant information")]
            string query,
            [Description("Number of results to return (default: 10)")]
            int count = 10)
        {
            try
            {
                // If no API key is configured, return a helpful message
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return $"Web search for '{query}' - API not configured. Recommend checking recent tutorials, documentation, and community resources for this topic.";
                }

                var request = new TavilySearchRequest
                {
                    ApiKey = _apiKey,
                    Query = query,
                    MaxResults = count,
                    SearchDepth = "basic",
                    IncludeAnswer = false,
                    IncludeImages = false,
                    IncludeDomains = null,
                    ExcludeDomains = null
                };

                var jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var response = await _httpClient.PostAsync(TAVILY_SEARCH_ENDPOINT, 
                    new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    return $"Unable to search web for '{query}' - search service unavailable.";
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var searchResults = JsonSerializer.Deserialize<TavilySearchResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (searchResults?.Results == null || !searchResults.Results.Any())
                {
                    return $"No web results found for '{query}'.";
                }

                var results = searchResults.Results.Take(count).Select(result => 
                    $"**{result.Title}**\n{result.Content}\nURL: {result.Url}\n").ToList();

                return $"Web search results for '{query}':\n\n" + string.Join("\n", results);
            }
            catch (Exception ex)
            {
                return $"Error searching web for '{query}': {ex.Message}";
            }
        }

        [Description("Search for educational content and tutorials on specific learning topics")]
        public async Task<string> SearchEducationalContent(
            [Description("The learning topic to search for educational content")]
            string topic,
            [Description("Type of content to prioritize (tutorial, course, documentation, etc.)")]
            string contentType = "tutorial")
        {
            var educationalQuery = $"{topic} {contentType} beginner guide learn";
            return await SearchWeb(educationalQuery, 8);
        }

        [Description("Search for current industry best practices and trends")]
        public async Task<string> SearchBestPractices(
            [Description("The technology or skill area to search best practices for")]
            string skillArea,
            [Description("Year to focus on (current year by default)")]
            int year = 2024)
        {
            var bestPracticesQuery = $"{skillArea} best practices {year} industry standards";
            return await SearchWeb(bestPracticesQuery, 6);
        }
    }

    // DTOs for Tavily Search API
    public class TavilySearchRequest
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public int MaxResults { get; set; } = 10;
        public string SearchDepth { get; set; } = "basic"; // "basic" or "advanced"
        public bool IncludeAnswer { get; set; } = false;
        public bool IncludeImages { get; set; } = false;
        public string[]? IncludeDomains { get; set; }
        public string[]? ExcludeDomains { get; set; }
    }

    public class TavilySearchResponse
    {
        public string Query { get; set; } = string.Empty;
        public List<TavilyResult> Results { get; set; } = new();
        public string? Answer { get; set; }
        public double ResponseTime { get; set; }
    }

    public class TavilyResult
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}