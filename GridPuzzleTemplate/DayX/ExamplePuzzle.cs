using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

// This is where you implement your algorithm. 

namespace GridPuzzleTemplate.DayX;

internal struct SPoint
{
    public int X;
    public int Y;
}

internal class ExamplePuzzle : IPuzzleAlgorithm
{
    private Image<Rgba32> grid;
    private int width;
    private int height;
    private SPoint start;
    private SPoint pos;
    SPoint scout;
    private bool isComplete;
    string result = "";
    Dictionary<char, Color> colorMap;

    private SPoint north = new() { X = 0, Y = -1 };
    private SPoint south = new() { X = 0, Y = 1 };
    private SPoint east = new() { X = 1, Y = 0 };
    private SPoint west = new() { X = -1, Y = 0 };

    private SPoint direction;

    // The constructor is used for initializations
    public ExamplePuzzle()
    {
        // Do not change this section except for the input file path
        var input = File.ReadAllLines("DayX/input.txt"); 
        width = input[0].Length;
        height = input.Length;
        grid = new Image<Rgba32>(width, height);

        // This is where you define custom colors for the distinct characters in your puzzle input
        colorMap = new()
        {
            {'.', new Rgba32(0,0,0)},
            {'x', new Rgba32(255,0,0)}
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x,y] = colorMap[input[y][x]];
            }
        }
        start = new SPoint() { X = 0, Y = 0 };
        pos = start;
        isComplete = false;
        direction = east;
    }

    // ========== INTERFACE METHODS ==========

    public void ExecuteStep()
    {
        scout = pos;
        grid[pos.X, pos.Y] = colorMap['.'];
        scout.X += direction.X;
        scout.Y += direction.Y;

        if (IsOutOfBounds(scout))
            ChangeDirection();

        pos.X += direction.X;
        pos.Y += direction.Y;
        grid[pos.X, pos.Y] = colorMap['x'];

        if (pos.Equals(start))
        {
            isComplete = true;
            result = "300";
        }
    }

    public Image<Rgba32> GetImage() => grid;

    public string GetResult()
    {
        if (result.Length == 0)
            throw new Exception("The result was queried prematurely");

        return result;
    }

    public bool IsComplete() => isComplete;

    // ============= HELPER METHODS =============

    void ChangeDirection()
    {
        if (direction.Equals(east))
            direction = south;
        else if (direction.Equals(south))
            direction = west;
        else if (direction.Equals(west))
            direction = north;
        else if (direction.Equals(north))
            direction = east;
    }

    bool IsOutOfBounds(SPoint pos)
    =>  pos.X < 0 || pos.X >= width ||
        pos.Y < 0 || pos.Y >= height;

}
