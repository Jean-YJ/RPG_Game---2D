using UnityEngine;

public class Entity_AnimationTrigger : MonoBehaviour
{
    private Entity e;
    private Entity_Combat ec;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        this.e = this.GetComponentInParent<Entity>();
        this.ec = this.GetComponentInParent<Entity_Combat>();
    }

    private void CurrentStateTrigger()
    {
        this.e.CurrentStateAnimationTrigger();
    }

    private void PerformAttack()
    {
        this.ec.PerformAttack();
    }
}
