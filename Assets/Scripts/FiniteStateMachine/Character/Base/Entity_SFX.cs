using UnityEngine;

public class Entity_SFX : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("SFX Names")]
    [SerializeField] private string attackHit;
    [SerializeField] private string attackMiss;

    [Space]
    [SerializeField] private float soundDistance = 15.0f;
    [SerializeField] private bool showGizmos;

    void Awake()
    {
        this.audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayAttackHit()
    {
        AudioManager.Instance.PlaySFX(this.attackHit, this.audioSource, this.soundDistance);
    }

    public void PlayAttackMiss()
    {
        AudioManager.Instance.PlaySFX(this.attackMiss, this.audioSource, this.soundDistance);
    }

    void OnDrawGizmos()
    {
        if (this.showGizmos)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(this.transform.position, this.soundDistance);
        }
    }
}
