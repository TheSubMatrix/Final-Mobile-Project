using System;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteShapeController))]
public class TerrainSegment : MonoBehaviour
{
    public SpriteShapeController ShapeController { get; private set; }
    [Range(3, 100), SerializeField] int m_terrainResolution = 50;
    [Range(1, 20), SerializeField] int m_overlapCount = 5;
    [SerializeField] Vector2 m_minimums = new(16, 6);
    [SerializeField] Vector2 m_maximums = new(34, 12);
    [SerializeField] float m_floorDistance = 30;
    [SerializeField] float m_maxHeight = 10;
    [SerializeField] float m_minHeight;
    [SerializeField] float m_tangentScale = 1 / 3f;

    public TerrainPointData[] OverlapPoints { get; private set; } = Array.Empty<TerrainPointData>();
    public TerrainPointData[] TerrainPoints { get; private set; } = Array.Empty<TerrainPointData>();

    void OnEnable() => Initialize();

    public void Initialize()
    {
        ShapeController = GetComponent<SpriteShapeController>();
    }

    public void GenerateTerrain(TerrainPointData[] overlapPoints = null, int? seed = null)
    {
        if (ShapeController == null) return;
        ShapeController.spline.Clear();

        TerrainPointData[] points = GenerateTerrainKeyPoints(overlapPoints, seed ?? 0);
        TerrainPoints = points;

        for (int i = 0; i < points.Length; i++)
        {
            ShapeController.spline.InsertPointAt(i, points[i].Position);
            if (i <= 0) SetLeadingEdgeTangent(points, i);
            else if (i >= points.Length - 1) SetTrailingEdgeTangent(points, i);
            else SetNonEdgeTangent(points, i);
        }

        Vector2 lastPoint = points[^1].Position;
        ShapeController.spline.InsertPointAt(points.Length, new Vector2(lastPoint.x, transform.position.y - m_floorDistance));
        ShapeController.spline.InsertPointAt(points.Length + 1, new Vector2(points[0].Position.x, transform.position.y - m_floorDistance));
        ShapeController.BakeMesh();
        ShapeController.RefreshSpriteShape();

        int newPointsStart = Mathf.Max(overlapPoints?.Length ?? 0, points.Length - m_overlapCount);
        int copyCount = points.Length - newPointsStart;
        OverlapPoints = new TerrainPointData[copyCount];
        float offsetX = transform.position.x;
        for (int i = 0; i < copyCount; i++)
        {
            TerrainPointData src = points[newPointsStart + i];
            OverlapPoints[i] = new(new(src.Position.x + offsetX, src.Position.y), src.IsValley);
        }
    }

    void SetNonEdgeTangent(TerrainPointData[] points, int index)
    {
        Vector2 tangent = ComputeTangent(points[index - 1].Position, points[index + 1].Position);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        ShapeController.spline.SetLeftTangent(index, -tangent);
        ShapeController.spline.SetRightTangent(index, tangent);
    }

    void SetLeadingEdgeTangent(TerrainPointData[] points, int index)
    {
        float mirrorXDistance = points[index + 1].Position.x - points[index].Position.x;
        Vector2 mirrorPoint = new(points[index].Position.x - mirrorXDistance, points[index].Position.y);
        Vector2 tangent = ComputeTangent(mirrorPoint, points[index + 1].Position);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        ShapeController.spline.SetLeftTangent(index, Vector2.down);
        ShapeController.spline.SetRightTangent(index, tangent);
    }

    void SetTrailingEdgeTangent(TerrainPointData[] points, int index)
    {
        float mirrorXDistance = points[index - 1].Position.x - points[index].Position.x;
        Vector2 mirrorPoint = new(points[index].Position.x - mirrorXDistance, points[index].Position.y);
        Vector2 tangent = ComputeTangent(points[index - 1].Position, mirrorPoint);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        ShapeController.spline.SetLeftTangent(index, -tangent);
        ShapeController.spline.SetRightTangent(index, Vector2.down);
    }

    TerrainPointData[] GenerateTerrainKeyPoints(TerrainPointData[] overlapPoints, int seed)
    {
        int overlapCount = overlapPoints?.Length ?? 0;
        TerrainPointData[] points = new TerrainPointData[overlapCount + m_terrainResolution];
        float localOffsetX = transform.position.x;

        if (overlapPoints != null)
        {
            for (int i = 0; i < overlapCount; i++)
            {
                TerrainPointData src = overlapPoints[i];
                points[i] = new(new(src.Position.x - localOffsetX, src.Position.y), src.IsValley);
            }
        }

        float currentY = overlapCount > 0 ? points[overlapCount - 1].Position.y : transform.position.y;
        float directionalSign = overlapPoints?[^1].IsValley ?? true ? -1f : 1f;

        Random.InitState(seed);
        for (int i = overlapCount; i < points.Length; i++)
        {
            float prevX = i > 0 ? points[i - 1].Position.x : 0f;
            Vector2 steps = new(Random.Range(m_minimums.x, m_maximums.x), Random.Range(m_minimums.y, m_maximums.y));
            float x = prevX + steps.x;
            float y = Mathf.Clamp(currentY + steps.y * directionalSign, m_minHeight, m_maxHeight);
            points[i] = new(new(x, y), directionalSign > 0f);
            currentY = y;
            directionalSign *= -1f;
        }
        return points;
    }

    Vector2 ComputeTangent(Vector2 previous, Vector2 next)
    {
        float slope = (next.y - previous.y) / (next.x - previous.x);
        float xStep = (next.x - previous.x) / 2f;
        return new(xStep * m_tangentScale, slope * m_tangentScale);
    }
}