using UnityEngine;

public class AudioRangeController : MonoBehaviour
{
    private AudioSource audioSource;
    private Transform player;

    [SerializeField] private float minDistanceToHearSound = 15.0f;
    [SerializeField] private bool showGizmos;
    private float maxVolume;

    void Start()
    {
        this.player = Player.Instance.transform;
        this.audioSource = this.GetComponent<AudioSource>();

        this.maxVolume = this.audioSource.volume;
    }

    void Update()
    {
        if (this.player == null)
            return;

        float distance = Vector2.Distance(this.transform.position, this.player.position);
        float t = Mathf.Clamp01(1 - (distance / minDistanceToHearSound));

        this.audioSource.volume = Mathf.Lerp(0, maxVolume, t * t);
    }

    void OnDrawGizmos()
    {
        if (this.showGizmos)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(this.transform.position, this.minDistanceToHearSound);
        }
    }
}
