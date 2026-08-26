using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Interfaces;

public interface IGrainDetector
{
    List<GrainBox> Detect(byte[] imageBytes);
}