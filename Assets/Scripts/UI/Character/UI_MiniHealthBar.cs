using Unity.VisualScripting;
using UnityEngine;

public class UI_MiniHealthBar : MonoBehaviour
{
    private Entity entity;
    void Awake()
    {
        this.entity = GetComponentInParent<Entity>();
    }
    void OnEnable()
    {
        this.entity.OnFlip += this.HandleFlip;
    }

    private void HandleFlip() => this.transform.rotation = Quaternion.identity;

    void OnDisable()
    {
        this.entity.OnFlip -= this.HandleFlip;
    }

}
