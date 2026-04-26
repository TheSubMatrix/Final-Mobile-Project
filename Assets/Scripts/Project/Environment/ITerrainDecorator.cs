using UnityEngine;

public interface ITerrainDecorator
{
    void Initialize(IInjector injector);
    void DecorateTerrain(TerrainSegment segment);
    void CleanupTerrain(TerrainSegment segment);
}
