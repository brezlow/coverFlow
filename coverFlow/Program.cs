using Raylib_cs;
using System.Numerics;

class Program
{
    static int screenWidth = 1280;
    static int screenHeight = 720;
    static int currentIndex = 1;

    static Texture2D[] covers;

    static void Main()
    {
        Console.WriteLine($"当前工作目录: {Directory.GetCurrentDirectory()}");
        Raylib.InitWindow(screenWidth, screenHeight, "Raylib C# CoverFlow");
        Raylib.SetTargetFPS(60);

        covers = new Texture2D[]
        {
            Raylib.LoadTexture("/home/breeze/RiderProjects/coverFlow/coverFlow/assets/cover3.jpg"),
            Raylib.LoadTexture("/home/breeze/RiderProjects/coverFlow/coverFlow/assets/cover3.jpg"),
            Raylib.LoadTexture("/home/breeze/RiderProjects/coverFlow/coverFlow/assets/cover3.jpg")
        };

        while (!Raylib.WindowShouldClose())
        {
            Update();
            Draw();
        }

        foreach (var tex in covers)
            Raylib.UnloadTexture(tex);
        Raylib.CloseWindow();
    }

    static void Update()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Right))
            currentIndex = Math.Min(currentIndex + 1, covers.Length - 1);
        if (Raylib.IsKeyPressed(KeyboardKey.Left))
            currentIndex = Math.Max(currentIndex - 1, 0);
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib_cs.Color.DarkGray);

        int centerX = screenWidth / 2;
        int centerY = screenHeight / 2;

        for (int i = 0; i < covers.Length; i++)
        {
            Texture2D tex = covers[i];
            float spacing = 250f;
            float scale = (i == currentIndex) ? 1.0f : 0.6f;
            float angle = (i == currentIndex) ? 0f : (i < currentIndex ? 30f : -30f);

            Vector2 texSize = new Vector2(tex.Width, tex.Height) * scale;
            Vector2 origin = texSize / 2f;
            Vector2 position = new(centerX + (i - currentIndex) * spacing, centerY);

            Raylib.DrawTexturePro(
                tex,
                new Rectangle(0, 0, tex.Width, tex.Height),
                new Rectangle(position.X, position.Y, texSize.X, texSize.Y),
                origin,
                angle,
                Raylib_cs.Color.White
            );
        }

        Raylib.EndDrawing();
    }
}
