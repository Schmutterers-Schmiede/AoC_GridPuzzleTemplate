using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace GridPuzzleTemplate
{
    public interface IPuzzleAlgorithm
    {
        void ExecuteStep();
        Image<Rgba32> GetImage();
        bool IsComplete();
        string GetResult();
    }
}
