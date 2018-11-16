using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author:xj
/// FileName:AudioManager.cs
/// Description:
/// Time:2016/10/14 9:29:11
/// </summary>
public class AudioManager : SingletonBehaviour<AudioManager>
{
    #region 公有属性
    public AudioSource mAudioMgr;
    public float BackgroundVolume = 1.0f;
    public float EffectVolume = 1.0f;
    #endregion

    #region 其他属性
    AudioClip mPlayClip;
    BackgroundMusic mCurMusic = BackgroundMusic.Music_None;
    Dictionary<SoundEffect, AudioClip> mEffectDict = null;
    Dictionary<BackgroundMusic, AudioClip> mMusicDict = null;
    List<GameObject> mCacheAudioSource = null;
    #endregion

    #region 公有函数
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="music"></param>
    public void PlayBG(BackgroundMusic music)
    {
        if (music != mCurMusic)
        {
            mPlayClip = GetMusicAudioClip(music);
            if (null == mPlayClip)
            {
                StopBG();
                return;
            }
            if (null == mAudioMgr)
            {
                GameObject go = new GameObject("BackgroundMusic");
                go.transform.localPosition = Vector3.zero;
                go.transform.parent = transform;

                // create the source
                mAudioMgr = go.AddComponent<AudioSource>();
                mAudioMgr.volume = BackgroundVolume;
                mAudioMgr.pitch = 1.0f;
                mAudioMgr.loop = true;
            }
            mAudioMgr.clip = mPlayClip;
            mAudioMgr.Play();
            mCurMusic = music;
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBG()
    {
        mAudioMgr.Stop();
        mCurMusic = BackgroundMusic.Music_None;
    }

    public AudioSource Play(SoundEffect effect)
    {
        return Play(effect, Vector3.zero, EffectVolume, 1.0f, false);
    }

    public AudioSource Play(SoundEffect clip, Transform emitter, bool loop)
    {
        return Play(clip, emitter, EffectVolume, 1f, loop);
    }

    public AudioSource Play(SoundEffect clip, Transform emitter, float volume, bool loop)
    {
        return Play(clip, emitter, volume, 1f, loop);
    }

    /// <summary>
    /// Plays a sound by creating an empty game object with an AudioSource
    /// and attaching it to the given transform (so it moves with the transform). 
    /// Destroys it after it finished playing if it dosen't loop.
    /// </summary>
    /// <param name='clip'>
    /// Clip.
    /// </param>
    /// <param name='emitter'>
    /// Emitter.
    /// </param>
    /// <param name='volume'>
    /// Volume.
    /// </param>
    /// <param name='pitch'>
    /// Pitch.
    /// </param>
    /// <param name='loop'>
    /// Loop.
    /// </param>
    public AudioSource Play(SoundEffect effect, Transform emitter, float volume, float pitch, bool loop)
    {
        AudioClip clip = GetEffectAudioClip(effect);
        if (null == clip)
        {
            return null;
        }

        AudioSource source = GetAudioSource(effect.ToString());
        GameObject go = source.gameObject;
        go.transform.position = emitter.position;
        go.transform.parent = emitter;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.playOnAwake = false;
        source.Play();
        if (!loop)
        {
            DestroyAudioGameObject(go, clip.length);
        }
        return source;
    }

    public AudioSource Play(SoundEffect clip, Vector3 point, bool loop)
    {
        return Play(clip, point, EffectVolume, 1f, loop);
    }

    public AudioSource Play(SoundEffect clip, Vector3 point, float volume, bool loop)
    {
        return Play(clip, point, volume, 1f, loop);
    }

    /// <summary>
    /// Plays a sound at the given point in space by creating an empty game object with an AudioSource
    /// in that place and destroys it after it finished playing if it dosen't loop.
    /// </summary>
    /// <param name='clip'>
    /// Clip.
    /// </param>
    /// <param name='point'>
    /// Point.
    /// </param>
    /// <param name='volume'>
    /// Volume.
    /// </param>
    /// <param name='pitch'>
    /// Pitch.
    /// </param>
    /// <param name='loop'>
    /// Loop.
    /// </param>
    public AudioSource Play(SoundEffect effect, Vector3 point, float volume, float pitch, bool loop)
    {
        AudioClip clip = GetEffectAudioClip(effect);
        if (null == clip)
        {
            return null;
        }
        AudioSource source = GetAudioSource(effect.ToString());
        GameObject go = source.gameObject;
        go.transform.parent = transform;
        go.transform.position = point;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.playOnAwake = false;
        source.Play();
        if (!loop)
        {
            DestroyAudioGameObject(go, clip.length);
        }
        return source;
    }
    /// <summary>
    /// 释放背景音乐
    /// </summary>
    public void ReleaseMusic()
    {
        if (null != mMusicDict)
        {
            if (BackgroundMusic.Music_None != mCurMusic)
            {
                AudioClip clip = mMusicDict[mCurMusic];
                mMusicDict.Clear();
                mMusicDict[mCurMusic] = clip;
            }
            else
            {
                mMusicDict.Clear();
            }
            Resources.UnloadUnusedAssets();
        }
    }
    /// <summary>
    /// 释放音效
    /// </summary>
    public void ReleaseEffect()
    {
        if (null != mEffectDict)
        {
            mEffectDict.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
    #endregion

    #region 其他函数
    void Awake()
    {
        gameObject.AddComponent<AudioListener>();
    }
    /// <summary>
    /// 获取音效
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    AudioClip GetEffectAudioClip(SoundEffect effect)
    {
        if (null != mEffectDict && mEffectDict.ContainsKey(effect))
        {
            return mEffectDict[effect];
        }
        AudioClip clip = Resources.Load("Sounds/Effect/" + effect) as AudioClip;
        if (null != clip)
        {
            if (null == mEffectDict)
            {
                mEffectDict = new Dictionary<SoundEffect, AudioClip>();
            }
            mEffectDict[effect] = clip;
        }
        return clip;
    }
    /// <summary>
    /// 获取背景音乐
    /// </summary>
    /// <param name="music"></param>
    /// <returns></returns>
    AudioClip GetMusicAudioClip(BackgroundMusic music)
    {
        if (null != mMusicDict && mMusicDict.ContainsKey(music))
        {
            return mMusicDict[music];
        }
        AudioClip clip = Resources.Load("Sounds/Music/" + music) as AudioClip;
        if (null != clip)
        {
            if (null == mMusicDict)
            {
                mMusicDict = new Dictionary<BackgroundMusic, AudioClip>();
            }
            mMusicDict[music] = clip;
        }
        return clip;
    }

    AudioSource GetAudioSource(string name)
    {
        GameObject obj = null;
        if (null != mCacheAudioSource && mCacheAudioSource.Count > 0)
        {
            obj = mCacheAudioSource[0];
            obj.SetActive(true);
            obj.name = name;
            mCacheAudioSource.RemoveAt(0);
        }
        else
        {
            obj = new GameObject(name);
        }
        AudioSource audioSource = obj.GetComponent<AudioSource>();
        if (null == audioSource)
        {
            audioSource = obj.AddComponent<AudioSource>();
        }
        return audioSource;
    }

    void DestroyAudioGameObject(GameObject obj, float sec)
    {
        SingletonBehaviour<ClientMain>.GetInst().WaitTimeInvoke(sec, delegate () {
            obj.SetActive(false);
            if (null == mCacheAudioSource)
            {
                mCacheAudioSource = new List<GameObject>();
            }
            mCacheAudioSource.Add(obj);
        });
    }
    #endregion
}

/// <summary>
/// 音效名字
/// </summary>
public enum SoundEffect
{
    /// <summary>
    /// 普通按钮音效名字
    /// </summary>
    Normal_Btn,
}

/// <summary>
/// 背景音乐名字
/// </summary>
public enum BackgroundMusic
{
    Music_None,
}