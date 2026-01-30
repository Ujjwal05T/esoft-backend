using ETNA.Api.Models.Entities;

namespace ETNA.Api.Data.Interfaces;

public interface IInquiryRepository
{
    // Create
    Task<int> CreateInquiryAsync(Inquiry inquiry);
    Task<int> CreateInquiryItemAsync(InquiryItem item);
    
    // Read
    Task<Inquiry?> GetInquiryByIdAsync(int id);
    Task<List<InquiryItem>> GetInquiryItemsByInquiryIdAsync(int inquiryId);
    Task<List<Inquiry>> GetInquiriesByVehicleIdAsync(int vehicleId);
    Task<List<Inquiry>> GetInquiriesByVehicleVisitIdAsync(int vehicleVisitId);
    Task<List<Inquiry>> GetInquiriesByWorkshopOwnerIdAsync(int workshopOwnerId);
    Task<List<Inquiry>> GetInquiriesByStatusAsync(int workshopOwnerId, InquiryStatus status);
    
    // Update
    Task<bool> UpdateInquiryStatusAsync(int id, InquiryStatus status);
    Task<bool> UpdateInquiryAsync(Inquiry inquiry);
    
    // Delete
    Task<bool> DeleteInquiryAsync(int id);
    
    // Utility
    Task<string> GenerateInquiryNumberAsync(int workshopOwnerId);
    Task<int> GetInquiryCountByWorkshopOwnerIdAsync(int workshopOwnerId);
}
