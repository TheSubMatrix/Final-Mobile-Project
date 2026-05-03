using JetBrains.Annotations;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using MatrixUtils.GenericDatatypes;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour, IScoreReaderWriter, IDependencyProvider
{
    [Provide, UsedImplicitly]
    IScoreWriter GetScoreWriter() => this;
    
    [Provide, UsedImplicitly]
    IScoreReader GetHighScoreReader() => this;
    
    [Provide, UsedImplicitly]
    IScoreReaderWriter GetScoreReaderWriter() => this;
    
    [SerializeReference, ClassSelector] IScoreRepository m_repository;
    
    readonly Observer<ScoreData> m_currentHighScore = new(new());
    readonly Observer<ScoreData> m_latestScore = new(new());
    readonly Observer<ScoreData> m_currentScore = new(new());
    void Awake()
    {
        SavedScoreInformation scoreInformation = m_repository.Load();
        m_currentHighScore.Value = scoreInformation.HighScore;
        m_latestScore.Value = scoreInformation.LatestScore;
    }

    public void UpdateDistance(float distance)
    {
        ScoreData data = m_currentScore.Value;
        data.Distance = distance;
        m_currentScore.Value = data;
        m_currentScore.Notify();
    }

    public void UpdateExtraPoints(uint extraPoints)
    {
        ScoreData data = m_currentScore.Value;
        data.PickupPoints = extraPoints;
        m_currentScore.Value = data;
        m_currentScore.Notify();
    }

    public void UpdateBonusPoints(uint bonusPoints)
    {
        ScoreData data = m_currentScore.Value;
        data.BonusPoints = bonusPoints;
        m_currentScore.Value = data;
        m_currentScore.Notify();
    }

    public void CommitScore()
    {
        if (m_currentScore.Value.Total > m_currentHighScore.Value.Total)
        {
            m_currentHighScore.Value = m_currentScore;
            m_currentHighScore.Notify();
        }
        m_latestScore.Value = m_currentScore;
        m_latestScore.Notify();
        SavedScoreInformation scoreInformation = new(m_currentHighScore.Value, m_latestScore.Value);
        m_repository.Save(scoreInformation);
        ResetCurrentScore();
    }

    public void ResetCurrentScore()
    {
        m_currentScore.Value = new();
    }
    
    public ScoreData GetHighScore() => m_currentHighScore;
    public UnityEvent<ScoreData> OnHighScoreUpdated => m_currentHighScore.GetUnderlyingUnityEvent();
    public ScoreData GetCurrentScore() => m_currentScore;
    public UnityEvent<ScoreData> OnCurrentScoreUpdated => m_currentScore.GetUnderlyingUnityEvent();
    public ScoreData GetLatestScore() => m_latestScore;
    public UnityEvent<ScoreData> OnLatestScoreUpdated => m_latestScore.GetUnderlyingUnityEvent();

    [ContextMenu("Reset Current Score")]
    public void ResetAllScores()
    {
        m_currentScore.Value = new();
        m_latestScore.Value = new();
        m_currentHighScore.Value = new();
        m_currentHighScore.Notify();
        m_latestScore.Notify();
        m_currentScore.Notify();
        SavedScoreInformation scoreInformation = new(m_currentHighScore.Value, m_latestScore.Value);
        m_repository.Save(scoreInformation);
    }
}