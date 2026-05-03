public interface IScoreWriter
{
    void UpdateDistance(float distance);
    void UpdateExtraPoints(uint extraPoints);
    void UpdateBonusPoints(uint bonusPoints);
    void CommitScore();
    void ResetCurrentScore();
}