# AOC_GridPuzzleTemplate
I found that rendering 2D grids for Advent of Code on the C# console does not work well for larger inputs, especially when rendering frames in quick succession, so I developed this visualizer template for 2D grid puzzles using imGUI for the UI and Silk.Net, which uses OpenGL under the hood for rendering the grid. I developed this tool with cross platform compatibility in mind, but I never tested it on Linux, so I can't guarantee anything.

## How to use
I designed this template to be as easy to use as I could. If you follow these steps and pay attention to the comments in the code, you should have no problems.

### Copying the template
To use this template in your own solution, you can just copy the GridPuzzleTemplate folder to your own solution folder and add the GridPuzzleTemplate.csproj file as an existing project in Visual Studio.

### UI Scaling
I developed this template on a 4k monitor, so unless you also have a 4k monitor, the UI scaling will probably be off on your side. You can adjust this in the program.cs file of the GridPuzzleTemplate project.
<img width="916" height="211" alt="image" src="https://github.com/user-attachments/assets/15d67efa-30e1-4ca4-a435-246320f4d7e0" />

### Creating a new Puzzle class
You can create any number of puzzle classes and they will show up in the dropdown menu in the UI. For this to work it is important that your puzzle class implements the IPuzzleAlgorithm interface.

### Initialization
All your initialization should happen inside the constructor of you puzzle class.
<img width="984" height="827" alt="image" src="https://github.com/user-attachments/assets/89f01da6-8594-4800-9cd2-8f1f34577dab" />



### Color mapping
In the constructor you also need to map a color to each distinct character in your input. If your input has many different characters, it can become tedious to map each individual character by hand. This is why I wrote the ColorMapGenerator. This is a small console application that generates the color map for you. Just paste your input into the color map generator's input file, run the program and select what kind of map you want. There are four options:
- **empty map:** generates a map with all the characters and empty color values for you to fill in.
- **random colors:** generates a map with random color values assigned to the characters
- **gradient black to green:** generates a map with incremental green values assigned to the characters (useful for height maps)
- **gradient green to black:** same as black to green, but in descending order

<img width="962" height="152" alt="image" src="https://github.com/user-attachments/assets/fed0ebf2-50ac-48b9-9962-9ff57acb4bfd" />

### Implementing an algorithm
There is no main loop here. Instead you need to implement the ExecuteStep method, which performs a single step of your algorithm and you need to save your state in properties of the puzzle class. This is because the Program takes care of timing frames when you let the algorithm run freely and it needs to trigger each individual step step at the right time.

<img width="432" height="495" alt="image" src="https://github.com/user-attachments/assets/ad190a7c-3905-4a8a-99d1-d6953ac00b2f" />


### Setting the result
The program will check if your algorithm is finished each step using the isFinished method and if it is, it will query your the result using the GetResult method. When your algorithm is finished, you need to set the isFinished property, convert the result to a string and assign that string to the result property.

<img width="247" height="132" alt="image" src="https://github.com/user-attachments/assets/214b6fa7-ae36-4c42-91d6-ecd98cdff10c" />

## Download
latest version (2.1): https://github.com/Schmutterers Schmiede/AoC_GridPuzzleTemplate/archive/refs/heads/V2.1.zip
## Planned features
