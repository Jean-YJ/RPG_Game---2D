using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private string musicGroupName;

    void Start()
    {
        AudioManager.Instance.StartBGM(this.musicGroupName);
    }
}
