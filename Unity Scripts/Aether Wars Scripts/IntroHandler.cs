using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroHandler : MonoBehaviour
{
    public static IntroHandler instance;

    [Header ("References")]
    public GameObject defaultShip;

    [Header ("Debug")]
    public bool spawnUnits;
    public bool spawnAsteroids;
    public bool devQuickstart;
    [Space (5)]
    public bool spawnStartItems;
    public GameObject[] startupItems;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
    }

    void Start () {
        StartCoroutine (Start_Sequence ());
    }

    IEnumerator Start_Sequence () {
        // ------------ Save Data Loading Stuff ['Essentials']
        PlayerProfile.instance.Load_Player_Data ();

        // Generation always-stuff
        if (GalaxyHandler.instance) {
            GalaxyHandler.instance.Generate_Galaxy ();
        }
        if (TrafficHandler.instance && spawnUnits == true) {
            TrafficHandler.instance.Spawn_Sector_Units ();
        }

        // ------------ Quickstart Stuff
        if (devQuickstart == false) {
            //Spawn_Player ();
            //Create_Start_Items ();
            if (HUDHandler.instance && HUDHandler.instance.blackscreen) {
                float alpha = 1f;
                HUDHandler.instance.blackscreen.color = new Color (HUDHandler.instance.blackscreen.color.r, HUDHandler.instance.blackscreen.color.g, HUDHandler.instance.blackscreen.color.b, alpha);
                
                while (alpha > 0f) {
                    alpha -= 1f * Time.deltaTime;
                    HUDHandler.instance.blackscreen.color = new Color (HUDHandler.instance.blackscreen.color.r, HUDHandler.instance.blackscreen.color.g, HUDHandler.instance.blackscreen.color.b, alpha);
                    yield return new WaitForEndOfFrame ();
                }
            }

            yield return new WaitForSeconds (1f);
        } else if (devQuickstart == true) {
            if (HUDHandler.instance && HUDHandler.instance.blackscreen) {
                HUDHandler.instance.blackscreen.color = new Color (HUDHandler.instance.blackscreen.color.r, HUDHandler.instance.blackscreen.color.g, HUDHandler.instance.blackscreen.color.b, 0f);
            }
            //Spawn_Player ();
            //Create_Start_Items ();
        }

        yield return null;
    }

    // Called by PlayerProfile when done loading save data
    public void Spawn_Player () {
        if (PlayerProfile.instance != null) {
            // Find saved shipchoice by Ship_Description-class' shipName - if noone in Resources/Ships matches, set shipPrefab to a default ship
            GameObject shipPrefab = defaultShip; // In case we don't find a saved ship
            //Debug.Log ("ShipPrefab PRE: " + shipPrefab);
            GameObject[] resShips = Resources.LoadAll <GameObject>("Ships");
            for (int s = 0; s < resShips.Length; s++) {
                Ship_Description shipdesc = resShips[s].GetComponent<Ship_Description>();
                //Debug.Log (resShips[s] + " || " + shipdesc + " || " + shipdesc.shipName + " compared_to " + PlayerProfile.instance.shipChoice);
                if (shipdesc != null && shipdesc.shipName == PlayerProfile.instance.shipChoice) {
                    shipPrefab = resShips[s];
                    break;
                }
            }
            //Debug.Log ("ShipPrefab POST: " + shipPrefab);

            if (shipPrefab != null) {
                GameObject newPlayer = Instantiate (shipPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                newPlayer.name = shipPrefab.name + "_PLAYER_";
                Set_As_Player (newPlayer);
                //Debug.LogError ("PLAYER SPAWNED");
            } else {
                Debug.LogError ("shipPrefab was null - so both no saved was found and shipDefault seems to not be set. Fix immediately, pls! :(");
            }
        } else {
            Debug.Log ("No PlayerProfile found!");
        }

        // Spawn playerprofile-saved items, if able (they have to match name-wise with our resources-loaded items)
        if (PlayerProfile.instance != null && PlayerProfile.instance.inventoryNames != null && PlayerProfile.instance.inventoryAmounts != null && PlayerProfile.instance.inventoryAmounts.Length == PlayerProfile.instance.inventoryNames.Length) {
            Inventory_Item[] resItems = Resources.LoadAll <Inventory_Item> ("Items");
            for (int i = 0; i < PlayerProfile.instance.inventoryNames.Length; i++) {
                for (int r = 0; r < resItems.Length; r++) {
                    if (resItems[r].itemName == PlayerProfile.instance.inventoryNames[i]) {
                        GameObject newItem = Instantiate (resItems[r].gameObject, Vector3.zero, Quaternion.identity) as GameObject;
                        newItem.name = resItems[r].gameObject.name;
                        Inventory_Item ii = newItem.GetComponent<Inventory_Item>();
                        if (ii != null) {
                            ii.Adjust_Item_Quantity (PlayerProfile.instance.inventoryAmounts[i]);
                        }
                        break;
                    }
                }
            }
        }
        // -------------------------------------------------------------------------------------------------------

        // Setup all modules, based on loaded player-data
        if (PlayerProfile.instance != null && PlayerProfile.instance.moduleNames != null && PlayerProfile.instance.moduleNames.Length > 0 && ReferenceHandler.instance != null && ReferenceHandler.instance.playerIFF != null) {
            Module[] resModules = Resources.LoadAll <Module> ("Modules");
            Hardpoint[] playerHardpoints = ReferenceHandler.instance.playerIFF.transform.root.GetComponentsInChildren<Hardpoint>();
            if (PlayerProfile.instance.moduleNames.Length <= playerHardpoints.Length) { // NOTE It's fine if playerprofile-modulenames are fewer ; then we just have empty slots on player for the rest, since we don't loop over those 'extra' spots
                for (int m = 0; m < PlayerProfile.instance.moduleNames.Length; m++) {
                    if (PlayerProfile.instance.moduleNames[m] != "None") {
                        for (int rm = 0; rm < resModules.Length; rm++) {
                            if (resModules[rm] != null && resModules[rm].moduleName == PlayerProfile.instance.moduleNames[m]) {
                                GameObject newMod = Instantiate (resModules[rm].gameObject, playerHardpoints[m].transform.position, playerHardpoints[m].transform.rotation) as GameObject;
                                newMod.name = resModules[rm].gameObject.name;
                                newMod.transform.SetParent (playerHardpoints[m].transform);

                                // If hardpoint has multi-muzzles, use these to override our new module's muzzle-setup
                                if (playerHardpoints[m].multimuzzled == true && playerHardpoints[m].muzzles.Length > 0) {
                                    Weapon_Generic wepgen = newMod.GetComponent<Weapon_Generic>();
                                    if (wepgen != null) {
                                        wepgen.muzzles.Clear();
                                        for (int muz = 0; muz < playerHardpoints[m].muzzles.Length; muz++) {
                                            wepgen.muzzles.Add (playerHardpoints[m].muzzles[muz]);
                                        }
                                        //wepgen.Visuals_Setup ();
                                    }
                                }

                            }
                        }
                    }
                }
            } else {
                Debug.LogError ("ERROR - playerHardpoints-length mismatch with PlayerProfile's moduleNames-length"); // They ought to be equally long, since the saved ship _oughta_ contain same hardpoint amount
            }

            HUDHandler.instance.Set_Radar_PlayerTransform();
            HangarHandler.instance.Reinitialize_Weapon_Systems ();
        }
        // ----------------------------------------------

        Create_Start_Items ();
    }

    void Create_Start_Items () {
        if (spawnStartItems == false) {
            return;
        }
        for (int s = 0; s < startupItems.Length; s++) {
            GameObject newItem = Instantiate (startupItems[s], Vector3.zero, Quaternion.identity) as GameObject;
            newItem.name = startupItems[s].name;
        }
    }

    // Note Also used by ShipPurchaseHandler when switching player-ship
    public void Set_As_Player (GameObject playerTopObject) {
        playerTopObject.tag = "Player";
        ReferenceHandler.instance.playerIFF = playerTopObject.GetComponent<IFF>();
        ReferenceHandler.instance.playerRB = playerTopObject.GetComponent<Rigidbody>();

        if (ConsoleHandler.instance && !devQuickstart) {
            ConsoleHandler.instance.Do_Console_Bootup_Sequence_For_ShipObject (playerTopObject);
        }
    }

}
