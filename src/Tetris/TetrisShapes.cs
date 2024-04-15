namespace Tetris;

public abstract class TetrisShape
{
    public abstract Color GetColor();
    public abstract (int x, int y)[] BuildTemplate(TetrisRotation rotation);
    public virtual int GetShapeWidth((int x, int y)[] template) => template.Select(t => t.x).Max();
    public virtual int GetShapeHeight((int x, int y)[] template) => template.Select(t => t.y).Max();
    internal virtual (int x, int y)[] ApplyBoundsToTemplate((int x, int y)[] template, int x, int y) => template.Select(t => (t.x + x, t.y + y)).ToArray();
    public virtual (int x, int y)[] Build(int x, int y, int availableWidth, int availableHeight, TetrisRotation rotation)
    {
        var template = BuildTemplate(rotation);
        var templateWidth = GetShapeWidth(template);
        var templateHeight = GetShapeHeight(template);

        if (x + templateWidth >= availableWidth)
        {
            x = (availableWidth - 1) - templateWidth;
        }

        if (y + templateHeight >= availableHeight)
        {
            y = (availableHeight - 1) - templateHeight;
        }

        return ApplyBoundsToTemplate(template, x, y);
    }
}

/// <summary>
/// [][][]
///   []
/// </summary>
public class TangoShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            //   []
            // [][][]
            TetrisRotation.Degrees0 => [
                        (1, 0),
                (0, 1), (1, 1), (2, 1),
            ],

            // []
            // [][]
            // []
            TetrisRotation.Degrees90 => [
                (0, 0),
                (0, 1), (1, 1),
                (0, 2),
            ],

            // [][][]
            //   []
            TetrisRotation.Degrees180 => [
                (0, 0), (1, 0), (2, 0),
                        (1, 1),
            ],

            //   []
            // [][]
            //   []
            TetrisRotation.Degrees270 => [
                        (1, 0),
                (0, 1), (1, 1),
                        (1, 2),
            ],

            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("a000f0");
    }
}

/// <summary>
/// [][]
/// [][]
/// </summary>
public class OscarShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            // [][]
            // [][]
            TetrisRotation.Degrees0 or TetrisRotation.Degrees90 or TetrisRotation.Degrees180 or TetrisRotation.Degrees270 => [
                (0, 0), (1, 0),
                (0, 1), (1, 1),
            ],
            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("f0f000");
    }
}

/// <summary>
/// []
/// []
/// []
/// []
/// </summary>
public class IndiaShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            TetrisRotation.Degrees0 or TetrisRotation.Degrees180 => [
                (0, 0), (1, 0), (2, 0), (3, 0),
            ],
            TetrisRotation.Degrees90 or TetrisRotation.Degrees270 => [
                (0, 0),
                (0, 1),
                (0, 2),
                (0, 3),
            ],
            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("00f0f0");
    }
}

/// <summary>
///   []
///   []
/// [][]
/// </summary>
public class JuliettShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            //   []
            //   []
            // [][]
            TetrisRotation.Degrees0 => [
                        (1, 0),
                        (1, 1),
                (0, 2), (1, 2),
            ],

            // []
            // [][][]
            TetrisRotation.Degrees90 => [
                (0, 0),
                (0, 1), (1, 1), (2, 1),
            ],

            // [][]
            // []
            // []
            TetrisRotation.Degrees180 => [
                (0, 0), (1, 0),
                (0, 1),
                (0, 2),
            ],

            // [][][]
            //     []
            TetrisRotation.Degrees270 => [
                (0, 0), (1, 0), (2, 0),
                                (2, 1),
            ],

            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("0000f0");
    }
}

/// <summary>
/// []
/// []
/// [][]
/// </summary>
public class LimaShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            // []
            // []
            // [][]
            TetrisRotation.Degrees0 => [
                (0, 0),
                (0, 1),
                (0, 2), (1, 2),
            ],

            // [][][]
            // []
            TetrisRotation.Degrees90 => [
                (0, 0), (1, 0), (2, 0),
                (0, 1),
            ],

            // [][]
            //   []
            //   []
            TetrisRotation.Degrees180 => [
                (0, 0),(1, 0),
                       (1, 1),
                       (1, 2),
            ],

            //     []
            // [][][]            
            TetrisRotation.Degrees270 => [
                                (2, 0),
                (0, 1), (1, 1), (2, 1),
            ],

            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("f0a000");
    }
}

/// <summary>
///   [][]
/// [][]
/// </summary>
public class SierraShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            //   [][]
            // [][]
            TetrisRotation.Degrees0 or TetrisRotation.Degrees180 => [
                        (1, 0), (2, 0),
                (0, 1), (1, 1),
            ],

            // []
            // [][]
            //   []
            TetrisRotation.Degrees90 or TetrisRotation.Degrees270 => [
                (0, 0),
                (0, 1), (1, 1),
                        (1, 2),
            ],

            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("00f000");
    }
}

/// <summary>
/// [][]
///   [][]
/// </summary>
public class ZuluShape : TetrisShape
{
    public override (int x, int y)[] BuildTemplate(TetrisRotation rotation)
    {
        return rotation switch
        {
            // [][]
            //   [][]
            TetrisRotation.Degrees0 or TetrisRotation.Degrees180 => [
                (0, 0), (1, 0),
                        (1, 1), (2, 1),
            ],

            //   []
            // [][]
            // []
            TetrisRotation.Degrees90 or TetrisRotation.Degrees270 => [
                        (1, 0),
                (0, 1), (1, 1),
                (0, 2),
            ],

            _ => throw new NotSupportedException(),
        };
    }

    public override Color GetColor()
    {
        return Color.FromArgb("f00000");
    }
}