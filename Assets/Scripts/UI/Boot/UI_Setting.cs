using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Setting : MonoBehaviour, IUIView
{
    [SerializeField] private Toggle toggle_HealthBar;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixmultiplier = 25f;

    [Header("BGM Volume Setting")]
    [SerializeField] private Slider slider_BGM;
    [SerializeField] private string bgmParameter;

    [Header("SFX Volume Setting")]
    [SerializeField] private Slider slider_SFX;
    [SerializeField] private string sfxParameter;

    private UI_SettingPresenter presenter;

    public float BGMValue => this.slider_BGM != null ? this.slider_BGM.value : 0f;
    public float SFXValue => this.slider_SFX != null ? this.slider_SFX.value : 0f;
    public AudioMixer AudioMixer => this.audioMixer;
    public float MixMultiplier => this.mixmultiplier;
    public string BGMParameter => this.bgmParameter;
    public string SFXParameter => this.sfxParameter;

    private void Awake()
    {
        EnsurePresenter();
    }

    private void Start()
    {
        if (this.toggle_HealthBar != null)
            this.toggle_HealthBar.onValueChanged.AddListener(OnHealthBarToggleChanged);
    }

    void OnEnable()
    {
        LoadAudioSetting();
    }

    public void LoadAudioSetting()
    {
        EnsurePresenter();
        this.presenter.LoadSettingData();
    }

    public void SetAudioSliderValues(float bgmValue, float sfxValue)
    {
        if (this.slider_BGM != null)
            this.slider_BGM.value = bgmValue;

        if (this.slider_SFX != null)
            this.slider_SFX.value = sfxValue;
    }

    private void OnHealthBarToggleChanged(bool isOn)
    {
        EnsurePresenter();
        this.presenter.SetHealthBarVisible(isOn);
    }

    public void BackToMainMenu()
    {
        EnsurePresenter();
        this.presenter.BackToMainMenu();
    }

    public void OnBGMSliderValueChanged(float value)
    {
        EnsurePresenter();
        this.presenter.ApplyBGMVolume(value);
    }

    public void OnSFXSliderValueChanged(float value)
    {
        EnsurePresenter();
        this.presenter.ApplySFXVolume(value);
    }

    //确保绑定了代理
    private void EnsurePresenter()
    {
        if (this.presenter != null)
            return;

        this.presenter = new UI_SettingPresenter();
        this.presenter.Attach(this);
    }

    void OnDisable()
    {
        EnsurePresenter();
        this.presenter.SaveSettingData();
    }

    private void OnDestroy()
    {
        if (this.toggle_HealthBar != null)
            this.toggle_HealthBar.onValueChanged.RemoveListener(OnHealthBarToggleChanged);

        this.presenter?.Detach();
    }
}

public class SettingData
{
    public float bgmValue = 1;
    public float sfxValue = 1;
}
