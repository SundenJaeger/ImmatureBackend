using ImmatureBackend.Models;
using ImmatureBackend.Repositories.Interfaces;
using Supabase;

namespace ImmatureBackend.Repositories;

public class ReplicateRepository(Client supabaseClient) : IReplicateRepository
{
    public async Task<ReplicateEntity> CreateAsync(ReplicateEntity entity)
    {
        var response = await supabaseClient.From<ReplicateEntity>().Insert(entity);
        return response.Models.First();
    }

    public async Task<List<ReplicateEntity>> GetAllAsync()
    {
        var response = await supabaseClient
            .From<ReplicateEntity>()
            .Select(
                "id,technician_name,created_at,sample_id,ai_predicted_grains,confirmed_grains,immature_weight,percentage,grade,review_status")
            .Get();
        return response.Models.ToList();
    }

    public async Task<byte[]?> GetImageBytesAsync(Guid id)
    {
        var response = await supabaseClient
            .From<ReplicateEntity>()
            .Select("original_image")
            .Where(entity => entity.Id == id)
            .Get();

        return response.Models.FirstOrDefault()?.OriginalImage;
    }

    public async Task<string> UpdateStatusAsync(Guid id, string status)
    {
        var response = await supabaseClient
            .From<ReplicateEntity>()
            .Where(x => x.Id == id)
            .Set(x => x.ReviewStatus, status)
            .Update();

        return response.Models.First().ReviewStatus;
    }
}