using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, ISaveable
{
    private static GameManager instance;
    public static GameManager Instance => instance;
    private GameManager() { }

    private Vector3 lastPlayerPosition;
    private string lastScenePlayed;
    private bool isSaveDataLoaded = false;

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
    public void ContinuePlay()
    {
        ChangeScene(this.lastScenePlayed, RespawnType.NonSpecific);
    }

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        SaveManager.Instance.SaveGame();
        Time.timeScale = 1;
        StartCoroutine(ChangeSceneCo(sceneName, respawnType));
    }


    private IEnumerator ChangeSceneCo(string sceneName, RespawnType respawnType)
    {
        // 淡出渐变特效
        UI_FadeScreen fadeScreen = FindFadeScreenUI();
        fadeScreen.FadeOut();
        yield return fadeScreen.fadeEffCor;

        SceneManager.LoadScene(sceneName);
        this.isSaveDataLoaded = false;
        yield return null;
        while (this.isSaveDataLoaded == false)
        {
            yield return null;
        }
        fadeScreen = FindFadeScreenUI();
        fadeScreen.FadeIn();

        if (Player.Instance == null)
            yield break;
        Vector3 respawnPos = GetPlayerRespawnPosition(respawnType);
        if (respawnPos != Vector3.zero)
            Player.Instance.TeleportPlayerTo(respawnPos);
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI_CanvasRoot.Instance != null)
            return UI_CanvasRoot.Instance.fadeScreen;
        else
            return FindAnyObjectByType<UI_FadeScreen>();

    }

    private Vector3 GetWayPointPosition(RespawnType respawnType)
    {
        var allWayPoints = FindObjectsByType<Object_WayPoint>(FindObjectsInactive.Exclude);

        foreach (var point in allWayPoints)
        {
            if (point.GetRespawnType() == respawnType)
            {
                return point.GetRespwanPosAndSetTriggerFalse();
            }
        }

        return Vector3.zero;
    }

    private Vector3 GetPlayerRespawnPosition(RespawnType respawnType)
    {
        if (respawnType == RespawnType.Portal)
        {
            Object_Portal portal = Object_Portal.Instance;

            Vector3 position = portal.GetRespwanPosition();

            portal.SetTrigger(false);
            portal.DisableIfNeeded();

            return position;
        }

        if (respawnType == RespawnType.NonSpecific)
        {
            GameData data = SaveManager.Instance.GetGameData();

            //找到场景中所有的CheckPoint
            Object_CheckPoint[] allCheckPoints = FindObjectsByType<Object_CheckPoint>();
            // Where条件过滤掉没激活的。保留满足条件的数据，丢弃不满足条件的数据。
            // Select把一个对象转换成另一个对象。把Object_CheckPoint[]转换成Vector3[]
            // 最终得到场景中所有的已经激活的CheckPoint的位置
            var unlockedCheckPointsPosition = allCheckPoints
            .Where(cp => data.unlockedCheckPoints.TryGetValue(cp.GetCheckPointID(), out bool isUnlocked) && isUnlocked)
            .Select(cp => cp.GetRespawnPosition()).ToList();

            //找到场景中所有的WayPoint
            Object_WayPoint[] allWayPoints = FindObjectsByType<Object_WayPoint>();
            // Where条件过滤掉类型不是RespawnType.Enter的
            // Select把Object_WayPoint[]转换成Vector3[]
            // 最终得到场景中所有的类型为RespawnType.Enter的WayPoint的位置
            var enterWayPointsPosition = allWayPoints
            .Where(wp => wp.GetRespawnType() == RespawnType.Enter)
            .Select(wp => wp.GetRespwanPosAndSetTriggerFalse())
            .ToList();

            // 把的得到的两个列表拼接起来
            // Concat连接两个 IEnumerable 序列。不会修改原集合
            var filteredPosition = unlockedCheckPointsPosition.Concat(enterWayPointsPosition).ToList();

            if (filteredPosition.Count == 0)
                return Vector3.zero;

            //根据与上次死亡位置的距离进行升序排序
            filteredPosition.Sort((a, b) =>
            {
                return Vector3.Distance(this.lastPlayerPosition, a) > Vector3.Distance(this.lastPlayerPosition, b) ? 1 : -1;
            });

            return filteredPosition.First();

        }

        return GetWayPointPosition(respawnType);
    }

    // public void SetLastPlayerPosition(Vector3 position) => this.lastPlayerPosition = position;

    public void ReStartScene()
    {
        // SaveManager.Instance.SaveGame();

        string scenename = SceneManager.GetActiveScene().name;
        ChangeScene(scenename, RespawnType.NonSpecific);
    }

    public void LoadData(GameData data)
    {
        // throw new System.NotImplementedException();
        this.lastScenePlayed = data.lastScenePlayed;
        this.lastPlayerPosition = data.lastPlayerPosition;
        // Player.Instance.TeleportPlayerTo(this.lastPlayerPosition);

        if (string.IsNullOrEmpty(this.lastScenePlayed))
            this.lastScenePlayed = "Level_0";

        this.isSaveDataLoaded = true;
    }

    public void SaveData(ref GameData data)
    {
        // throw new System.NotImplementedException();
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenu")
            return;

        data.lastPlayerPosition = Player.Instance.transform.position;
        data.lastScenePlayed = currentSceneName;

        this.isSaveDataLoaded = false;
    }
}
