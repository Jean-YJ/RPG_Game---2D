using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Buff effect", fileName = "Item effect data - Buff")]
public class ItemBuffData_SO : ItemEffectData_SO
{
    [SerializeField] private BuffEffectData[] buffsToApply; // 该buff影响的属性类型，例如攻击力、移动速度等
    [SerializeField] private float duration;
    [SerializeField] private string buffSource = Guid.NewGuid().ToString(); // 用于标识增益效果的来源，确保同一来源的增益效果不会重复应用


    public override bool CanBeUsed(Player player)
    {
        if(player.playerStats.CanAddBuff(this.buffSource))
        {
            this.player = player;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void ExcuteEffect()
    {
        // base.ExcuteEffect();
        this.player.playerStats.ApplyBuffs(this.buffsToApply, this.duration, this.buffSource);
        this.player = null; // 立即将player引用置空，防止增益效果持续期间对player的任何引用
    }
    
}
