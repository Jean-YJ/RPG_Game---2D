using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Setting : MonoBehaviour
{
    private Player player;
    [SerializeField] private Toggle toggle_HealthBar;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixmultiplier = 25f;

    [Header("BGM Volume Setting")]
    [SerializeField] private Slider slider_BGM;
    [SerializeField] private string bgmParameter;

    [Header("SFX Volume Setting")]
    [SerializeField] private Slider slider_SFX;
    [SerializeField] private string sfxParameter;

    private void Start()
    {
        this.player = FindAnyObjectByType<Player>();

        this.toggle_HealthBar.onValueChanged.AddListener(OnHealthBarToggleChanged);
    }
    void OnEnable()
    {
        SettingData settingData = PlayerPrefsDataMgr.Instance.LoadData(typeof(SettingData), "SETTINGDATA") as SettingData;
        this.slider_BGM.value = settingData.bgmValue;
        this.slider_SFX.value = settingData.sfxValue;
    }
    public void LoadAudioSetting()
    {
        SettingData settingData = PlayerPrefsDataMgr.Instance.LoadData(typeof(SettingData), "SETTINGDATA") as SettingData;
        this.slider_BGM.value = settingData.bgmValue;
        this.slider_SFX.value = settingData.sfxValue;
    }

    private void OnHealthBarToggleChanged(bool isOn)
    {
        this.player.entity_Health.EnableHealthBar(isOn);
    }

    public void BackToMainMenu()
    {
        SaveManager.Instance.SaveGame();
        GameManager.Instance.ChangeScene("MainMenu", RespawnType.NonSpecific);
    }

    public void OnBGMSliderValueChanged(float value)
    {
        float newValue = Mathf.Log10(value) * this.mixmultiplier;
        this.audioMixer.SetFloat(this.bgmParameter, newValue);
    }

    public void OnSFXSliderValueChanged(float value)
    {
        float newValue = Mathf.Log10(value) * this.mixmultiplier;
        this.audioMixer.SetFloat(this.sfxParameter, newValue);
    }

    void OnDisable()
    {
        SettingData settingData = new SettingData();
        settingData.sfxValue = this.slider_SFX.value;
        settingData.bgmValue = this.slider_BGM.value;
        PlayerPrefsDataMgr.Instance.SaveData(settingData, "SETTINGDATA", true);
    }
}

public class SettingData
{
    public float bgmValue;
    public float sfxValue;
}
