using SharpHook;

namespace Tetris;

public static class TetrisController
{
    public static event EventHandler<KeyboardHookEventArgs>? KeyPressed;
    public static void OnKeyPressed(object? sender, KeyboardHookEventArgs e) => KeyPressed?.Invoke(sender, e);
}