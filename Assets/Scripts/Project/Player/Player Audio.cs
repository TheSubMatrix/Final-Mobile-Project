using System.Collections.Generic;
using AudioSystem;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] List<SoundData> m_sounds;
    public void Play()
    {
        if(Random.Range(0, 100) < 50) return;
        SoundData sound = m_sounds[Random.Range(0, m_sounds.Count)];
        SoundManager.Instance.CreateSound().WithSoundData(sound).WithRandomPitch().AttachedTo(transform).Play();
    }
}
