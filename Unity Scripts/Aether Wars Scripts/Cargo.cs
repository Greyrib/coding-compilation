using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cargo : MonoBehaviour
{
    // Part of the Inventory set of classes ; This is for Ships (so enemies can pick up cargo & drop it when they die + player can pick up stuff)
    // InventoryHandler handles the HUD element of listing Inventory/Cargo

    [Header ("Specifications")]
    public float cargoMax = 10f; // Ts of max cargo able to carry

    [Header ("Dynamics")]
    public float cargoLeft;
    public List<Inventory_Item> cargoItems;

    Transform itemsParent;

    void Awake () {
        GameObject cargoParent = new GameObject ("_Cargo_");
        cargoParent.transform.SetParent (transform);
        itemsParent = cargoParent.transform;

        cargoItems = new List<Inventory_Item>();
        Update_CargoSpace_Used ();
    }

    public void Add_To_Cargo (Inventory_Item item) {
        // If item already is in cargo, add to quantity
        bool alreadyAdded = false;
        for (int i = 0; i < cargoItems.Count; i++) {
            if (cargoItems[i].itemName == item.itemName) {
                cargoItems[i].Adjust_Item_Quantity (cargoItems[i].itemQuantity + item.itemQuantity);
                item.Destroy_Item (); // Destroy (item.gameObject);
                alreadyAdded = true;
                break;
            }
        }

        // If not alreadyAdded, add as new item & update the HUD inventory list
        if (alreadyAdded == false) {
            cargoItems.Add (item);
            item.transform.SetParent (itemsParent);
            item.transform.position = transform.root.position;
            item.gameObject.SetActive (false);

            InventoryHandler.instance.Add_Item_To_InvList (item);
        }

        
        // Play a sound for pickup either way
        if (transform.root.tag == "Player" && SFX.instance && SFX.instance.itemPickupPlayerSound != null) {
            SFX.instance.Play_SFX (SFX.instance.itemPickupPlayerSound, item.transform.position);
        }

        // Spawn a vfx for pickup either way
        if (Objectpooler.instance != null && Objectpooler.instance.pickupEffect != null) {
            Instantiate (Objectpooler.instance.pickupEffect, item.transform.position, Quaternion.identity);
        }

        // Update relevants
        Update_CargoSpace_Used ();
        InventoryHandler.instance.Update_Cargo_HUD_Bars ();
        InventoryHandler.instance.Resize_Inventory_List (InventoryHandler.instance.itemsParent.childCount);
        EventSystem_Custom.instance.Event_ReUpdateSellList (); // Becows if we buy in hangarhud, new items may be added, so we need to reupdate from here
        EventSystem_Custom.instance.Event_UpdateSellButtonQuantities (); // Becows if additional items are purchased in hangarhud, we need to update itemquantities (here, it's for if we add a new sell-button as above)
    }

    // Public so items can call this, e.g. when player sells/buys items in hangarhud
    public void Update_CargoSpace_Used () {
        cargoLeft = cargoMax;
        for (int i = 0; i < cargoItems.Count; i++) {
            cargoLeft -= (cargoItems[i].itemQuantity * cargoItems[i].weightPerItemUnit);
        }
    }

    #region jettisoning

    float jettisonForce = 10f;

    public void Jettison_Items () {
        // TODO Standard jettison
    }

    public void Jettison_Items_For_Beacon (Beacon requestingBeacon) {
        StartCoroutine (JettisonSequence_Beacon (requestingBeacon));
    }

    IEnumerator JettisonSequence_Beacon (Beacon requestingBeacon) {
        //Debug.LogError ("JettisonSequence_Beacon ...");
        for (int i = 0; i < cargoItems.Count + 1; i++) {
            //Debug.Log ("JS_B i = " + i);
            if (cargoItems.Count > 0 && cargoItems[0] != null) {
                //Debug.Log ("JS_B i : " + i + " || " + cargoItems[0]);
                cargoItems[0].tagBeacon = requestingBeacon;
                cargoItems[0].owner = null;
                cargoItems[0].transform.SetParent (null);
                cargoItems[0].transform.position = new Vector3 (cargoItems[0].transform.position.x, -30f, cargoItems[0].transform.position.z); // Reposition item to be below pickup issues (a bit below zeroplane)
                cargoItems[0].Clear_Inventory_HUD_ItemCard ();
                float force = 3f;
                cargoItems[0].RB ().velocity = new Vector3 (Random.Range (-1f, 1f), 0f, Random.Range (-1f, 1f)) * force;
                requestingBeacon.beacontaggedItems.Add (cargoItems[0]);
                this.cargoLeft += cargoItems[0].weightPerItemUnit * cargoItems[0].itemQuantity;
                if (this.cargoLeft > cargoMax) {
                    this.cargoLeft = cargoMax;
                }
                cargoItems[0].gameObject.SetActive (true);
                cargoItems.Remove (cargoItems[0]);
            }
            //yield return new WaitForSeconds (0.5f);
        }

        //cargoItems = new List<Inventory_Item>();
        InventoryHandler.instance.Rebuild_Inventory_HUD_List ();

        yield return null;
    }
    #endregion jettisoning

}
