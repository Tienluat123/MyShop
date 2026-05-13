using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MyShop.Models;
using MyShop.Services;
using MyShop.Data;
using System.Linq;
using System.IO;

namespace MyShop.Services;

public interface IAiAssistantService
{
    Task<string> GetAiResponse(string userMessage, List<ChatMessage> history, string mode);
}

public class AiAssistantService : IAiAssistantService
{
    private string _apiKey = ""; // Will be loaded from file
    private readonly string _baseUrl = "https://api.groq.com/openai/v1/chat/completions";
    private readonly HttpClient _httpClient;
    
    private readonly IReportService _reportService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IPromotionService _promotionService;

    public AiAssistantService(
        IReportService reportService,
        IProductService productService,
        ICustomerService customerService,
        IPromotionService promotionService)
    {
        LoadApiKey();
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        
        _reportService = reportService;
        _productService = productService;
        _customerService = customerService;
        _promotionService = promotionService;
    }

    private void LoadApiKey()
    {
        try
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SecretConfig.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                _apiKey = doc.RootElement.GetProperty("GroqApiKey").GetString() ?? "";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading API Key: {ex.Message}");
        }
    }

    public async Task<string> GetAiResponse(string userMessage, List<ChatMessage> history, string mode)
    {
        try
        {
            if (string.IsNullOrEmpty(_apiKey)) return "API Key missing. Please check SecretConfig.json.";

            var shopContext = await GetOptimizedShopContext();

            var messages = new List<object>
            {
                new { role = "system", content = GetSystemPrompt(shopContext, mode) }
            };

            foreach (var msg in history.TakeLast(6))
            {
                messages.Add(new { role = msg.IsUser ? "user" : "assistant", content = msg.Content });
            }

            messages.Add(new { role = "user", content = userMessage });

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = messages,
                temperature = 0.6,
                max_tokens = 1000
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl, content);
            
            if (!response.IsSuccessStatusCode) return "AI Connection error. Please try again.";

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
        catch (Exception ex)
        {
            return $"System error: {ex.Message}";
        }
    }

    private async Task<string> GetOptimizedShopContext()
    {
        var now = DateTime.Now;
        var start = now.AddDays(-30);
        
        var report = await _reportService.GetReportDataAsync(start, now, "Month");
        var customersResult = await _customerService.GetAll(new PagingRequest { PageSize = 1 });
        var totalCustomers = customersResult.Pagination.TotalItems;
        
        var allProducts = await _productService.GetAll(new PagingRequest { PageSize = 15 });

        var sb = new StringBuilder();
        sb.AppendLine("### BUSINESS REPORT (LAST 30 DAYS) ###");
        sb.AppendLine($"- Revenue: {report.Summary.TotalRevenue:N0} VND");
        sb.AppendLine($"- Profit: {report.Summary.TotalProfit:N0} VND");
        sb.AppendLine($"- Total Orders: {report.Summary.OrdersCount}");
        sb.AppendLine($"- Total Customers: {totalCustomers}");
        
        sb.AppendLine("\n### PRODUCT CATALOG & STOCK (Top 15) ###");
        foreach (var p in allProducts.Items)
        {
            string stockStatus = p.StockQuantity <= 0 ? "OUT OF STOCK" : $"Stock: {p.StockQuantity}";
            sb.AppendLine($"- {p.Name} | Price: {p.Price:N0} VND | Status: {stockStatus}");
        }

        return sb.ToString();
    }

    private string GetSystemPrompt(string context, string mode)
    {
        bool isPromotionMode = mode.Equals("promotion", StringComparison.OrdinalIgnoreCase);
        var taskInstructions = isPromotionMode
            ? @"TASK: PROMOTION CODE CREATION
You must analyze the request and propose exactly one promotion code.

PROMO CODE RULES (MANDATORY):
To enable the code generation system, you MUST include the following line in your response:
CODE=PROMO_CODE, VALUE=AMOUNT, TYPE=TYPE, REASON=SHORT_REASON

IMPORTANT NOTES:
1. Code Name (CODE): Must be UPPERCASE, use underscores (_) instead of hyphens (-). Example: CLEARANCE_SALE, VIP_GIFT. NEVER use IDs.
2. Value (VALUE): Numbers only. Example: 10, 20.
3. Type (TYPE): Use 'Percent' or 'FixedAmount'.
4. Keep the explanation short and make the proposed code easy to review.

Example:
Based on the data, Salted Coffee has high inventory, so I propose:
CODE=SALTED_COFFEE_SALE, VALUE=20, TYPE=Percent, REASON=Clearance sale"
            : @"TASK: BUSINESS Q&A AND DATA ANALYSIS
Answer questions, explain trends, and give operational advice only.
Do NOT propose or create promotion codes in this mode.
Do NOT include CODE=, VALUE=, or TYPE= lines.
If the user wants to create a promotion, tell them to switch to Request Promotion Code mode.";

        return $@"You are a Senior Business Strategy and Marketing Expert for MyShop.
Shop Data:
{context}

WORKING STYLE:
- Sharp analysis based on data (Revenue, Inventory).
- Always provide high-level strategic business advice.
- If a product has high stock, suggest a clearance discount.
- If a product is out of stock, remind the shop owner to restock.
- ALWAYS RESPOND IN ENGLISH.

{taskInstructions}

FORMAT: Use plain text only, no Markdown.";
    }
}
