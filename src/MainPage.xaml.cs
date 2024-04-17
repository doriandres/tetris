using SharpHook;
using SharpHook.Native;
using Tetris.Extensions;

namespace Tetris;

public partial class MainPage : ContentPage
{
    private readonly Border[,] GridBoxes = new Border[10, 20];
    private readonly bool[,] FilledBoxes = new bool[10, 20];
    private readonly Border[,] NextGridBoxes = new Border[4, 4];

    private long ScorePoints = 0;
    private bool IsGameOver = false;
    private bool IsCheckingGame = false;
    private TetrisShape CurrentShape;
    private TetrisShape NextShape;
    private int FallIntervalInMs = 400;
    private TetrisRotation TetrisRotation = TetrisRotation.Degrees0;
    private int PositionX = 4;
    private int PositionY = 0;
    private TetrisRotation PreviousRotation;
    private int PreviousX;
    private int PreviousY;

    private TetrisShape[] AvailableShapes = [new TangoShape(), new OscarShape(), new IndiaShape(), new JuliettShape(), new LimaShape(), new SierraShape(), new ZuluShape()];

    public MainPage()
    {
        InitializeComponent();
        BuildGrid();

        PreviousRotation = TetrisRotation;
        PreviousX = PositionX;
        PreviousY = PositionY;

        CurrentShape = GetRandomShape();
        NextShape = GetRandomShape();

        PaintNextShape();

        TetrisController.KeyPressed += OnKeyDown;
        _ = Loop().ConfigureAwait(true);

        TetrisMusic.Loaded += (_, _) =>
        {
            TetrisMusic.Play();
            TetrisMusic.ShouldLoopPlayback = true;
        };
    }

    private TetrisShape GetRandomShape()
    {
        Random random = new();
        random.Shuffle(AvailableShapes);
        var i = random.Next(AvailableShapes.Length);
        return AvailableShapes[i];
    }

    private (int x, int y)[] GetCurrentShapePoints()
    {
        return CurrentShape.Build(PositionX, PositionY, GridBoxes.GetLength(0), GridBoxes.GetLength(1), TetrisRotation);
    }

    private void OnKeyDown(object? sender, KeyboardHookEventArgs e)
    {
        if (MainThread.IsMainThread)
        {
            HandleKeyBoard(e);
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(() => HandleKeyBoard(e));
        }
    }

    private void HandleKeyBoard(KeyboardHookEventArgs e)
    {
        if (IsCheckingGame || IsGameOver)
        {
            return;
        }

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
                Paint();
                break;
            case KeyCode.VcLeft:
                PositionX -= 1;
                if (PositionX < 0)
                {
                    PositionX = 0;
                }
                Paint();
                break;
            case KeyCode.VcRight:
                PositionX += 1;
                if (PositionX >= GridBoxes.GetLength(0))
                {
                    PositionX = GridBoxes.GetLength(0) - 1;
                }
                Paint();
                break;
            case KeyCode.VcDown:
                PositionY += 1;
                if (PositionY >= GridBoxes.GetLength(1))
                {
                    PositionY = GridBoxes.GetLength(1) - 1;
                }
                Paint();
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

    private void PaintNextShape()
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

    private bool CheckOverlap((int x, int y)[] points) => points.Any(p => FilledBoxes[p.x, p.y]);

    private bool CheckForLanding((int x, int y)[] points)
    {
        var h = CurrentShape.GetShapeHeight(points);

        if (h == GridBoxes.GetLength(1) - 1)
        {
            return true;
        }

        foreach (var (x, y) in points)
        {
            if (FilledBoxes[x, y + 1])
            {
                return true;
            }
        };

        return false;
    }

    private async Task CheckGame()
    {
        var shapePoints = GetCurrentShapePoints();

        if (!CheckForLanding(shapePoints))
        {
            PositionY++;            
            return;
        }

        if (PositionY == 0)
        {
            IsGameOver = true;
            GameOverLabel.Text = "Game Over!";

            return;
        }

        foreach (var (x, y) in shapePoints)
        {
            FilledBoxes[x, y] = true;
        };

        var checkY = FilledBoxes.GetLength(1);
        --checkY;

        while (checkY > -1)
        {
            var rowFilled = true;

            for (var x = 0; x < FilledBoxes.GetLength(0); x++)
            {
                rowFilled = rowFilled && FilledBoxes[x, checkY];
            }

            if (!rowFilled)
            {
                --checkY;
                continue;
            }

            for (var x = 0; x < FilledBoxes.GetLength(0); x++)
            {
                FilledBoxes[x, checkY] = false;
                GridBoxes[x, checkY].Background = Colors.White;                
            }

            await Task.Delay(FallIntervalInMs).ConfigureAwait(true);

            for (var aboveCheckY = checkY - 1; aboveCheckY > -1; aboveCheckY--)
            {
                for (var x = 0; x < FilledBoxes.GetLength(0); x++)
                {
                    FilledBoxes[x, aboveCheckY + 1] = FilledBoxes[x, aboveCheckY];
                    GridBoxes[x, aboveCheckY + 1].Background = GridBoxes.At(x, aboveCheckY).Background;

                    FilledBoxes[x, aboveCheckY] = false;
                    GridBoxes[x, aboveCheckY].Background = Colors.White;
                }
            }

            ScorePoints += 100;
            TetrisScore.Text = ScorePoints.ToString();

            await Task.Delay(FallIntervalInMs).ConfigureAwait(true);
        }

        TetrisRotation = TetrisRotation.Degrees0;
        PreviousRotation = TetrisRotation;
        PositionY = 0;
        PositionX = 4;
        PreviousX = PositionX;
        PreviousY = PositionY;
        CurrentShape = NextShape;
        NextShape = GetRandomShape();
        PaintNextShape();

        await Task.Delay(FallIntervalInMs).ConfigureAwait(true);
    }

    private void Paint()
    {
        var shapePoints = GetCurrentShapePoints();

        if (CheckOverlap(shapePoints))
        {
            PositionY = PreviousY;
            PositionX = PreviousX;
            TetrisRotation = PreviousRotation;

            return;
        }

        if (PreviousX != PositionX || PreviousY != PositionY || TetrisRotation != PreviousRotation)
        {
            var lastPoints = CurrentShape.Build(PreviousX, PreviousY, GridBoxes.GetLength(0), GridBoxes.GetLength(1), PreviousRotation);

            foreach (var (x, y) in lastPoints)
            {                
                GridBoxes[x, y].Background = Colors.White;                
            }
        }

        foreach (var (x, y) in shapePoints)
        {            
            GridBoxes[x, y].Background = CurrentShape.GetColor();
        }

        PreviousRotation = TetrisRotation;
        PreviousX = PositionX;
        PreviousY = PositionY;
    }

    private async Task Tick()
    {
        Paint();
        await Task.Delay(FallIntervalInMs).ConfigureAwait(true);
        IsCheckingGame = true;
        await CheckGame();
        IsCheckingGame = false;
    }

    private async Task Loop()
    {
        while (!IsGameOver)
        {
            await Task.Yield();
            await Tick();
        }

        TetrisMusic.Stop();
    }
}