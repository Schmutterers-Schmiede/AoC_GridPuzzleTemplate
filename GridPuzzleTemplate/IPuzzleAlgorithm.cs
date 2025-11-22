using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace GridPuzzleTemplate
{
    public interface IPuzzleAlgorithm
    {
        void ExecuteStep();
        Image<Rgba32> GetImage();
        bool IsFinished();
        string GetResult();
    }
}
