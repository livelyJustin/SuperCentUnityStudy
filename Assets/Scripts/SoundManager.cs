using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public enum SoundList
{
    Shot = 0,
    Reload,
    Blood,
    Explosion
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance; // get만 가능하게 보호
    public static SoundManager Instance { get { return instance; } }
    [SerializeField] private AudioSource soundMg_source;
    [SerializeReference] private List<AudioClip> soundList = new List<AudioClip>();
    private Dictionary<SoundList, AudioClip> dic_Sound = new Dictionary<SoundList, AudioClip>(); // 기존에는 키 값으로 스트링을 사용 했으나 
    //휴먼에러나 퍼포먼스로 스트링 보다는 다른 타입을 사용하는게 좋음

    private void Awake()
    {
        if (instance == null)
            instance = this;

        // 싱글톤 예외 처리
    }

    private void Start()
    {
        Init();
    }


    void Init()
    {
        int count = 0;
        foreach (SoundList item in Enum.GetValues(typeof(SoundList)))
        {
            dic_Sound.Add(item, soundList[count]);
            count++;
        }
    }

    public void PlaySound(SoundList name)
    {
        if (!dic_Sound.ContainsKey(name))
        {
            Debug.Log("사운드를 추가해주세요.");
            return;
        }
        soundMg_source.clip = GetSoundClip(name);
        soundMg_source.Play();
    }

    public void PlayOneShot(SoundList name)
    {
        if (!dic_Sound.ContainsKey(name))
        {
            Debug.Log("사운드를 추가해주세요.");
            return;
        }
        soundMg_source.clip = GetSoundClip(name);
        soundMg_source.PlayOneShot(soundMg_source.clip);
    }

    AudioClip GetSoundClip(SoundList name) // 널체크
    {
        if (!dic_Sound.ContainsKey(name))
            throw new Exception("Get Clip을 할 수 없습니다.");

        dic_Sound.TryGetValue(name, out var clip);
        return clip;
    }
}
