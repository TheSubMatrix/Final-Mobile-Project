using System.Collections.Generic;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using UnityEngine;
using UnityEngine.Pool;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int m_initialSegmentCount = 2;
    [SerializeField] int m_triggerLookahead = 2;
    [SerializeField] float m_triggerHeight = 1000f;
    [Header("Prefabs")]
    [SerializeField, RequiredField] TerrainSegment m_segmentPrefab;
    [SerializeField, RequiredField] TerrainLoadTrigger m_loadTriggerPrefab;
    [SerializeField, RequiredField] Collectible m_coinPrefab;
    [SerializeField, RequiredField] Collectible m_powerUpPrefab;
    
    [Inject] IInjector m_injector;
    IObjectPool<TerrainSegment> m_terrainSegmentPool;
    IObjectPool<Collectible> m_coinPool;
    IObjectPool<Collectible> m_powerUpPool;
    readonly LinkedList<TerrainSegment> m_activeSegments = new();
    TerrainLoadTrigger m_loadTrigger;

    void Start()
    {
        m_terrainSegmentPool = new ObjectPool<TerrainSegment>(CreateTerrainSegment);
        m_coinPool = new ObjectPool<Collectible>(() => SpawnCollectible(m_coinPrefab));
        m_powerUpPool = new ObjectPool<Collectible>(() => SpawnCollectible(m_powerUpPrefab));
        m_loadTrigger = Instantiate(m_loadTriggerPrefab);
        m_loadTrigger.OnThresholdCrossed += OnThresholdCrossed;
        m_activeSegments.AddLast(SpawnSegment(null));
        for (int i = 0; i < m_initialSegmentCount; i++)
            m_activeSegments.AddLast(SpawnSegment(m_activeSegments.Last.Value));
        UpdateLoadTrigger();
    }

    void OnDestroy()
    {
        m_loadTrigger.OnThresholdCrossed -= OnThresholdCrossed;
    }

    void OnThresholdCrossed()
    {
        m_activeSegments.AddLast(SpawnSegment(m_activeSegments.Last.Value));
        TerrainSegment oldest = m_activeSegments.First.Value;
        m_activeSegments.RemoveFirst();
        m_terrainSegmentPool.Release(oldest);
        UpdateLoadTrigger();
    }

    void UpdateLoadTrigger()
    {
        LinkedListNode<TerrainSegment> node = m_activeSegments.Last;
        for (int i = 0; i < m_triggerLookahead && node.Previous != null; i++) node = node.Previous;
        TerrainPointData[] overlapPoints = node.Value.OverlapPoints;
        if (overlapPoints.Length == 0) return;
        m_loadTrigger.SetBounds(overlapPoints[0].Position.x, m_triggerHeight);
    }

    TerrainSegment SpawnSegment(TerrainSegment prior)
    {
        TerrainSegment segment = m_terrainSegmentPool.Get();
        segment.gameObject.SetActive(true);
        segment.Initialize();
        float startX = prior?.OverlapPoints.Length > 0 ? prior.OverlapPoints[0].Position.x : 0f;
        segment.transform.position = new(startX, 0f, 0f);
        GenerateTerrainForSegment(segment, prior);
        segment.ShapeController.RefreshSpriteShape();
        return segment;
    }

    static void GenerateTerrainForSegment(TerrainSegment segment, TerrainSegment prior = null)
    {
        if (segment == null) return;
        if (prior?.OverlapPoints.Length > 0) segment.GenerateTerrain(prior.OverlapPoints);
        else segment.GenerateTerrain();
    }

    TerrainSegment CreateTerrainSegment()
    {
        TerrainSegment segment = Instantiate(m_segmentPrefab);
        segment.gameObject.SetActive(false);
        return segment;
    }

    Collectible SpawnCollectible(Collectible collectible)
    {
        Collectible spawnedCoin = Instantiate(collectible);
        spawnedCoin.gameObject.SetActive(false);
        m_injector.Inject(spawnedCoin);
        return spawnedCoin;
    }
}