namespace GridPuzzleTemplate;

using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;
using ImGuiNET;
using System.Reflection;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

internal class Program
{
    private static GL gl;
    private static IInputContext inputContext;
    private static ImGuiController controller;
    private static IPuzzleAlgorithm algorithm;

    private static Dictionary<string, Type> availableAlgorithms = new();
    private static string selectedAlgorithm = "";

    private static uint texture;
    private static Image<Rgba32> image;
    private static bool isRunning = false;
    private static bool isFinished = false;
    private static bool runInstantly = false;
    private static string result = "";

    private static float defaultRenderScale = 18f;
    private static float maxRenderScale = 50f;
    private static readonly Silk.NET.Maths.Vector2D<int> minSize = new(3000, 2000);


    private static TimeSpan stepInterval = TimeSpan.FromMilliseconds(100);
    private static DateTime lastStepTime = DateTime.MinValue;

    private static ManualResetEventSlim signal = new ManualResetEventSlim(false);
    private static CancellationTokenSource cts = new CancellationTokenSource();

    public unsafe static void Main(string[] args)
    {
        var windowOptions = WindowOptions.Default;
        windowOptions.Size = minSize;
        using var window = Window.Create(windowOptions);

        window.Load += () =>
        {
            gl = window.CreateOpenGL();
            inputContext = window.CreateInput();
            controller = new ImGuiController(gl, window, inputContext);

            ImGui.GetIO().FontGlobalScale = 2.4f; // GUI scaling

            DiscoverAlgorithms();
            if (availableAlgorithms.Count > 0)
            {
                selectedAlgorithm = availableAlgorithms.Keys.First();
                algorithm = LoadAlgorithm(selectedAlgorithm);

            }

            image = algorithm.GetImage();
            UpdateTexture();

            texture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, texture);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)image.Width, (uint)image.Height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

            UpdateTexture();

            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Task.Factory.StartNew(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (runInstantly && !isFinished)
                    {
                        while (!algorithm.IsFinished())
                        {
                            algorithm.ExecuteStep();
                        }
                        image = algorithm.GetImage();
                        isFinished = true;
                        result = algorithm.GetResult();
                        runInstantly = false; 
                        signal.Set();
                    }
                    else if (isRunning && !isFinished)
                    {
                        if (DateTime.Now - lastStepTime >= stepInterval)
                        {
                            algorithm.ExecuteStep();
                            image = algorithm.GetImage();
                            isFinished = algorithm.IsFinished();
                            if (isFinished)
                                result = algorithm.GetResult();
                            lastStepTime = DateTime.Now;
                            signal.Set();
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }, TaskCreationOptions.LongRunning);

        };

        window.Render += delta =>
        {
            controller.Update((float)delta);

            gl.ClearColor(System.Drawing.Color.FromArgb(30, 30, 30));
            gl.Clear((uint)ClearBufferMask.ColorBufferBit);

            if (signal.Wait(1))
            {
                UpdateTexture();
                signal.Reset();
            }

            ImGui.Begin("Controls");


            if (isRunning) ImGui.BeginDisabled();
            if (ImGui.BeginCombo("Algorithm", selectedAlgorithm))
            {
                foreach (var key in availableAlgorithms.Keys)
                {
                    bool isSelected = selectedAlgorithm == key;
                    if (ImGui.Selectable(key, isSelected))
                    {
                        selectedAlgorithm = key;
                        algorithm = LoadAlgorithm(selectedAlgorithm);
                        image = algorithm.GetImage();
                        isFinished = false;
                        result = "";
                        isRunning = false;
                        ReinitializeTexture();
                    }
                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (isRunning) ImGui.EndDisabled();


            ImGui.Spacing();
            if (isFinished) ImGui.BeginDisabled();

            if (ImGui.Button(isRunning ? "Pause" : "Start"))
            {
                if (!isFinished)
                    isRunning = !isRunning;
            }

            ImGui.SameLine();

            if (isRunning) ImGui.BeginDisabled();
            if (ImGui.Button("Step"))
            {
                if (!isFinished)
                {
                    algorithm.ExecuteStep();
                    image = algorithm.GetImage();
                    isFinished = algorithm.IsFinished();
                    if (isFinished)
                        result = algorithm.GetResult();
                    UpdateTexture();
                }
            }
            if (isRunning) ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button("Run Instantly"))
            {
                if (!isFinished)
                {
                    runInstantly = true;
                    isRunning = false;
                }
            }
            if (isFinished) ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Reset"))
            {
                algorithm = LoadAlgorithm(selectedAlgorithm);
                image = algorithm.GetImage();
                isFinished = false;
                result = "";
                isRunning = false;
                ReinitializeTexture();
            }
            
            ImGui.Spacing();

            ImGui.SliderFloat("Scale", ref defaultRenderScale, 1f, maxRenderScale);

            ImGui.Text("Step Interval (ms):");
            bool disableInterval = isRunning;
            if (disableInterval) ImGui.BeginDisabled();
            string intervalStr = stepInterval.TotalMilliseconds.ToString();
            if (ImGui.InputText("##interval", ref intervalStr, 10, ImGuiInputTextFlags.CharsDecimal))
            {
                if (int.TryParse(intervalStr, out int ms))
                    stepInterval = TimeSpan.FromMilliseconds(Math.Max(ms, 1));
            }
            if (disableInterval) 
                ImGui.EndDisabled();

            ImGui.Spacing();

            if (isFinished)
            {
                ImGui.Separator();
                ImGui.Text("Result:");

                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0f, 1f, 0f, 1f)); // Green color RGBA
                ImGui.TextWrapped(result);
                ImGui.PopStyleColor();
            }

            ImGui.End();

            ImGui.Begin("Grid");
            ImGui.Image(new IntPtr(texture), new Vector2(image.Width * defaultRenderScale, image.Height * defaultRenderScale));
            ImGui.End();

            controller.Render();
        };

        window.FramebufferResize += size => gl.Viewport(size);

        window.Closing += () =>
        {
            controller.Dispose();
            inputContext.Dispose();
            gl.Dispose();
            cts.Cancel();
        };

        window.Run();
    }

    private unsafe static void UpdateTexture()
    {
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                fixed (void* data = accessor.GetRowSpan(y))
                {
                    gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1,
                        PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
            }
        });
    }

    private static void DiscoverAlgorithms()
    {
        availableAlgorithms = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => typeof(IPuzzleAlgorithm).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToDictionary(t => t.Name, t => t);
    }

    private static IPuzzleAlgorithm LoadAlgorithm(string className)
    {
        if (!availableAlgorithms.TryGetValue(className, out var type))
            throw new Exception("Algorithm not found: " + className);

        return (IPuzzleAlgorithm)Activator.CreateInstance(type);
    }

    private static unsafe void ReinitializeTexture()
    {
        // Delete old texture if it exists
        if (texture != 0)
            gl.DeleteTexture(texture);

        // Create new texture
        texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        // Allocate storage for the new image size
        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba,
            (uint)image.Width,
            (uint)image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            null
        );

        // Set filtering
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

        // Upload pixel data
        UpdateTexture();
    }

}
