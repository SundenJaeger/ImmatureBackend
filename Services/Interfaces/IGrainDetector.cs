using ImmatureBackend.DTOs;

namespace ImmatureBackend.Services.Interfaces;

public interface IGrainDetector
{
    List<GrainBox> Detect(byte[] imageBytes);
}