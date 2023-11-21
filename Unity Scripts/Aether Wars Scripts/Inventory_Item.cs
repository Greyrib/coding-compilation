using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Item : MonoBehaviour
{
    [Header ("Details")]
    public string itemName;
    public InventoryHandler.ItemRarity rarity;
    public int itemQuantity = 1;
    public float weightPerItemUnit = 1f; // In Ts ; bullet probs 0.01f, whereas iron would be 1
    public int creditvalue;

    [Header ("Details - Buying")]
    public int levelRequiredToPurchase = 0; // If is -1, won't show up in hangar-buylist (might be quest items or something we only want as drops ; accomodates this, since all items are in Resources)

    [Header ("Dynamics")]
    public Cargo owner; // If this is set, we cannot get picked up via collision (to avoid multiple collisions when being picked up, essentially adding ourselves, increasing items too much from a single pickup)
    public Beacon tagBeacon; // Which beacon has tagged this item for pickup, preventing others from picking it up or tractoring it
    public TractorBeam tractor; // Who is currently tractoring this, to avoid multiple tractors

    Rigidbody rootRB;

    void Awake () {
        rootRB = GetComponent<Rigidbody>();
        if (transform.position.y != 0f) {
            transform.position = new Vector3 (transform.position.x, 0f, transform.position.z);
        }
    }

    public Rigidbody RB () {
        return rootRB;
    }

    // NOTE Cannot use OnDestroy(), since when do Destroy (this.gameObject), it will actually get called, so no use trying to separate them. Just remember to use Destroy_Item() always

    public void Adjust_Item_Quantity (int newAmount) {
        itemQuantity = newAmount;
        // Recalculate weights in Cargo-owner for new amount
        if (owner != null) {
            owner.Update_CargoSpace_Used ();
        }
        if (itemQuantity <= 0) {
            transform.SetParent (null);
            //InventoryHandler.instance.Rebuild_Inventory_HUD_List (); // No need to rebuild
            // Inventorylist gets updated OnDestroy() below
            // Cargobars get updated OnDestroy() below also
            Remove_From_Root_Cargo_If_Held (); // Since we adjusted to non-existant quantity
            Destroy (gameObject);
        }
        InventoryHandler.instance.Update_Item_Quantities ();
        InventoryHandler.instance.Update_Cargo_HUD_Bars ();
    }

    void Remove_From_Root_Cargo_If_Held () {
        if (owner != null && owner.cargoItems != null && owner.cargoItems.Contains (this)) {
            owner.cargoItems.Remove (this);
        }
    }

    void OnDestroy () {
        InventoryHandler.instance.Update_Item_Quantities (); // Update itemcard quantities; if they're 0 or below, will auto-remove & auto-resize inventorylist
        InventoryHandler.instance.Update_Cargo_HUD_Bars ();
    }

    public void OnCollisionEnter (Collision col) {
        if (owner == null && tagBeacon == null) {
            Cargo cargo = col.transform.root.GetComponent<Cargo>();
            //Debug.LogError ("ONCOLLISIONENTER FOR ITEM " + itemName);
            if (cargo != null && cargo.cargoLeft >= (itemQuantity * weightPerItemUnit)) {
                owner = cargo;
                
                //Debug.LogError ("Add_To_Cargo " + this + " [OnCollisionEnter] " + Time.time.ToString("F2"));

                cargo.Add_To_Cargo (this);
            }
        }
    }

    /// <summary>
    /// Destroy an item while ensuring it's removed from InventoryHandler's list as well, should it be present.
    /// </summary>
    public void Destroy_Item () {
        Clear_Inventory_HUD_ItemCard ();
        Destroy (this.gameObject);
    }

    public void Clear_Inventory_HUD_ItemCard () {
        if (InventoryHandler.instance != null && InventoryHandler.instance.listItems != null) {
            for (int l = 0; l < InventoryHandler.instance.listItems.Count; l++) {
                if (InventoryHandler.instance.listItems[l] != null && InventoryHandler.instance.listItems[l].linkedItem == this) {
                    GameObject cachedListCard = InventoryHandler.instance.listItems[l].gameObject;
                    InventoryHandler.instance.Resize_Inventory_List (Mathf.Clamp (InventoryHandler.instance.listItems.Count - 1, 0, 99999));
                    InventoryHandler.instance.listItems.Remove (InventoryHandler.instance.listItems[l]);
                    Destroy (cachedListCard);
                    break;
                }
            }
        }
    }

}
