using UnityEngine;

public class Object_NPC : MonoBehaviour
{
    protected Player player;
    protected UI_CanvasRoot canvasRoot;

    [SerializeField] private Transform npc;
    private bool isFacingRight = true;

    [Header("Float Interact Detail")]
    [SerializeField] private GameObject interactToolTip;
    [SerializeField] private float speed = 8.0f;
    [SerializeField] private float range = 0.15f;
    private Vector3 initialPosition;
    private float yOffset = 0;

    protected virtual void Awake()
    {
        this.interactToolTip.SetActive(false);
        this.canvasRoot = FindAnyObjectByType<UI_CanvasRoot>();
        this.initialPosition = this.interactToolTip.transform.position;
    }

    protected virtual void Update()
    {
        HandleNPCFilp();
        HandleToolTipFloat();
    }

    private void HandleNPCFilp()
    {
        if (this.player == null || this.npc == null)
            return;

        if (this.npc.position.x < this.player.transform.position.x && !this.isFacingRight)
        {
            this.npc.Rotate(0, 180, 0);
            this.isFacingRight = true;
        }
        if (this.npc.position.x > this.player.transform.position.x && this.isFacingRight)
        {
            this.npc.Rotate(0, 180, 0);
            this.isFacingRight = false;
        }
    }

    private void HandleToolTipFloat()
    {
        if (this.interactToolTip.activeSelf)
        {
            this.yOffset = Mathf.Sin(Time.time * this.speed) * range;
            this.interactToolTip.transform.position = this.initialPosition + new Vector3(0, yOffset);
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        this.player = collision.GetComponent<Player>();
        this.interactToolTip.SetActive(true);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        this.player = null;
        this.interactToolTip.SetActive(false);

    }

}
