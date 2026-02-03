using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

public interface IQuoteRepository
{
    // Create
    Task<int> CreateQuoteAsync(Quote quote);
    Task<int> CreateQuoteItemAsync(QuoteItem item);

    // Read
    Task<Quote?> GetQuoteByIdAsync(int id);
    Task<List<Quote>> GetAllQuotesAsync();
    Task<List<Quote>> GetQuotesByInquiryIdAsync(int inquiryId);
    Task<List<Quote>> GetQuotesByVehicleIdAsync(int vehicleId);
    Task<List<Quote>> GetQuotesByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<List<QuoteItem>> GetQuoteItemsByQuoteIdAsync(int quoteId);

    // Update
    Task<bool> UpdateQuoteStatusAsync(int id, string status);

    // Delete
    Task<bool> DeleteQuoteAsync(int id);

    // Utility
    Task<string> GenerateQuoteNumberAsync(int workshopOwnerId);
}
