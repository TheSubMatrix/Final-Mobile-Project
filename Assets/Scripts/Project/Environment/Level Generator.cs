using System.Collections.Generic;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] int m_initialSegmentCount = 2;
    [SerializeField] int m_triggerLookahead = 2;
    [SerializeField] float m_triggerHeight = 1000f;
    [Header("Prefabs")]
    [SerializeField] PrefabPool<TerrainSegment> m_terrainSegmentPool;
    [SerializeField, RequiredField] TerrainLoadTrigger m_loadTriggerPrefab;
    [Header("Decorators")]
    [ClassSelector, SerializeReference] ITerrainDecorator m_powerUpDecorator;
    [ClassSelector, SerializeReference] ITerrainDecorator m_coinDecorator;

    [Inject] IInjector m_injector;
    readonly LinkedList<TerrainSegment> m_activeSegments = new();
    TerrainLoadTrigger m_loadTrigger;

    void Start()
    {
        m_terrainSegmentPool.Initialize();
        m_powerUpDecorator?.Initialize(m_injector);
        m_coinDecorator?.Initialize(m_injector);
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
        TerrainSegment oldest = m_activeSegments.First.Value;
        m_activeSegments.RemoveFirst();
        m_powerUpDecorator?.CleanupTerrain(oldest);
        m_coinDecorator?.CleanupTerrain(oldest);
        m_terrainSegmentPool.Release(oldest);
        m_activeSegments.AddLast(SpawnSegment(m_activeSegments.Last.Value));
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
        m_powerUpDecorator?.DecorateTerrain(segment);
        m_coinDecorator?.DecorateTerrain(segment);
        segment.ShapeController.RefreshSpriteShape();
        return segment;
    }

    static void GenerateTerrainForSegment(TerrainSegment segment, TerrainSegment prior = null)
    {
        if (segment == null) return;
        if (prior?.OverlapPoints.Length > 0) segment.GenerateTerrain(prior.OverlapPoints);
        else segment.GenerateTerrain();
    }
}