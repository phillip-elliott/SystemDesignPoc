using System.Net.Http.Json;
using Shared.Contracts;
using Shared.Contracts.Models;

namespace BlazorUi.Services;

public class ProductApiClient(HttpClient http)
{
    public async Task<List<ProductReadModel>> GetProductsAsync(CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<ProductReadModel>>("/api/read/api/products", ct) 
            ?? [];
    }
}

public class ProductCommandClient(HttpClient http)
{
    public async Task<bool> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        using var response = await http.PostAsJsonAsync("/api/write/api/products", request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"/api/write/api/products/{id}", ct);
        return response.IsSuccessStatusCode;
    }
}