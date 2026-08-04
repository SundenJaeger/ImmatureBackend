using ImmatureBackend.DTOs;
using ImmatureBackend.Services.Interfaces;

namespace ImmatureBackend.Services;

public class PlaceholderDetector : IGrainDetector
{
    public List<GrainBox> Detect(byte[] imageBytes)
    {
        return new List<GrainBox>
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
    }
}