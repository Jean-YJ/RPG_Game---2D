using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_WayPoint : Object_Interact, IInteractable
{
    [SerializeField] private string transferToScene;
    [SerializeField] private RespawnType wayPointType;
    [SerializeField] private RespawnType connectedWayPointType;
    [SerializeField] private Transform respwanPos;
    [SerializeField] private bool canBeTriggered = true;

    public RespawnType GetRespawnType() => this.wayPointType;
    public Vector3 GetRespwanPosAndSetTriggerFalse()
    {
        this.canBeTriggered = false;
        return this.respwanPos == null ? this.transform.position : this.respwanPos.position;
    }

    // public void SetPointTriggered(bool value) => this.canBeTriggered = value;

    void OnValidate()
    {
        this.gameObject.name = "Object_WayPoint - " + wayPointType.ToString() + " - " + transferToScene;

        if (this.wayPointType == RespawnType.Enter)
            this.connectedWayPointType = RespawnType.Exit;

        if (this.wayPointType == RespawnType.Exit)
            this.connectedWayPointType = RespawnType.Enter;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        this.canBeTriggered = true;
    }

    public void Interact()
    {
        // throw new System.NotImplementedException();
        if (!this.canBeTriggered) //避免反复触发
            return;

        // SaveManager.Instance.SaveGame();

        //切换场景
        GameManager.Instance.ChangeScene(this.transferToScene, this.connectedWayPointType);
    }
}


public enum RespawnType
{
    Enter,
    Exit,
    Portal,
    NonSpecific
}
