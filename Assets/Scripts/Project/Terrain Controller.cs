using System.Collections.Generic;
using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.Pool;

public class TerrainController : MonoBehaviour
{
    [SerializeField, RequiredField] TerrainSegment m_segmentPrefab;
    [SerializeField, RequiredField] TerrainLoadTrigger m_loadTriggerPrefab;
    [SerializeField] int m_initialSegmentCount = 2;
    [SerializeField] int m_triggerLookahead = 2;
    [SerializeField] float m_triggerHeight = 1000f;

    IObjectPool<TerrainSegment> m_terrainSegmentPool;
    readonly LinkedList<TerrainSegment> m_activeSegments = new();
    TerrainLoadTrigger m_loadTrigger;

    void Start()
    {
        m_terrainSegmentPool = new ObjectPool<TerrainSegment>(CreateTerrainSegment);
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
        for (int i = 0; i < m_triggerLookahead && node.Previous != null; i++)
            node = node.Previous;

        Vector2[] overlapPoints = node.Value.OverlapPoints;
        if (overlapPoints.Length == 0) return;

        m_loadTrigger.SetBounds(overlapPoints[0].x, m_triggerHeight);
    }

    TerrainSegment SpawnSegment(TerrainSegment prior)
    {
        TerrainSegment segment = m_terrainSegmentPool.Get();
        segment.gameObject.SetActive(true);
        segment.Initialize();
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
}