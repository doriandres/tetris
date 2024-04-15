using SharpHook;
using SharpHook.Native;
using Tetris.Extensions;

namespace Tetris;

public partial class MainPage : ContentPage
{
    private readonly Border[,] GridBoxes = new Border[10, 20];
    private readonly bool[,] FilledBoxes = new bool[10, 20];
    private long ScorePoints = 0;

    private bool IsGameOver = false;
    private readonly Border[,] NextGridBoxes = new Border[4, 4];

    private TetrisShape CurrentShape;
    private TetrisShape NextShape;

    private bool IsPainting = false;
    private int FallIntervalInMs = 400;

    private TetrisRotation TetrisRotation = TetrisRotation.Degrees0;
    private int PositionX = 4;
    private int PositionY = 0;

    private TetrisRotation PreviousRotation;
    private int PreviousX;
    private int PreviousY;

    private TetrisShape[] AvailableShapes = [
        new TangoShape(),
        new OscarShape(),
        new IndiaShape(),
        new JuliettShape(),
        new LimaShape(),
        new SierraShape(),
        new ZuluShape(),
    ];

    public MainPage()
    {
        InitializeComponent();
        BuildGrid();

        PreviousRotation = TetrisRotation;
        PreviousX = PositionX;
        PreviousY = PositionY;

        CurrentShape = GetRandomShape();
        NextShape = GetRandomShape();

        RenderNext();

        TetrisController.KeyPressed += OnKeyPressed;
        _ = GameLoop().ConfigureAwait(true);

        TetrisMusic.Loaded += (_, _) =>
        {
            TetrisMusic.Play();
            TetrisMusic.ShouldLoopPlayback = true;
        };
    }
    public void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (IsPainting) return;

