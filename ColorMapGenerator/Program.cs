using Spectre.Console;
using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using TextCopy;

using IntVector3 = (int R, int G, int B);

namespace ColorMapGenerator;
internal class Program
{
    private static Dictionary<char, IntVector3> colorMap;
    private static string emptyMap = "";
    private static char[] chars;
    private static string rawMapString;
    private static string styledMapString;

    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            PrintHeader();
            var mode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Pick color generation mode:")
                    .PageSize(5)           // how many items to show at once
                    .AddChoices(new[] {
                        "empty map", "random colors", "gradient black to green", "gradient green to black"
                    }));

            chars = File
                .ReadAllText("input.txt")
                .Distinct()
                .Where(character => character != '\n' && character != '\r')
                .OrderBy(characeter => characeter)
                .ToArray();

            GenerateColorMap(mode);
            rawMapString = CreateRawMapString();
            PrintStyledMapString(); 

            if(mode == "empty map")
                emptyMap = "";
            else
                colorMap.Clear();

            Console.WriteLine();
            ClipboardService.SetText(rawMapString);
            AnsiConsole.MarkupLine("[green]The code has been copied to the clipboard[/]");
            Console.WriteLine();
            AnsiConsole.WriteLine("Press any key to generate a new color map...");
            Console.ReadLine();
        }
    }

    static void PrintHeader()
    {
        Console.WriteLine("+--------------------------------------------------+");
        Console.WriteLine("| Color Map Generator For AOC Grid Puzzle Template | ");
        Console.WriteLine("+--------------------------------------------------+");
        Console.WriteLine();
    }
    
    static string CreateRawMapString()
    {
        if (emptyMap.Length > 0)
            return emptyMap;
        else
        {
            StringBuilder sb = new();
            IntVector3 rgb;
            Color color;
            Style style;
            for (int i = 0; i < chars.Length; i++)
            {
                rgb = colorMap[chars[i]];
                color = new Color((byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
                style = new Style(background: color);
                sb.Append($"{{'{chars[i]}', new Rgba({rgb.R},{rgb.G},{rgb.B})}}");

                if (i < chars.Length - 1)
                    sb.Append(',');

                sb.Append('\n');
            }
            return sb.ToString();
        }
    }

    static void PrintStyledMapString()
    {
        if(emptyMap.Length > 0)
            Console.WriteLine(emptyMap);
        else
        {
            IntVector3 rgb;
            Color color;
            Style style;
            for (int i = 0; i < chars.Length; i++)
            {
                rgb = colorMap[chars[i]];
                color = new Color((byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
                style = new Style(background: color);
                AnsiConsole.Write(new Text($"{{'{chars[i]}', new Rgba({rgb.R},{rgb.G},{rgb.B})}}", style));

                if (i < chars.Length - 1)
                    AnsiConsole.Write(',');

                AnsiConsole.WriteLine();
            }
        }
    }

    private static void GenerateColorMap(string mode)
    {
        switch (mode)
        {
            case "empty map":
                emptyMap = GenerateEmptyMap();
                break;
            case "random colors":
                colorMap = GenerateRandomValues();
                break;
            case "black to green":
                colorMap = GenerateGreenGradient(true);
                break;
            case "green to black":
                colorMap = GenerateGreenGradient(false);
                break;
        }
    }

    private static Dictionary<char, IntVector3> GenerateRandomValues()
    {
        var values = new Dictionary<char, IntVector3>();
        var redValues = Enumerable.Range(0, 256).ToList();
        var greenValues = Enumerable.Range(0, 256).ToList();
        var blueValues = Enumerable.Range(0, 256).ToList();

        int red, green, blue, redIndex, greenIndex, blueIndex;

        for (int i = 0; i < chars.Length; i++)
        {
            redIndex = RandomNumberGenerator.GetInt32(0, redValues.Count - 1);
            red = redValues[redIndex];
            redValues.RemoveAt(redIndex);

            greenIndex = RandomNumberGenerator.GetInt32(0, greenValues.Count - 1);
            green = greenValues[greenIndex];
            greenValues.RemoveAt(greenIndex);

            blueIndex = RandomNumberGenerator.GetInt32(0, blueValues.Count - 1);
            blue = blueValues[blueIndex];
            blueValues.RemoveAt(blueIndex);

            values.Add(chars[i], (red, green, blue));
        }
        return values;
    }

    private static Dictionary<char, IntVector3> GenerateGreenGradient(bool ascending)
    {
        Dictionary<char, IntVector3> gradient = [];

        if (chars.Length == 1)
        {
            gradient.Add(chars[0], new(0,255,0));
            return gradient;
        }

        float valueIncrement = 255 / (chars.Length - 1);
        float greenValue;

        

        if (ascending)
            greenValue = 0f;
        else
        {
            greenValue = 255f;
            valueIncrement *= -1;
        }

        for (int i = 0; i < chars.Length; i++)
        {
            gradient.Add(chars[i], (0,Convert.ToInt32(greenValue),0));
            greenValue += valueIncrement;
        }
        return gradient;
    }

    private static string GenerateEmptyMap()
    {
        var values = new StringBuilder();
        
        for (int i = 0; i < chars.Length; i++)
        {
            values.Append($"{{'{chars[i]}', new Rgba(,,)}}");

            if (i < chars.Length - 1)
            {
                values.Append(',');
                values.Append('\n');
            }
        }
        return values.ToString();
    }

    static string RgbToHex(IntVector3 rgb) => $"#{rgb.R:X2}{rgb.G:X2}{rgb.B:X2}";
}
