using System;
using System.Collections.Generic;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int m_initialSegmentCount = 2;
    [SerializeField] int m_triggerLookahead = 2;
    [SerializeField] float m_triggerHeight = 1000f;
    [SerializeField] DifficultySettings m_easiestDifficultySettings;
    [SerializeField] DifficultySettings m_hardestDifficultySettings;
    [SerializeField] float m_difficultyRate = .001f;
    [Header("Prefabs")]
    [SerializeField] PrefabPool<TerrainSegment> m_terrainSegmentPool;
    [SerializeField, RequiredField] TerrainLoadTrigger m_loadTriggerPrefab;
    [Header("Decorators")]
    [ClassSelector, SerializeReference] ITerrainDecorator m_powerUpDecorator;
    [ClassSelector, SerializeReference] ITerrainDecorator m_coinDecorator;
    
    [Inject] IInjector m_injector;
    [Inject] IScoreReader m_scoreManager;
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
        ApplyDifficultySettings(segment);
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

    static float InterpolateExponential(float x, float v1, float v2, float rate = 1)
    {
        float t = 1 - Mathf.Exp(-x * rate);
        return v1 + t * (v2 - v1);
    }

    static DifficultySettings InterpolateDifficultySettings(float x, DifficultySettings v1, DifficultySettings v2, float rate = 1)
    {
        DifficultySettings result = new()
        {
            Minimums = new(InterpolateExponential(x, v1.Minimums.x, v2.Minimums.x, rate), 
                InterpolateExponential(x, v1.Minimums.y, v2.Minimums.y, rate)),
            Maximums = new(InterpolateExponential(x, v1.Maximums.x, v2.Maximums.x, rate),
                InterpolateExponential(x, v1.Maximums.y, v2.Maximums.y, rate))
        };
        return result;
    }

    void ApplyDifficultySettings(TerrainSegment segment)
    {
        DifficultySettings current = InterpolateDifficultySettings(m_scoreManager.GetCurrentScore().Distance, m_easiestDifficultySettings, m_hardestDifficultySettings, m_difficultyRate);
        segment.m_maximums = current.Maximums;
        segment.m_minimums = current.Minimums;
    }
    
    [Serializable]
    struct DifficultySettings
    {
        public Vector2 Minimums;
        public Vector2 Maximums;
    }
}