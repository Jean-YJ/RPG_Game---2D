using UnityEngine;

public class Object_ItemPickup : MonoBehaviour
{
    [SerializeField] private Vector2 dropForce = new Vector2(1, 5);
    [SerializeField] private ItemData_SO itemData;
    [Space]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D colider;


    void OnValidate()
    {
        if (this.itemData == null) return;

        this.sr = GetComponent<SpriteRenderer>();

        SetUpVisual();
    }

    public void SetUpItem(ItemData_SO itemData)
    {
        this.itemData = itemData;
        SetUpVisual();

        // 让掉落物出现散落的轨迹
        float xDropForce = Random.Range(-this.dropForce.x, this.dropForce.x);
        this.rb.linearVelocity = new Vector2(xDropForce, this.dropForce.y);
        this.colider.isTrigger = false; //让物体可以碰撞地面，并在OnCollisionEnter2D进行后续的逻辑处理
    }

    // 设置物体的视觉图片
    private void SetUpVisual()
    {
        this.sr.sprite = itemData.itemIcon;
        this.gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("Player picked up " + itemData.itemName);
        // Debug.Log("Trigger");
        Inventory_Player playerInventory = collision.GetComponent<Inventory_Player>();
        if (playerInventory == null) return;

        Inventory_Item itemToAdd = new Inventory_Item(this.itemData);
        Inventory_Storage storage = playerInventory.storage;

        //Material类型的物品直接放到Storage
        if (itemToAdd.itemData.itemType == ItemType.Material)
        {
            storage.AddMaterialToStash(itemToAdd);
            Destroy(this.gameObject);
            return;
        }
        //其他类型的物品放到Inventory
        if (playerInventory.CanAddItem(itemToAdd))
        {
            playerInventory.AddItem(itemToAdd);
            Destroy(this.gameObject);
        }

    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("Collision");
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            this.colider.isTrigger = true; // 碰到地面变回trigger
            this.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

}
