using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Interfaces;

public interface IGrainDetector
{
    PredictResponse Detect(byte[] imageBytes);
}