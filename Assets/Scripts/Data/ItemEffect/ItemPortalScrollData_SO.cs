using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "RPG SetUp/Item Data/Item effect/Open Portal", fileName = "Item effect data - Portal Scroll")]
public class ItemPortalScrollData_SO : ItemEffectData_SO
{
    public override void ExcuteEffect()
    {
        base.ExcuteEffect();

        if (SceneManager.GetActiveScene().name == "Level_0")
        {
            Debug.Log("Cannot Open Portal In Town");
            return;
        }

        Vector3 portalPosition = Player.Instance.transform.position + new Vector3(Player.Instance.faceDir * 1.5f, 0);
        
        Object_Portal.Instance.ActivatePortal(portalPosition, Player.Instance.faceDir);
    }
}
