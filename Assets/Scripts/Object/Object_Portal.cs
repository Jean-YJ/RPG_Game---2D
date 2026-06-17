using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_Portal : Object_Interact, ISaveable, IInteractable
{
    private static Object_Portal instance;
    public static Object_Portal Instance => instance;
    private Object_Portal() { }

    public bool isActive { get; private set; }
    [SerializeField] private Vector3 defaultPpsition; //Portal出现在Town中的位置
    [SerializeField] private string townSceneName = "Level_0"; //Town所在的场景的名称

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTrigger;

    private string currentSceneName;
    private string returnedSceneName;
    private bool isReturningFromTown;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        this.currentSceneName = SceneManager.GetActiveScene().name;
        this.gameObject.SetActive(false); // 隐藏起来

    }



    public void ActivatePortal(Vector3 position, int facingDir = 1)
    {
        this.gameObject.SetActive(true);
        this.isActive = true;
        this.transform.position = position;
        SaveManager.Instance.GetGameData().inScenePortals.Clear();

        if (facingDir == -1)
            this.transform.Rotate(0, 180, 0);
    }
    private void UseTeleport()
    {
        string destinationScene = InTown() ? this.returnedSceneName : this.townSceneName;
        GameManager.Instance.ChangeScene(destinationScene, RespawnType.Portal);
    }

    private bool InTown() => this.currentSceneName == this.townSceneName;
    public void SetTrigger(bool status) => this.canBeTrigger = status;
    public Vector3 GetRespwanPosition() => this.respawnPoint == null ? this.transform.position : this.respawnPoint.position;

    public void DisableIfNeeded()
    {
        // 从Level返回Town后，传送门应该出现在defaultPoint，不应该禁用
        if (!isReturningFromTown)
            return;

        SaveManager.Instance.GetGameData().inScenePortals.Remove(this.currentSceneName);
        // 从Town返回Level后，传送门应该消失掉
        this.isActive = false;
        SetTrigger(true);
        this.gameObject.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);


    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        this.canBeTrigger = true;
    }

    public void LoadData(GameData data)
    {
        // throw new System.NotImplementedException();
        if (InTown() && data.inScenePortals.Count > 0)
        {
            this.gameObject.SetActive(true);
            this.transform.position = this.defaultPpsition;
            this.isActive = true;
        }
        else if (data.inScenePortals.TryGetValue(this.currentSceneName, out Vector3 portalPosition))
        {
            this.transform.position = portalPosition;
            this.isActive = true;
        }

        this.isReturningFromTown = data.isReturningFormTown;
        this.returnedSceneName = data.portalDestinationSceneName;
    }

    public void SaveData(ref GameData data)
    {
        // throw new System.NotImplementedException();
        data.isReturningFormTown = InTown();

        if (this.isActive && !InTown())
        {
            data.inScenePortals[this.currentSceneName] = this.transform.position;
            data.portalDestinationSceneName = currentSceneName;
        }
        else
        {
            data.inScenePortals.Remove(this.currentSceneName);
        }

    }

    public void Interact()
    {
        if (!this.canBeTrigger)
            return;

        UseTeleport();
    }
}
