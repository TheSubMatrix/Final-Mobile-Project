using System;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteShapeController))]
public class TerrainSegment : MonoBehaviour
{
    public SpriteShapeController ShapeController{ get; private set;}
    [Range(3, 100), SerializeField] int m_terrainResolution = 50;
    [Range(1, 20), SerializeField] int m_overlapCount = 5;
    [SerializeField] Vector2 m_minimums = new(16, 6);
    [SerializeField] Vector2 m_maximums = new(34, 12);
    [SerializeField] float m_floorDistance = 30;
    [SerializeField] float m_maxHeight = 10;
    [SerializeField] float m_minHeight;
    [SerializeField] float m_tangentScale = 1 / 3f;

    public Vector2[] OverlapPoints { get; private set; } = Array.Empty<Vector2>();
    void OnEnable()
    {
        Initialize();
    }
    public void Initialize()
    {
        ShapeController = GetComponent<SpriteShapeController>();
    }

    public void GenerateTerrain(Vector2[] overlapPoints = null, int? seed = null)
    {
        if (ShapeController == null) return;
        ShapeController.spline.Clear();
        Vector2[] points = GenerateTerrainKeyPoints(overlapPoints, seed ?? 0);
        for (int i = 0; i < points.Length; i++)
        {
            ShapeController.spline.InsertPointAt(i, points[i]);
            if (i <= 0) SetLeadingEdgeTangent(points, i);
            else if (i >= points.Length - 1) SetTrailingEdgeTangent(points, i);
            else SetNonEdgeTangent(points, i);
        }
        
        Vector2 lastPoint = points[^1];
        ShapeController.spline.InsertPointAt(points.Length, new Vector2(lastPoint.x, transform.position.y - m_floorDistance));
        ShapeController.spline.InsertPointAt(points.Length + 1, new Vector2(points[0].x, transform.position.y - m_floorDistance));
        ShapeController.BakeMesh();
        ShapeController.RefreshSpriteShape();
        int newPointsStart = Mathf.Max(overlapPoints?.Length ?? 0, points.Length - m_overlapCount);
        int copyCount = points.Length - newPointsStart;
        OverlapPoints = new Vector2[copyCount];
        for (int i = 0; i < copyCount; i++)
            OverlapPoints[i] = new(points[newPointsStart + i].x + transform.position.x, points[newPointsStart + i].y);
    }
    void SetNonEdgeTangent(Vector2[] points, int index)
    {
        Vector2 tangent = ComputeTangent(points[index - 1], points[index + 1]);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        ShapeController.spline.SetLeftTangent(index, -tangent);
        ShapeController.spline.SetRightTangent(index, tangent);
    }
    void SetLeadingEdgeTangent(Vector2[] points, int index)
    {
        float mirrorXDistance = points[index + 1].x - points[index].x;
        Vector2 mirrorPoint = new(points[index].x - mirrorXDistance, points[index].y);
        Vector2 tangent = ComputeTangent(mirrorPoint, points[index + 1]);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        ShapeController.spline.SetLeftTangent(index, Vector2.down);
        ShapeController.spline.SetRightTangent(index, tangent);
    }
    void SetTrailingEdgeTangent(Vector2[] points, int index)
    {
        float mirrorXDistance = points[index - 1].x - points[index].x;
        Vector2 mirrorPoint = new(points[index].x - mirrorXDistance, points[index].y);
        Vector2 tangent = ComputeTangent(points[index - 1], mirrorPoint);
        ShapeController.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        ShapeController.spline.SetLeftTangent(index, -tangent);
        ShapeController.spline.SetRightTangent(index, Vector2.down);
    }
    Vector2[] GenerateTerrainKeyPoints(Vector2[] overlapPoints, int seed)
    {
        int overlapCount = overlapPoints?.Length ?? 0;
        Vector2[] points = new Vector2[overlapCount + m_terrainResolution];
        float localOffsetX = transform.position.x;
        if (overlapPoints != null)
        {
            for (int i = 0; i < overlapCount; i++)
                points[i] = new(overlapPoints[i].x - localOffsetX, overlapPoints[i].y);
        }

        float currentYPosition = overlapCount > 0 ? points[overlapCount - 1].y : transform.position.y;
        float directionalSign = overlapCount switch
        {
            >= 2 => points[overlapCount - 1].y > points[overlapCount - 2].y ? -1f : 1f,
            _ => 1f
        };

        Random.InitState(seed);
        for (int i = overlapCount; i < points.Length; i++)
        {
            Vector2 prev = i > 0 ? points[i - 1] : Vector2.zero;
            Vector2 steps = new(Random.Range(m_minimums.x, m_maximums.x), Random.Range(m_minimums.y, m_maximums.y));
            points[i].x = prev.x + steps.x;
            points[i].y = Mathf.Clamp(currentYPosition + steps.y * directionalSign, m_minHeight, m_maxHeight);
            currentYPosition = points[i].y;
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