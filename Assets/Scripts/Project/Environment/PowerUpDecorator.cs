using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

[Serializable]
public class PowerUpDecorator : ITerrainDecorator
{
    [SerializeField, Range(0f, 1f)] float m_spawnChance = 0.3f;
    [SerializeField] float m_heightOffset = 1f;
    [SerializeField] PrefabPool<Collectible> m_powerUpPool;
    readonly Dictionary<TerrainSegment, List<Collectible>> m_activePowerUps = new();
    public void Initialize(IInjector injector)
    {
        m_powerUpPool.Initialize(() => SpawnCollectible(m_powerUpPool.Prefab, injector));
    }

    public void DecorateTerrain(TerrainSegment segment)
    {
        List<Collectible> powerUps = new();
        m_activePowerUps[segment] = powerUps;
        foreach (TerrainPointData point in segment.TerrainPoints)
        {
            if (!point.IsValley || Random.value > m_spawnChance) continue;
            Collectible powerUp = m_powerUpPool.Get();
            powerUp.transform.position = new(point.Position.x + segment.transform.position.x, point.Position.y + m_heightOffset);
            powerUp.Initialize(() => { powerUps.Remove(powerUp); m_powerUpPool.Release(powerUp); });
            powerUp.gameObject.SetActive(true);
            powerUps.Add(powerUp);
        }
    }

    public void CleanupTerrain(TerrainSegment segment)
    {
        if (!m_activePowerUps.TryGetValue(segment, out List<Collectible> powerUps)) return;
        foreach (Collectible powerUp in powerUps)
        {
            powerUp.gameObject.SetActive(false);
            m_powerUpPool.Release(powerUp);
        }
        m_activePowerUps.Remove(segment);
    }
    Collectible SpawnCollectible(Collectible collectible, IInjector injector)
    {
        Collectible spawnedCollectible = Object.Instantiate(collectible);
        spawnedCollectible.Initialize(() =>
        {
            m_powerUpPool.Release(spawnedCollectible);
        });
        spawnedCollectible.gameObject.SetActive(false);
        injector.Inject(spawnedCollectible);
        return spawnedCollectible;
    }
}