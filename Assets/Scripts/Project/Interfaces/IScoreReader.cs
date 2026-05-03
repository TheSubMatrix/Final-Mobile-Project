
using UnityEngine.Events;

public interface IScoreReader
{
    ScoreData GetHighScore();
    UnityEvent<ScoreData> OnHighScoreUpdated { get; }
    ScoreData GetCurrentScore();
    UnityEvent<ScoreData> OnCurrentScoreUpdated { get; }
    ScoreData GetLatestScore();
    UnityEvent<ScoreData> OnLatestScoreUpdated { get; }
}
