namespace Tetris.Extensions;
public static class ArrayExtensions
{
    public static T At<T>(this T[,] array, params int[] indices)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            var dimIdx = indices[i];
            var dimLen = array.GetLength(i);

            while (dimIdx < 0)
            {
                dimIdx += dimLen;
            }

            while (dimIdx >= dimLen)
            {
                dimIdx -= dimLen;
            }

            indices[i] = dimIdx;
        }

        return (T)array.GetValue(indices)!;
    }
}
