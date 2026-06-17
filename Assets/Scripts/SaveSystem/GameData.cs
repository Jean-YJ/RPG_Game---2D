using System;
using UnityEngine;

[Serializable]
public class GameData
{
   public int gold = 5000;

   public SerializableDictionary<string, int> playerInventory; //saveID - stackSize
   public SerializableDictionary<string, int> storageItems;
   public SerializableDictionary<string, int> storageMaterials;

   public SerializableDictionary<string, ItemType> equipedItems; //saveID - slotType

   public int skillPoint = 10;
   public SerializableDictionary<string, bool> skillTreeUI; //skillName - unlock status
   public SerializableDictionary<SkillType, SkillUpgradeType> skillUpgrades; //skill type - upgrade type

   // public Vector3 savedCheckPoint;
   public SerializableDictionary<string, bool> unlockedCheckPoints; // checkpoint ID - unlocked status

   public SerializableDictionary<string, Vector3> inScenePortals; // sceneName - portalPosition
   public string portalDestinationSceneName;
   public bool isReturningFormTown;

   public string lastScenePlayed;
   public Vector3 lastPlayerPosition;

   public GameData()
   {
      this.playerInventory = new SerializableDictionary<string, int>();
      this.storageItems = new SerializableDictionary<string, int>();
      this.storageMaterials = new SerializableDictionary<string, int>();

      this.equipedItems = new SerializableDictionary<string, ItemType>();

      this.skillTreeUI = new SerializableDictionary<string, bool>();
      this.skillUpgrades = new SerializableDictionary<SkillType, SkillUpgradeType>();

      this.unlockedCheckPoints = new SerializableDictionary<string, bool>();

      this.inScenePortals = new SerializableDictionary<string, Vector3>();
   }
}
