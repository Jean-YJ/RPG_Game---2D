using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance => instance;
    private AudioManager() { }

    [SerializeField] private AudioDataBase_SO audioDataBase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    private AudioClip lastMuiscPlayed;

    [Space]
    private Transform player;

    private string currentBGMGroupName;
    private Coroutine currentBGMCor;
    private bool isBGMPlaying;


    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (!this.bgmSource.isPlaying && this.isBGMPlaying)
        {
            if (!string.IsNullOrEmpty(this.currentBGMGroupName))
                NextBGM(this.currentBGMGroupName);
        }
        if (this.bgmSource.isPlaying && !this.isBGMPlaying)
            StopBGM();
    }

    public void PlaySFX(string sfxName, AudioSource sfxSource, float minDistanceToHearSound = 5f)
    {
        if (this.player == null)
            this.player = Player.Instance.transform;

        var audioData = this.audioDataBase.GetAudioClipData(sfxName);
        if (audioData != null)
        {
            Debug.Log("Attempt to play sound:" + sfxName);
        }
        else
        {
            Debug.Log("Failed to play sound:" + sfxName);
            return;
        }

        var clip = audioData.GetRandomClip();
        if (clip == null)
            return;

        float maxVolume = audioData.maxVolume;
        float distance = Vector2.Distance(sfxSource.transform.position, this.player.position);
        float t = Mathf.Clamp01(1 - (distance / minDistanceToHearSound));

        sfxSource.pitch = Random.Range(0.85f, 1.15f);
        sfxSource.clip = clip;
        sfxSource.volume = Mathf.Lerp(0, maxVolume, t * t);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayGlobalSFX(string sfxName)
    {
        var audioData = this.audioDataBase.GetAudioClipData(sfxName);
        if (audioData == null)
            return;

        var clip = audioData.GetRandomClip();
        if (clip == null)
            return;

        this.sfxSource.pitch = Random.Range(0.85f, 1.15f);
        this.sfxSource.clip = clip;
        this.sfxSource.volume = audioData.maxVolume;
        this.sfxSource.PlayOneShot(clip);
    }

    public void StartBGM(string musicGroup)
    {
        this.isBGMPlaying = true;

        if (musicGroup == this.currentBGMGroupName)
            return;

        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)
    {
        this.isBGMPlaying = true;
        this.currentBGMGroupName = musicGroup;

        if (this.currentBGMCor != null)
            StopCoroutine(this.currentBGMCor);

        this.currentBGMCor = StartCoroutine(SwitchBGMTo(musicGroup));
    }

    public void StopBGM()
    {
        this.isBGMPlaying = false;

        StartCoroutine(FadeVolumeCor(this.bgmSource, 0f, 1f));
        if (this.currentBGMCor != null)
            StopCoroutine(this.currentBGMCor);
    }

    private IEnumerator SwitchBGMTo(string musicGroup)
    {
        AudioClipData clipData = this.audioDataBase.GetAudioClipData(musicGroup);
        AudioClip nextMusic = clipData.GetRandomClip();
        if (clipData == null || clipData.clips.Count == 0)
        {

            yield break;
        }

        //当有多个音乐时，一直随机直到音乐与上一个不重复
        if (clipData.clips.Count > 1)
        {
            while (nextMusic == lastMuiscPlayed)
                nextMusic = clipData.GetRandomClip();
        }

        //停止当前的音乐
        if (this.bgmSource.isPlaying)
            yield return FadeVolumeCor(this.bgmSource, 0f, 1f);

        this.lastMuiscPlayed = nextMusic;
        this.bgmSource.clip = nextMusic;
        this.bgmSource.volume = 0f;
        this.bgmSource.Play();

        StartCoroutine(FadeVolumeCor(this.bgmSource, clipData.maxVolume, 1f));
    }

    private IEnumerator FadeVolumeCor(AudioSource audioSource, float targetVolume, float duration)
    {
        float timer = 0;
        float startVolume = audioSource.volume;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}
