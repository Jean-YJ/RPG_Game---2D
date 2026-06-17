using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    void Start()
    {
        transform.root.GetComponentInChildren<UI_Setting>(true).LoadAudioSetting();
        transform.root.GetComponentInChildren<UI_FadeScreen>().FadeIn();
        AudioManager.Instance.StartBGM("playList_MainMenu");
    }
    public void PlayGame()
    {
        AudioManager.Instance.PlayGlobalSFX("btn_click");
        GameManager.Instance.ContinuePlay();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
