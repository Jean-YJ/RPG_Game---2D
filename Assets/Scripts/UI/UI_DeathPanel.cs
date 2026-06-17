using UnityEngine;

public class UI_DeathPanel : MonoBehaviour
{
    public void GoToTown()
    {
        GameManager.Instance.ChangeScene("Level_0", RespawnType.NonSpecific);
    }

    public void GoToCheckPoint()
    {
        GameManager.Instance.ReStartScene();
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.ChangeScene("MainMenu", RespawnType.NonSpecific);

    }
}
