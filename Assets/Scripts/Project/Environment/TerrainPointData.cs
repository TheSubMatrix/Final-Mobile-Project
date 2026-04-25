using UnityEngine;

public readonly struct TerrainPointData
{
    public readonly Vector2 Position;
    public readonly bool IsValley;

    public TerrainPointData(Vector2 position, bool isValley)
    {
        Position = position;
        IsValley = isValley;
    }
}