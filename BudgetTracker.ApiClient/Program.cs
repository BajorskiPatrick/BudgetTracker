using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetTracker.Models; // Dostęp do klas modeli

namespace BudgetTracker.ApiClient;

public class Program
{
    private static readonly string BaseUrl = "http://localhost:5260";

    private static readonly string ApiUsername = "Nowy";
    private static readonly string ApiToken = "042f8395f9144504ae4440d405396f1a";

    private static HttpClient _client = new HttpClient();

    public static async Task Main(string[] args)
    {
        Console.WriteLine("BudgetTracker API Client Demo");
        ConfigureHttpClient();

        Console.WriteLine("\n--- Category API Demo ---");
        await DemoCategoryApi();
        
        Console.WriteLine("\n--- PaymentMethod API Demo ---");
        await DemoPaymentMethodApi();
        
        Console.WriteLine("\n--- Expense API Demo ---");
        await DemoExpenseApi();
        
        Console.WriteLine("\n--- Income API Demo ---");
        await DemoIncomeApi();
        
        Console.WriteLine("\n--- Limit API Demo ---");
        await DemoLimitApi();


        Console.WriteLine("\nDemo complete. Press any key to exit.");
        Console.ReadKey();
    }

    // Konfiguracja HttpClient z nagłówkami autoryzacyjnymi
    private static void ConfigureHttpClient()
    {
        _client.BaseAddress = new Uri(BaseUrl);
        // Dodajemy nagłówki autoryzacyjne, które nasz ApiAuthorizationFilter będzie czytał.
        _client.DefaultRequestHeaders.Add("X-Username", ApiUsername);
        _client.DefaultRequestHeaders.Add("X-Api-Token", ApiToken);
        // Akceptujemy odpowiedzi w formacie JSON
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    private static async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        Console.WriteLine($"GET: {requestUri}");
        return await _client.GetAsync(requestUri);
    }

