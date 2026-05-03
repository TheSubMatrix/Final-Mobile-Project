using System;
using UnityEngine;

[Serializable]
public struct ScoreData
{
    public float Distance;
    public uint BonusPoints;
    public uint PickupPoints;

    public ScoreData(float distance = 0, uint bonusPoints = 0, uint pickupPoints = 0)
    {
        Distance = distance;
        PickupPoints = pickupPoints;
        BonusPoints = bonusPoints;
    }
    public uint Total => (uint)(Mathf.RoundToInt(Mathf.Abs(Distance)) + BonusPoints + PickupPoints);
}
[Serializable]
public class SavedScoreInformation
{
    public ScoreData HighScore;
    public ScoreData LatestScore;

    public SavedScoreInformation(ScoreData highScore, ScoreData latestScore)
    {
        HighScore = highScore;
        LatestScore = latestScore;
    }
    public SavedScoreInformation()
    {
        HighScore = new();
        LatestScore = new();
    }
}