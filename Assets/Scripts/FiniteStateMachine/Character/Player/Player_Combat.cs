using System;
using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("Counter Attack Detail")]
    [SerializeField] private float counterRecoverDuration = 0.2f;

    public float GetCounterRecoverDuration() => this.counterRecoverDuration;

    public bool CounterAttackPerformed()
    {
        bool hasPerformedCounter = false;
        foreach (var item in this.GetDetectedColiders())
        {
            ICounterable ic = item.GetComponent<ICounterable>();
            if(ic == null)
                continue;

            if (ic.CanBeCountered)
            {
                hasPerformedCounter = true;
                ic.HandleCounter();
            }
        }
        return hasPerformedCounter;
    }
}
