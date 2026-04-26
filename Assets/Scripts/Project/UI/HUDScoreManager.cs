using System;
using MatrixUtils.GenericDatatypes;
using UnityEngine;
using UnityEngine.Events;
[Serializable]
public class HUDScoreManager : IScoreManager
{

    [field:SerializeField] public Observer<uint> Score { get; private set; }
    [field:SerializeField] public UnityEvent<uint> OnBonusEarned { get; private set; }
    public void AddScore(uint scoreToAdd)
    {
        Score.Value += scoreToAdd;
    }

    public void AddBonus(uint scoreToAdd)
    {
        OnBonusEarned?.Invoke(scoreToAdd);
        AddScore(scoreToAdd);
    }

    public void RemoveScore(uint scoreToRemove)
    {
        Score.Value = scoreToRemove > Score ? 0 : Score - scoreToRemove;
    }

    public void ClearCurrentScore()
    {
        Score.Value = 0;
    }
}
