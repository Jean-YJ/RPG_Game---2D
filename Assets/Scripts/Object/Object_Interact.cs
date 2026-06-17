using UnityEngine;

public class Object_Interact : MonoBehaviour
{
    [Header("Float Interact Detail")]
    [SerializeField] private GameObject interactToolTip;
    [SerializeField] private float speed = 8.0f;
    [SerializeField] private float range = 0.15f;
    private Vector3 initialPosition;
    private float yOffset = 0;

    protected virtual void Awake()
    {
        this.interactToolTip.SetActive(false);
        // this.initialPosition = this.interactToolTip.transform.localPosition;
    }

    protected virtual void Update()
    {

        HandleToolTipFloat();
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
        this.interactToolTip.SetActive(true);
        this.initialPosition = this.interactToolTip.transform.position;
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        this.interactToolTip.SetActive(false);

    }
}
