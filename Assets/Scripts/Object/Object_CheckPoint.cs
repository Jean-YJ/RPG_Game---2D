using UnityEngine;

public class Object_CheckPoint : Object_Interact, ISaveable, IInteractable
{
    [SerializeField] private string checkPointID;
    [SerializeField] private Transform respawnPoint;
    public bool isActive { get; private set; }
    private Animator animator;
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        this.animator = GetComponentInChildren<Animator>();
        this.audioSource = GetComponent<AudioSource>();
    }

    // 注意：在场景上放置新的本物体时，要移除并重新挂载本脚本。否则会导致this.checkPointID出现重复
#if UNITY_EDITOR
    void OnValidate()
    {
        if (string.IsNullOrEmpty(this.checkPointID))
        {
            this.checkPointID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
    public string GetCheckPointID() => this.checkPointID;

    public Vector3 GetRespawnPosition() => this.respawnPoint == null ? this.transform.position : this.respawnPoint.position;

    public void ActivateCheckPoint(bool activate)
    {
        this.isActive = activate;
        this.animator.SetBool("isActive", activate);

        if (this.isActive)
            this.audioSource.Play();
        else
            this.audioSource.Stop();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

    }

    public void LoadData(GameData data)
    {
        if (data.unlockedCheckPoints.TryGetValue(this.checkPointID, out bool status))
        {
            ActivateCheckPoint(status);
        }

    }

    public void SaveData(ref GameData data)
    {
        if (!this.isActive)
            return;

        if (!data.unlockedCheckPoints.ContainsKey(this.checkPointID))
            data.unlockedCheckPoints.Add(this.checkPointID, true);
    }

    public void Interact()
    {
        // throw new System.NotImplementedException();
        ActivateCheckPoint(true);
    }
}
