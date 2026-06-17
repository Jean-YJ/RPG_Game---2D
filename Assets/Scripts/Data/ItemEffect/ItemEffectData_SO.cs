using UnityEngine;

public class ItemEffectData_SO : ScriptableObject
{
   [TextArea]
   public string effectDescription;
   protected Player player;

   public virtual bool CanBeUsed(Player player)
   {
       return true;
   }

   public virtual void ExcuteEffect()
   {
    //   Debug.Log("Executing item effect: " + effectDescription);
   }

   public virtual void Subscribe(Player player)
   {
       this.player = player;
   }
   
   public virtual void Unsubscribe()
   {
      //  this.player = null;
   }
}
