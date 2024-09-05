using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

public class BD1SoundManager : MonoBehaviour
{
    // 0. 싱글톤 적용
    public static BD1SoundManager instance = null;
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        BGMPlayer = transform.GetChild(0).GetComponent<AudioSource>();
        SFXPlayer = transform.GetChild(1).GetComponents<AudioSource>();
    }

    // 1. 변수 모음
    [Header("Audio Clip")]
    [SerializeField] private Sound[] BGM;
    [SerializeField] private Sound[] SFX;
    [Header("AudioSource")]
    [SerializeField] public AudioSource BGMPlayer;
    [SerializeField] private AudioSource[] SFXPlayer;

    // 2. 메서드 모음
    public void PlayBGM(string name)
    {
        foreach (Sound s in BGM)
        {
            if (s.name.Equals(name))
            {
                BGMPlayer.clip = s.clip;
                BGMPlayer.Play();
                return;
            }
        }
        Debug.Log($"PlayBGM {name} 없음");
    }

    public bool BGMisPlaying()
    {
        return BGMPlayer.isPlaying;
    }

    public void StopBGM()
    {
        BGMPlayer.Stop();
    }

    public void PlaySFX(string name)
    {
        foreach (Sound s in SFX)
        {
            if (s.name.Equals(name))
            {
                for (int i = 0; i < SFXPlayer.Length; i++)
                {
                    if (!SFXPlayer[i].isPlaying)
                    {
                        SFXPlayer[i].clip = s.clip;
                        SFXPlayer[i].Play();
                        return;
                    }
                }
                Debug.Log("모든 SFX플레이어가 사용 중임");
                return;
            }
        }
        Debug.Log($"PlaySFX {name} 없음");
    }


    public void StopSFX()
    {
        for (int i = 0; i < SFXPlayer.Length; i++)
        {
            SFXPlayer[i].Stop();
        }
    }
}
