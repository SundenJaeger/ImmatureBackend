using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Services;

public class PlaceholderDetector : IGrainDetector
{
    private const double ConfidenceThreshold = 0.5;

    public PredictResponse Detect(byte[] imageBytes)
    {
        var gb = new List<GrainBox>
        {
            new()
            {
                X = 100,
                Y = 200,
                Width = 24,
                Height = 24,
                Confidence = 0.7,
                Action = null
            }
        };

        var filteredGrains = gb
            .Where(box => box.Confidence is >= ConfidenceThreshold)
            .ToList();

        return new PredictResponse
        {
            Grains = filteredGrains,
            ImageId = Guid.NewGuid().ToString()
        };
    }
}