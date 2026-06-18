using UnityEngine;

public class UI_SettingPresenter : UIPresenterBase<UI_Setting>
{
    private const string SettingDataKey = "SETTINGDATA";
    private const string MainMenuSceneName = "MainMenu";
    private const float MinMixerSliderValue = 0.0001f;

    private Player player;

    public override void Attach(UI_Setting view)
    {
        base.Attach(view);
        player = Object.FindAnyObjectByType<Player>();
    }

    public void LoadSettingData()
    {
        if (!HasView)
            return;

        SettingData loadedData = PlayerPrefsDataMgr.Instance.LoadData(typeof(SettingData), SettingDataKey) as SettingData;
        SettingData settingData = loadedData?? new SettingData();
        View.SetAudioSliderValues(settingData.bgmValue, settingData.sfxValue);
        ApplyBGMVolume(settingData.bgmValue);
        ApplySFXVolume(settingData.sfxValue);
    }

    public void SaveSettingData()
    {
        if (!HasView)
            return;

        SettingData settingData = new SettingData
        {
            bgmValue = View.BGMValue,
            sfxValue = View.SFXValue
        };

        PlayerPrefsDataMgr.Instance.SaveData(settingData, SettingDataKey, true);
    }


    /// <summary>
    /// 设置Player头顶血条的可见状态
    /// </summary>
    /// <param name="isOn">是否可见</param>
    public void SetHealthBarVisible(bool isOn)
    {
        if (player == null)
            player = Object.FindAnyObjectByType<Player>();

        if (player == null || player.entity_Health == null)
            return;

        player.entity_Health.EnableHealthBar(isOn);
    }

    /// <summary>
    /// 返回菜单场景
    /// </summary>
    public void BackToMainMenu()
    {
        SaveManager.Instance.SaveGame();
        GameManager.Instance.ChangeScene(MainMenuSceneName, RespawnType.NonSpecific);
    }

    public void ApplyBGMVolume(float value)
    {
        ApplyMixerVolume(View?.BGMParameter, value);
    }

    public void ApplySFXVolume(float value)
    {
        ApplyMixerVolume(View?.SFXParameter, value);
    }



    //修改AudioMixer的参数，实现音量大小的控制
    private void ApplyMixerVolume(string parameter, float value)
    {
        if (!HasView || View.AudioMixer == null || string.IsNullOrEmpty(parameter))
            return;

        float safeValue = Mathf.Max(value, MinMixerSliderValue);
        float mixerValue = Mathf.Log10(safeValue) * View.MixMultiplier;
        View.AudioMixer.SetFloat(parameter, mixerValue);
    }
}
