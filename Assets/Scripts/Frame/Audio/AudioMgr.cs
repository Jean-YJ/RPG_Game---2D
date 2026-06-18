using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 音乐音效管理器
/// </summary>
public class AudioMgr : BaseSingleton<AudioMgr>
{
    private AudioMgr() 
    {
        MonoMgr.Instance.AddUpdateAction(Update);
    }

    //背景音乐播放组件
    private AudioSource bgm = null;
    //背景音乐音量
    public float volume = 0.3f;

    //存储正在播放的音效的容器，便于后续管理
    private List<AudioSource> sounds = new List<AudioSource>();
    //音效音量
    public float soundVolume = 0.3f;

    private bool isPause = false;

    private void Update()
    {
        if(this.isPause)
            return;

        //不停的遍历容器 检测有没有音效播放完毕 播放完了 就移除销毁它
        //为了避免边遍历边移除出问题 我们采用逆向遍历
        for (int i = sounds.Count - 1; i >= 0; i--)
        {
            if (!sounds[i].isPlaying)
            {
                //GameObject.Destroy(sounds[i]);
                //音效播放完毕了 不再使用了 我们将这个音效切片置空
                sounds[i].clip = null;
                PoolMgr.Instance.PushGameObject(sounds[i].gameObject);
                sounds.RemoveAt(i);

            }
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="musicName">背景音乐名称</param>
    public void PlayBGM(string musicName)
    {
        //动态创建播放背景音乐的组件 并且 不会过场景移除 
        //保证背景音乐在过场景时也能播放
        if (bgm == null)
        {
            GameObject obj = new GameObject("BGM");
            bgm = obj.AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(obj);
        }
        AddressablesMgr.Instance.LoadAssetAsync<AudioClip>(musicName, (handler) =>
        {
            bgm.clip = handler.Result;
            bgm.loop = true;
            bgm.Play();
        });
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopBGM() 
    {
        if (bgm == null)
            return;
        bgm.Stop();
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (bgm == null)
            return;
        bgm.Pause();
    }
    /// <summary>
    /// 暂停后恢复播放
    /// </summary>
    public void RecoverPlayBGM()
    {
        if (bgm == null)
            return;
        bgm.Play();
    }

    /// <summary>
    /// 设置背景音乐音量大小
    /// </summary>
    /// <param name="volume"></param>
    public void SetBGMVolume(float volume)
    {
        if (bgm == null)
            return;
        this.volume = volume;
        bgm.volume = this.volume;
    }

    /// <summary>
    /// 背景音乐静音或取消静音
    /// </summary>
    /// <param name="status">true表示静音，false表示取消静音</param>
    public void MuteBGM(bool status)
    {
        if (bgm == null)
            return;
        this.bgm.mute = status;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    /// <param name="isLoop">是否循环播放</param>
    /// <param name="callback">回调函数</param>
    public void PlaySound(string soundName, bool isLoop = false, Action<AudioSource> callback = null)
    {
        AddressablesMgr.Instance.LoadAssetAsync<AudioClip>(soundName, (handler) =>
        {
            if (handler.Result != null)
            {
                //从缓存池中取出音效对象得到对应组件
                AudioSource audio = PoolMgr.Instance.GetGameObject("Sound").GetComponent<AudioSource>();
                //如果取出来的音效是之前正在使用的 我们先停止它
                if (!sounds.Contains(audio))
                    audio.Stop();

                audio.clip = handler.Result;
                audio.loop = isLoop;
                audio.volume = this.soundVolume;
                audio.Play();

                //由于从缓存池中取出对象 有可能取出一个之前正在使用的（超上限时）
                //所以我们需要判断 容器中没有记录再去记录 不要重复去添加即可
                if (!sounds.Contains(audio))
                    sounds.Add(audio);
                callback?.Invoke(audio);
            }
        });
    }
    /// <summary>
    /// 停止播放音效
    /// </summary>
    /// <param name="audio">音效组件对象</param>
    public void StopSound(AudioSource audio) 
    {
        if (sounds.Contains(audio))
        {
            audio.Stop();
            sounds.Remove(audio);

            audio.clip = null;
            PoolMgr.Instance.PushGameObject(audio.gameObject);
        }
    }
    /// <summary>
    /// 改变音效大小
    /// </summary>
    /// <param name="volume"></param>
    public void SetSoundVolume(float volume)
    {
        this.soundVolume = volume;
        foreach (var sound in sounds)
            sound.volume = this.soundVolume;
    }

    /// <summary>
    /// 继续播放或者暂停所有音效
    /// </summary>
    /// <param name="status">是否是继续播放 true为播放 false为暂停</param>
    public void PlayOrPauseAll(bool status)
    {
        if(status)
        {
            this.isPause = false;
            foreach (var sound in sounds)
                sound?.Play();
        }
        else
        {
            this.isPause = true;
            foreach (var sound in sounds)
                sound?.Pause();
        }
    }

    /// <summary>
    /// 清空音效相关记录 过场景时在清空缓存池之前去调用它
    /// 重要的事情说三遍！！！
    /// 过场景时在清空缓存池之前去调用它
    /// 过场景时在清空缓存池之前去调用它
    /// 过场景时在清空缓存池之前去调用它
    /// </summary>
    public void ClearSounds()
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            sounds[i].Stop();
            sounds[i].clip = null;
            PoolMgr.Instance.PushGameObject(sounds[i].gameObject);
        }
        sounds.Clear();
    }
}
