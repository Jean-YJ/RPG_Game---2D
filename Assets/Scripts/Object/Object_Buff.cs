using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Object_Buff : MonoBehaviour
{
    private Player_Stats takerStats;
    [Header("Float Detail")]
    [SerializeField] private float speed = 2.0f;
    [SerializeField] private float range = 0.15f;
    private Vector3 initialPosition;
    private float xOffset = 0;
    private float yOffset = 0;

    [Header("Buff Detail")]
    [SerializeField] private float duration = 5.0f;
    [SerializeField] private string buffName = "Buff Name";
    // [SerializeField] private float buffValue = 10.0f;
    [SerializeField] private BuffEffectData[] buffs; // 该buff影响的属性类型，例如攻击力、移动速度等

    void Awake()
    {
        this.initialPosition = this.transform.position;
    }

    void Update()
    {
        this.yOffset = Mathf.Sin(Time.time * this.speed) * this.range;
        this.transform.position = this.initialPosition + new Vector3(this.xOffset, this.yOffset, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (!this.canBeUsed)
        //     return;

        this.takerStats = collision.GetComponent<Player_Stats>();
        if (this.takerStats.CanAddBuff(this.buffName))
        {
            this.takerStats.ApplyBuffs(this.buffs, this.duration, this.buffName);

            Destroy(this.gameObject);
        }


        // StartCoroutine(BuffCor(this.duration));
    }

    // 在Player_Stats中实现增益效果的应用和移除逻辑，这样可以更好地管理玩家的状态和增益效果
    // 该方法已不需要了，因为增益效果的应用和移除逻辑已经转移到Player_Stats中
    // private IEnumerator BuffCor(float duration)
    // {
    //     this.canBeUsed = false;
    //     // 拾取Buff后，由于直接销毁物体会中断协程，使后续逻辑无法触发
    //     // 所以使其不可见
    //     this.spriteRenderer.color = Color.clear;

    //     // 在这里应用增益效果，例如增加玩家的攻击力、移动速度等
    //     // 这可以通过访问玩家的属性或调用玩家的方法来实现
    //     SetBuff(true);

    //     // 等待持续时间结束
    //     yield return new WaitForSeconds(duration);

    //     // 在这里移除增益效果，恢复玩家的属性到原始状态
    //     SetBuff(false);
    //     Destroy(this.gameObject);
    // }

    // 该方法已不需要了
    // private void SetBuff(bool apply)
    // {
    //     foreach (var buff in this.buffs)
    //     {
    //         if (apply)
    //             this.takerStats.GetStatByType(buff.type).AddModifier(buff.value, this.buffName);
    //         else
    //             this.takerStats.GetStatByType(buff.type).RemoveModifier(this.buffName);
    //     }
    // }

}