    private static async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T data)
    {
        Console.WriteLine($"POST: {requestUri}");
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync(requestUri, content);
    }

    private static async Task<HttpResponseMessage> PutAsync<T>(string requestUri, T data)
    {
        Console.WriteLine($"PUT: {requestUri}");
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PutAsync(requestUri, content);
    }

    private static async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        Console.WriteLine($"DELETE: {requestUri}");
        return await _client.DeleteAsync(requestUri);
    }

    private static async Task PrintResponse(HttpResponseMessage response)
    {
        Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (JsonException)
                {
                    Console.WriteLine(content);
                }
            }
        }
        Console.WriteLine("------------------------------------------");
    }
    
    private static async Task DemoCategoryApi()
    {
        long newCategoryId = 0;
        
        // 1. Get all categories (should be empty initially or show existing)
        Console.WriteLine("Getting all categories:");
        var response = await GetAsync("/api/ApiCategory");
        await PrintResponse(response);

        // 2. Create a new category
        Console.WriteLine("Creating a new Expense category 'API Groceries':");
        var newCategory = new Category { Name = "API Groceries", Type = CategoryType.Expense, Description = "Groceries bought via API" };
        response = await PostAsync("/api/ApiCategory", newCategory);
        await PrintResponse(response);
        if (response.IsSuccessStatusCode)
        {
            var createdCategory = await response.Content.ReadFromJsonAsync<Category>();
            if (createdCategory != null) newCategoryId = createdCategory.CategoryId;
            Console.WriteLine($"Created Category ID: {newCategoryId}");
        }

        Console.WriteLine("Creating a new Income category 'API Salary':");
        var newCategoryIncome = new Category { Name = "API Salary", Type = CategoryType.Income, Description = "Salary received via API" };
        response = await PostAsync("/api/ApiCategory", newCategoryIncome);
        await PrintResponse(response);


        // 3. Get the created category by ID
        if (newCategoryId != 0)
        {
            Console.WriteLine($"Getting category with ID {newCategoryId}:");
            response = await GetAsync($"/api/ApiCategory/{newCategoryId}");
            await PrintResponse(response);
        }

        // 4. Update the category
        if (newCategoryId != 0)
        {
            Console.WriteLine($"Updating category with ID {newCategoryId} to 'API Groceries (Updated)':");
            var updatedCategory = new Category { CategoryId = newCategoryId, Name = "API Groceries (Updated)", Type = CategoryType.Expense, Description = "Updated description" };
            response = await PutAsync($"/api/ApiCategory/{newCategoryId}", updatedCategory);
            await PrintResponse(response);
        }

        // 5. Get all categories again to see the updated one
        Console.WriteLine("Getting all categories after update:");
        response = await GetAsync("/api/ApiCategory");
        await PrintResponse(response);
        
        // 6. Delete a category
        Console.WriteLine("Creating a temporary category for deletion:");
        var tempCategory = new Category { Name = "Temp Delete Category", Type = CategoryType.Expense, Description = "Will be deleted" };
        response = await PostAsync("/api/ApiCategory", tempCategory);
        long tempCatId = 0;
        if (response.IsSuccessStatusCode)
        {
            var createdTemp = await response.Content.ReadFromJsonAsync<Category>();
            if (createdTemp != null) tempCatId = createdTemp.CategoryId;
            Console.WriteLine($"Temporary Category ID: {tempCatId}");
        }

        if (tempCatId != 0)
        {
            Console.WriteLine($"Deleting temporary category with ID {tempCatId}:");
            response = await DeleteAsync($"/api/ApiCategory/{tempCatId}");
            await PrintResponse(response);
        }
    }
    
    private static async Task DemoPaymentMethodApi()
    {
        long newPmId = 0;

        Console.WriteLine("Getting all payment methods:");
        var response = await GetAsync("/api/ApiPaymentMethod");
        await PrintResponse(response);

        Console.WriteLine("Creating a new Payment Method 'API Credit Card':");
        var newPm = new PaymentMethod { Name = "API Credit Card" };
        response = await PostAsync("/api/ApiPaymentMethod", newPm);
        await PrintResponse(response);
        if (response.IsSuccessStatusCode)
        {
            var createdPm = await response.Content.ReadFromJsonAsync<PaymentMethod>();
            if (createdPm != null) newPmId = createdPm.PaymentMethodId;
            Console.WriteLine($"Created Payment Method ID: {newPmId}");
        }

        if (newPmId != 0)
        {
            Console.WriteLine($"Getting Payment Method with ID {newPmId}:");
            response = await GetAsync($"/api/ApiPaymentMethod/{newPmId}");
            await PrintResponse(response);
        }

        if (newPmId != 0)
        {
            Console.WriteLine($"Updating Payment Method with ID {newPmId} to 'API Debit Card':");
            var updatedPm = new PaymentMethod { PaymentMethodId = newPmId, Name = "API Debit Card" };
            response = await PutAsync($"/api/ApiPaymentMethod/{newPmId}", updatedPm);
            await PrintResponse(response);
        }

        Console.WriteLine("Getting all payment methods after update:");
        response = await GetAsync("/api/ApiPaymentMethod");
        await PrintResponse(response);

        Console.WriteLine("Creating a temporary payment method for deletion:");
        var tempPm = new PaymentMethod { Name = "Temp Delete PM" };
        response = await PostAsync("/api/ApiPaymentMethod", tempPm);
        long tempPmId = 0;
        if (response.IsSuccessStatusCode)
        {
            var createdTemp = await response.Content.ReadFromJsonAsync<PaymentMethod>();
            if (createdTemp != null) tempPmId = createdTemp.PaymentMethodId;
            Console.WriteLine($"Temporary Payment Method ID: {tempPmId}");
        }

        if (tempPmId != 0)
        {
            Console.WriteLine($"Deleting temporary payment method with ID {tempPmId}:");
            response = await DeleteAsync($"/api/ApiPaymentMethod/{tempPmId}");
            await PrintResponse(response);
        }
    }
    
    private static async Task DemoExpenseApi()
    {
        long categoryIdForExpense = 0;
        long paymentMethodIdForExpense = 0;

        // Fetch category ID
        var categoriesResponse = await GetAsync("/api/ApiCategory?typeFilter=Expense"); // Assuming filter works, otherwise fetch all and filter client-side
        if (categoriesResponse.IsSuccessStatusCode)
        {
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<Category>>();
            var targetCategory = categories?.FirstOrDefault(c => c.Name == "API Groceries (Updated)" || c.Name == "API Groceries");
            if (targetCategory != null)
            {
                categoryIdForExpense = targetCategory.CategoryId;
            }
            else
            {
                // Create if not found (for robustness in demo)
                Console.WriteLine("Creating default expense category for demo...");
                var newCat = new Category { Name = "Demo Expense Cat", Type = CategoryType.Expense, Description = "For API Expense Demo" };
                var createCatResponse = await PostAsync("/api/ApiCategory", newCat);
                if (createCatResponse.IsSuccessStatusCode)
                {
                    var createdCat = await createCatResponse.Content.ReadFromJsonAsync<Category>();
                    if (createdCat != null) categoryIdForExpense = createdCat.CategoryId;
                }
                else { await PrintResponse(createCatResponse); }
            }
        }
        else { await PrintResponse(categoriesResponse); }


        // Fetch payment method ID
        var pmsResponse = await GetAsync("/api/ApiPaymentMethod");
        if (pmsResponse.IsSuccessStatusCode)
        {
            var paymentMethods = await pmsResponse.Content.ReadFromJsonAsync<List<PaymentMethod>>();
            var targetPm = paymentMethods?.FirstOrDefault(pm => pm.Name == "API Debit Card" || pm.Name == "API Credit Card");
            if (targetPm != null)
            {
                paymentMethodIdForExpense = targetPm.PaymentMethodId;
            }
            else
            {
                // Create if not found
                Console.WriteLine("Creating default payment method for demo...");
                var newPm = new PaymentMethod { Name = "Demo PM" };
                var createPmResponse = await PostAsync("/api/ApiPaymentMethod", newPm);
                if (createPmResponse.IsSuccessStatusCode)
                {
                    var createdPm = await createPmResponse.Content.ReadFromJsonAsync<PaymentMethod>();
                    if (createdPm != null) paymentMethodIdForExpense = createdPm.PaymentMethodId;
                }
                else { await PrintResponse(createPmResponse); }
            }
        }
        else { await PrintResponse(pmsResponse); }


        if (categoryIdForExpense == 0 || paymentMethodIdForExpense == 0)
        {
            Console.WriteLine("Failed to setup prerequisites (category/payment method) for Expense demo. Skipping.");
            return;
        }

        long newExpenseId = 0;

        Console.WriteLine("\nGetting all expenses:");
        var response = await GetAsync("/api/ApiExpense");
        await PrintResponse(response);

        Console.WriteLine("Creating a new Expense 'API Lunch':");
        var newExpense = new Expense
        {
            Amount = 15.75M,
            TransactionDate = DateTime.Now,
            Description = "Lunch with colleagues",
            CategoryId = categoryIdForExpense,
            PaymentMethodId = paymentMethodIdForExpense,
            Payee = "Cafe Demo"
        };
        response = await PostAsync("/api/ApiExpense", newExpense);
        await PrintResponse(response);
        if (response.IsSuccessStatusCode)
        {
            var createdExpense = await response.Content.ReadFromJsonAsync<Expense>();
            if (createdExpense != null) newExpenseId = createdExpense.ExpenseId;
            Console.WriteLine($"Created Expense ID: {newExpenseId}");
        }

        if (newExpenseId != 0)
        {
            Console.WriteLine($"Getting Expense with ID {newExpenseId}:");
            response = await GetAsync($"/api/ApiExpense/{newExpenseId}");
            await PrintResponse(response);
        }

        if (newExpenseId != 0)
        {
            Console.WriteLine($"Updating Expense with ID {newExpenseId} to Amount 20.00:");
            var updatedExpense = new Expense
            {
                ExpenseId = newExpenseId,
                Amount = 20.00M,
                TransactionDate = newExpense.TransactionDate,
                Description = "Updated lunch expense",
                CategoryId = categoryIdForExpense,
                PaymentMethodId = paymentMethodIdForExpense,
                Payee = "Updated Cafe"
            };
            response = await PutAsync($"/api/ApiExpense/{newExpenseId}", updatedExpense);
            await PrintResponse(response);
        }

        Console.WriteLine("Getting all expenses after update:");
        response = await GetAsync("/api/ApiExpense");
        await PrintResponse(response);

        if (newExpenseId != 0)
        {
            Console.WriteLine($"Deleting Expense with ID {newExpenseId}:");
            response = await DeleteAsync($"/api/ApiExpense/{newExpenseId}");
            await PrintResponse(response);
        }
    }
    
    private static async Task DemoIncomeApi()
    {
        // First, ensure we have an income category
        long categoryIdForIncome = 0;

        // Fetch income category ID
        var categoriesResponse = await GetAsync("/api/ApiCategory?typeFilter=Income"); 
        if (categoriesResponse.IsSuccessStatusCode)
        {
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<Category>>();
            var targetCategory = categories?.FirstOrDefault(c => c.Name == "API Salary");
            if (targetCategory != null)
            {
                categoryIdForIncome = targetCategory.CategoryId;
            }
            else
            {
                Console.WriteLine("Creating default income category for demo...");
                var newCat = new Category { Name = "Demo Income Cat", Type = CategoryType.Income, Description = "For API Income Demo" };
                var createCatResponse = await PostAsync("/api/ApiCategory", newCat);
                if (createCatResponse.IsSuccessStatusCode)
                {
                    var createdCat = await createCatResponse.Content.ReadFromJsonAsync<Category>();
                    if (createdCat != null) categoryIdForIncome = createdCat.CategoryId;
                }
                else { await PrintResponse(createCatResponse); }
            }
        }
        else { await PrintResponse(categoriesResponse); }

        if (categoryIdForIncome == 0)
        {
            Console.WriteLine("Failed to setup prerequisites (income category) for Income demo. Skipping.");
            return;
        }

        long newIncomeId = 0;

        Console.WriteLine("\nGetting all incomes:");
        var response = await GetAsync("/api/ApiIncome");
        await PrintResponse(response);

        Console.WriteLine("Creating a new Income 'API Bonus':");
        var newIncome = new Income
        {
            Amount = 100.00M,
            TransactionDate = DateTime.Now,
            Description = "Yearly bonus",
            CategoryId = categoryIdForIncome,
            Source = "Employer"
        };
        response = await PostAsync("/api/ApiIncome", newIncome);
        await PrintResponse(response);
        if (response.IsSuccessStatusCode)
        {
            var createdIncome = await response.Content.ReadFromJsonAsync<Income>();
            if (createdIncome != null) newIncomeId = createdIncome.IncomeId;
            Console.WriteLine($"Created Income ID: {newIncomeId}");
        }

        if (newIncomeId != 0)
        {
            Console.WriteLine($"Getting Income with ID {newIncomeId}:");
            response = await GetAsync($"/api/ApiIncome/{newIncomeId}");
            await PrintResponse(response);
        }

        if (newIncomeId != 0)
        {
            Console.WriteLine($"Updating Income with ID {newIncomeId} to Amount 150.00:");
            var updatedIncome = new Income
            {
                IncomeId = newIncomeId,
                Amount = 150.00M,
                TransactionDate = newIncome.TransactionDate,
                Description = "Updated yearly bonus",
                CategoryId = categoryIdForIncome,
                Source = "Updated Employer"
            };
            response = await PutAsync($"/api/ApiIncome/{newIncomeId}", updatedIncome);
            await PrintResponse(response);
        }

        Console.WriteLine("Getting all incomes after update:");
        response = await GetAsync("/api/ApiIncome");
        await PrintResponse(response);

        if (newIncomeId != 0)
        {
            Console.WriteLine($"Deleting Income with ID {newIncomeId}:");
            response = await DeleteAsync($"/api/ApiIncome/{newIncomeId}");
            await PrintResponse(response);
        }
    }
    
    private static async Task DemoLimitApi()
    {
        // Need an expense category to link to a limit
        long categoryIdForLimit = 0;

        var categoriesResponse = await GetAsync("/api/ApiCategory?typeFilter=Expense");
        if (categoriesResponse.IsSuccessStatusCode)
        {
            var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<Category>>();
            var targetCategory = categories?.FirstOrDefault(c => c.Name == "API Groceries (Updated)" || c.Name == "Demo Expense Cat");
            if (targetCategory != null)
            {
                categoryIdForLimit = targetCategory.CategoryId;
            }
            else
            {
                Console.WriteLine("Creating default expense category for Limit demo...");
                var newCat = new Category { Name = "Demo Limit Cat", Type = CategoryType.Expense, Description = "For API Limit Demo" };
                var createCatResponse = await PostAsync("/api/ApiCategory", newCat);
                if (createCatResponse.IsSuccessStatusCode)
                {
                    var createdCat = await createCatResponse.Content.ReadFromJsonAsync<Category>();
                    if (createdCat != null) categoryIdForLimit = createdCat.CategoryId;
                }
                else { await PrintResponse(createCatResponse); }
            }
        }
        else { await PrintResponse(categoriesResponse); }

        if (categoryIdForLimit == 0)
        {
            Console.WriteLine("Failed to setup prerequisites (expense category) for Limit demo. Skipping.");
            return;
        }

        long newLimitId = 0;

        Console.WriteLine("\nGetting all limits:");
        var response = await GetAsync("/api/ApiLimit");
        await PrintResponse(response);

        Console.WriteLine("Creating a new Limit for 'API Groceries' category:");
        var newLimit = new Limit
        {
            Amount = 500.00M,
            CategoryId = categoryIdForLimit
        };
        response = await PostAsync("/api/ApiLimit", newLimit);
        await PrintResponse(response);
        if (response.IsSuccessStatusCode)
        {
            var createdLimit = await response.Content.ReadFromJsonAsync<Limit>();
            if (createdLimit != null) newLimitId = createdLimit.BudgetId;
            Console.WriteLine($"Created Limit ID: {newLimitId}");
        }
        else if ((int)response.StatusCode == 400 && (await response.Content.ReadAsStringAsync()).Contains("A limit for this category already exists"))
        {
            Console.WriteLine("Limit for this category already exists, trying to fetch it...");
            var existingLimits = await GetAsync("/api/ApiLimit");
            if (existingLimits.IsSuccessStatusCode)
            {
                var limitsList = await existingLimits.Content.ReadFromJsonAsync<List<Limit>>();
                newLimitId = limitsList?.FirstOrDefault(l => l.CategoryId == categoryIdForLimit)?.BudgetId ?? 0;
                Console.WriteLine($"Found existing Limit ID: {newLimitId}");
            }
        }


        if (newLimitId != 0)
        {
            Console.WriteLine($"Getting Limit with ID {newLimitId}:");
            response = await GetAsync($"/api/ApiLimit/{newLimitId}");
            await PrintResponse(response);
        }

        if (newLimitId != 0)
        {
            Console.WriteLine($"Updating Limit with ID {newLimitId} to Amount 600.00:");
            var updatedLimit = new Limit
            {
                BudgetId = newLimitId,
                Amount = 600.00M,
                CategoryId = categoryIdForLimit // CategoryId must remain the same or change to another unique one
            };
            response = await PutAsync($"/api/ApiLimit/{newLimitId}", updatedLimit);
            await PrintResponse(response);
        }

        Console.WriteLine("Getting all limits after update:");
        response = await GetAsync("/api/ApiLimit");
        await PrintResponse(response);

        if (newLimitId != 0)
        {
            Console.WriteLine($"Deleting Limit with ID {newLimitId}:");
            response = await DeleteAsync($"/api/ApiLimit/{newLimitId}");
            await PrintResponse(response);
        }
    }
}