        switch (e.Data.KeyCode)
        {
            case KeyCode.VcUp:
                switch (TetrisRotation)
                {
                    case TetrisRotation.Degrees0:
                        TetrisRotation = TetrisRotation.Degrees90;
                        break;
                    case TetrisRotation.Degrees90:
                        TetrisRotation = TetrisRotation.Degrees180;
                        break;
                    case TetrisRotation.Degrees180:
                        TetrisRotation = TetrisRotation.Degrees270;
                        break;
                    case TetrisRotation.Degrees270:
                        TetrisRotation = TetrisRotation.Degrees0;
                        break;
                }
                Render();
                break;
            case KeyCode.VcLeft:
                PositionX -= 1;
                if (PositionX < 0)
                {
                    PositionX = 0;
                }
                Render();
                break;
            case KeyCode.VcRight:
                PositionX += 1;
                if (PositionX >= GridBoxes.GetLength(0))
                {
                    PositionX = GridBoxes.GetLength(0) - 1;
                }
                Render();
                break;
            case KeyCode.VcDown:
                PositionY += 1;
                if (PositionY >= GridBoxes.GetLength(1))
                {
                    PositionY = GridBoxes.GetLength(1) - 1;
                }
                Render();
                break;
            default:
                break;
        }

    }

    private void BuildGrid()
    {
        var size = 40;

        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                Border box = new()
                {
                    Background = Colors.White,
                    Stroke = Colors.WhiteSmoke,
                    StrokeThickness = 1,
                    WidthRequest = size,
                    HeightRequest = size,
                };
                FilledBoxes[x, y] = false;
                GridBoxes[x, y] = box;
                TetrisBoard.Add(box);
            }
        }

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Border box = new()
                {
                    StrokeThickness = 1,
                    WidthRequest = size,
                    HeightRequest = size,
                };
                NextGridBoxes[x, y] = box;
                TetrisNextBoard.Add(box);
            }
        }
    }

    private void RenderNext()
    {
        var nextPoints = NextShape.Build(0, 0, 4, 4, TetrisRotation.Degrees0);

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                var b = NextGridBoxes[x, y];
                b.Background = null;
                b.Stroke = Colors.Black;
            }
        }

        foreach (var (x, y) in nextPoints)
        {
            var b = NextGridBoxes[x, y];
            b.Background = NextShape.GetColor();
            b.Stroke = Colors.WhiteSmoke;
        }
    }

    private TetrisShape GetRandomShape()
    {
        Random random = new();
        random.Shuffle(AvailableShapes);
        var i = random.Next(AvailableShapes.Length);
        return AvailableShapes[i];
    }


    private bool CheckOverlap((int x, int y)[] points) => points.Any(p => FilledBoxes[p.x, p.y]);

    private async void Paint()
    {
        if (IsPainting) return;

        IsPainting = true;

        var lastPoints = CurrentShape.Build(PreviousX, PreviousY, GridBoxes.GetLength(0), GridBoxes.GetLength(1), PreviousRotation);
        foreach (var (x, y) in lastPoints) GridBoxes.At(x, y).Background = Colors.White;

        var shapePoints = CurrentShape.Build(PositionX, PositionY, GridBoxes.GetLength(0), GridBoxes.GetLength(1), TetrisRotation);

        if (CheckOverlap(shapePoints))
        {
            PositionY = PreviousY;
            PositionX = PreviousX;
            TetrisRotation = PreviousRotation;

            shapePoints = CurrentShape.Build(PreviousX, PreviousY, GridBoxes.GetLength(0), GridBoxes.GetLength(1), PreviousRotation);

            IsGameOver = PositionY == 0;

            foreach (var (x, y) in shapePoints) GridBoxes.At(x, y).Background = IsGameOver ? Colors.Black : CurrentShape.GetColor();

            if (IsGameOver)
            {
                GameOverLabel.Text = "Game Over!";
                return;
            }
        }
        else
        {
            foreach (var (x, y) in shapePoints) GridBoxes.At(x, y).Background = CurrentShape.GetColor();
        }

        if (CheckForLanding(shapePoints))
        {
            foreach (var (x, y) in shapePoints) FilledBoxes[x, y] = true;

            for (var y = FilledBoxes.GetLength(1) - 1; y >= 0;)
            {
                var rowFilled = true;

                for (var x = 0; x < FilledBoxes.GetLength(0); x++) rowFilled = rowFilled && FilledBoxes[x, y];

                if (rowFilled)
                {
                    for (var x = 0; x < FilledBoxes.GetLength(0); x++)
                    {
                        GridBoxes.At(x, y).Background = Colors.White;
                        FilledBoxes[x, y] = false;
                    }

                    await Task.Yield();

                    for (var iy = y - 1; iy >= 0; iy--)
                    {
                        for (var x = 0; x < FilledBoxes.GetLength(0); x++)
                        {
                            FilledBoxes[x, iy + 1] = FilledBoxes[x, iy];
                            GridBoxes.At(x, iy + 1).Background = GridBoxes.At(x, iy).Background;
                            FilledBoxes[x, iy] = false;
                            GridBoxes.At(x, iy).Background = Colors.White;
                        }
                        await Task.Yield();
                    }

                    ScorePoints += 100;
                    TetrisScore.Text = ScorePoints.ToString();
                }
                else
                {
                    y--;
                }
            }

            TetrisRotation = TetrisRotation.Degrees0;
            PreviousRotation = TetrisRotation;

            PositionY = 0;
            PositionX = 4;

            PreviousX = PositionX;
            PreviousY = PositionY;

            CurrentShape = NextShape;
            NextShape = GetRandomShape();

            RenderNext();

            await Task.Delay(FallIntervalInMs).ConfigureAwait(true);
        }
        else
        {
            PreviousRotation = TetrisRotation;
            PreviousX = PositionX;
            PreviousY = PositionY;
        }

        IsPainting = false;
    }


    private void Render()
    {
        if (MainThread.IsMainThread) Paint();
        else MainThread.BeginInvokeOnMainThread(Paint);
    }

    private bool CheckForLanding((int x, int y)[] points)
    {
        var h = CurrentShape.GetShapeHeight(points);

        if (h == GridBoxes.GetLength(1) - 1)
        {
            return true;
        }

        foreach (var (x, y) in points) if (FilledBoxes[x, y + 1]) return true;

        return false;
    }

    private async Task GameLoop()
    {
        while (!IsGameOver)
        {
            if (!IsPainting)
            {
                Render();
                await Task.Delay(FallIntervalInMs).ConfigureAwait(true);
                PositionY++;
            }
            else
            {
                await Task.Yield();
            }
        }

        TetrisMusic.Stop();
    }
}