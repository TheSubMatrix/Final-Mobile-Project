public interface IScoreRepository
{
    SavedScoreInformation Load();
    void Save(SavedScoreInformation score);
}