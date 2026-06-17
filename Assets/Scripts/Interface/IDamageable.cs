using UnityEngine;

public interface IDamageable
{
    public bool TakeDamage(float damage, float elementalDamage, E_ElementType elementType, Transform dealer);
}
