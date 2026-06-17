using UnityEngine;

public class Object_Chest : MonoBehaviour, IDamageable
{
    private Rigidbody2D rb => this.GetComponent<Rigidbody2D>();
    private Entity_VFX ev => this.GetComponent<Entity_VFX>();
    private Animator animator => this.GetComponentInChildren<Animator>();
    private Entity_DropManager dropManager => this.GetComponent<Entity_DropManager>();
    [Header("Open Details")]
    [SerializeField] private bool canDrop = true;
    [SerializeField] private Vector2 knockBack;
    public bool TakeDamage(float damage, float elementalDamage, E_ElementType elementType, Transform dealer)
    {
        // throw new System.NotImplementedException();
        if (!this.canDrop) return false;
        this.canDrop = false;
        this.dropManager?.DropItems();

        this.ev.PlayOnDamagedVFX();
        this.animator?.SetBool("chestOpen", true);
        this.rb.linearVelocity = this.knockBack;
        this.rb.angularVelocity = Random.Range(-200, 200);

        return true;
    }
}
