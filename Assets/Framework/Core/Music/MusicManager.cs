using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音频管理器 背景音乐和音效合并
/// </summary>
public class MusicManager : Singleton<MusicManager>
{
    private MusicManager() {
        //音效自动停止检测
        Mono.Instance.onFixedUpdate += SoundFixedUpdate;
    }
    //背景音乐 用一个过场景不销毁的对象管理
    private AudioSource BKMusicComponent = null;
    private float BKMusicOldVolume = 0.1f;
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name">背景音乐的文件名</param>
    public void PlayBKMusic(string name)
    {
        if(BKMusicComponent == null)
        {
            GameObject obj = new GameObject("MusicManager");
            GameObject.DontDestroyOnLoad(obj);//设置过场景不销毁
            BKMusicComponent = obj.AddComponent<AudioSource>();
        }
        //加载音频并且播放
        ABResManager.Instance.LoadResAsync<AudioClip>("music", $"{name}.mp3", (clip) =>
        {
            Debug.Log("加载成功"+clip);
            BKMusicComponent.clip = clip;
            BKMusicComponent.loop = true;
            BKMusicComponent.Play();
        },false);
    }
    public void StopBKMusic()
    {
        if (BKMusicComponent)
            BKMusicComponent.Stop();//从头播放
    }
    public void PauseBKMusic()
    {
        if (BKMusicComponent)
            BKMusicComponent.Pause();//接着播放
    }
    /// <summary>
    /// 修改背景音量大小
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeBKMusicVolume(float volume)
    {
        if(BKMusicOldVolume != volume)
        {
            BKMusicOldVolume = volume;
            BKMusicComponent.volume = volume;
        }
    }
    //音效管理
    private List<AudioSource> soundList = new List<AudioSource>();//存放当前音效的容器
    private bool IsSoundStop = false;//是否处于音效暂停情况

    /// <summary>
    /// 播放某个音效
    /// </summary>
    /// <param name="name"></param>
    /// <param name="loop"></param>
    /// <param name="callback"></param>
    public void PlaySound(string name,bool loop = false,UnityAction<AudioSource> callback = null)
    {
        ABResManager.Instance.LoadResAsync<AudioClip>("music", $"{name}.mp3", (clip) =>
        {
            AudioSource audioSource = PoolManager.Instance.getObject("SoundObj").GetComponent<AudioSource>();
            //处理如果是超出缓冲池最大数量获取到的对象
            audioSource.Stop();

            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
            if (!soundList.Contains(audioSource))
            {
                //如果是超出缓冲池最大数量获取到的对象 这个对象已经在list里了 就不用添加
                soundList.Add(audioSource);
            }
            //如果是长效音效 将音效组件返回
            callback?.Invoke(audioSource);
        }, true);
    }
    /// <summary>
    /// 检测音效是否播放完成自动消除
    /// </summary>
    private void SoundFixedUpdate()
    {
        for(int i = soundList.Count - 1; i >= 0; i--)
        {
            if (IsSoundStop) break;
            if (soundList[i].isPlaying == false)
            {
                soundList[i].clip = null;
                PoolManager.Instance.putObject("SoundObj",soundList[i].gameObject);
                soundList.Remove(soundList[i]);
            }
        }
    }
    /// <summary>
    /// 暂停循环播放的音效 也就是该音效需要被消除了 而不是单单停止
    /// </summary>
    /// <param name="audioSource"></param>
    public void StopSound(AudioSource audioSource)
    {
        if (soundList.Contains(audioSource))
        {
            audioSource.Stop();
            audioSource.clip = null;
            PoolManager.Instance.putObject("SoundObj", audioSource.gameObject);
            soundList.Remove(audioSource);
        }
    }
    /// <summary>
    /// 修改所有音效的音量大小
    /// </summary>
    /// <param name="volume"></param>
    public void ChangeSoundVolume(float volume)
    {
        for(int i = 0; i < soundList.Count; i++)
        {
            soundList[i].volume = volume;
        }
    }
    /// <summary>
    /// 设置所有的音效的播放或者暂停
    /// </summary>
    /// <param name="IsPlay">是否播放</param>
    public void StopOrPlaySound(bool IsPlay)
    {
        if (IsPlay)
        {
            //播放
            IsSoundStop = false;
            for (int i = 0; i < soundList.Count; i++)
            {
                soundList[i].Play();
            }
        }
        else
        {
            //停止
            IsSoundStop = true;
            for (int i = 0; i < soundList.Count; i++)
            {
                soundList[i].Pause();
            }
        }
    }
    /// <summary>
    /// 用于在切换场景的时候清除引用 在PoolManagerClear之前调用 因为有对PoolManager的调用
    /// </summary>
    public void Clean()
    {
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].Stop();
            soundList[i].clip = null;
        }
        soundList.Clear();
    }
}
