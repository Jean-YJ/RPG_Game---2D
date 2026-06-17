using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Stats : Entity_Stats
{
    private List<string> activeBuffs = new List<string>();

    public bool CanAddBuff(string buffName)
    {
        return !activeBuffs.Contains(buffName);
    }

    public void ApplyBuffs(BuffEffectData[] buffsToApply,float duration,string source)
    {
        StartCoroutine(BuffCor(buffsToApply,duration,source));
    }

    private IEnumerator BuffCor(BuffEffectData[] buffsToApply,float duration,string source)
    {
        this.activeBuffs.Add(source);
        // 在这里应用增益效果，例如增加玩家的攻击力、移动速度等
        foreach (BuffEffectData buff in buffsToApply)
        {
            GetStatByType(buff.type).AddModifier(buff.value,source);
        }

        yield return new WaitForSeconds(duration);

        this.activeBuffs.Remove(source);
        // 在这里移除增益效果
        foreach (BuffEffectData buff in buffsToApply)
        {
            GetStatByType(buff.type).RemoveModifier(source);
        }
    }
}
