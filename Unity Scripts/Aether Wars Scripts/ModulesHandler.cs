using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ModulesHandler : MonoBehaviour
{
    public static ModulesHandler instance;

    [Header ("References")]
    public GameObject hardpointButtonPrefab;
    public Transform hardpointsParent;
    public RectTransform hardpointsResizeable;
    [Space (5)]
    public GameObject modulesListObject;
    public GameObject moduleButtonPrefab;
    public Transform modulesParent;
    public RectTransform modulesResizeable;

    // NOTE This also handles the hardpoint-selection menu in the 'Hangar' interface

    [Header ("Dynamics")]
    public Hardpoint hardpointCurrentlyEditing;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
    }

    void Start () {
        Set_Initial_States ();
    }

    void Set_Initial_States () {
        if (modulesListObject != null && modulesListObject.activeInHierarchy) {
            modulesListObject.SetActive (false);
        }
    }

    // This is the first thing player clicks - a list of all hardpoints ; clicking one, will show available installable modules for that hardpoint (e.g. primaries if primary-hardpoint, etc.)
    public void Build_Hardpoints_List () {
        // Remove old list childrens
        if (hardpointsParent != null && ReferenceHandler.instance) {
            ReferenceHandler.instance.Destroy_All_Children (hardpointsParent);
        }

        // Build new hardpoint select buttons list
        if (ReferenceHandler.instance && ReferenceHandler.instance.playerIFF != null) {
            Hardpoint[] hardpoints = ReferenceHandler.instance.playerIFF.transform.root.GetComponentsInChildren<Hardpoint>();
            for (int h = 0; h < hardpoints.Length; h++) {
                GameObject newHardpointButton = Instantiate (hardpointButtonPrefab) as GameObject;
                newHardpointButton.transform.SetParent (hardpointsParent);
                newHardpointButton.GetComponent<Hardpoint_SelectButton>().Associate_Hardpoint (hardpoints[h]);
            }

            // Resize hardpoints content/resizeable list to buttons count
            float heightPerButton = 30f;
            float spacingPerButton = 0f;
            float newHeight = (heightPerButton * hardpoints.Length) + (Mathf.Clamp (hardpoints.Length - 1, 0, 99999) * spacingPerButton);
            hardpointsResizeable.sizeDelta = new Vector2 (hardpointsResizeable.sizeDelta.x, newHeight);
        }
    }

    // This is the list that is shown, when a hardpoint-button is clicked ; shows installable modules for the chosen hardpoint
    public void Build_Modules_List (Module.ModuleType typesToList, Module.ModuleSize maxSize) {
        // Clear out previous modules list
        ReferenceHandler.instance.Destroy_All_Children (modulesParent);

        int modulesButtonized = 0; // Moved up here, becows both titles are also to be counted

        // 'Sell' title
        if (BuyHandler.instance && BuyHandler.instance.buyListTitleSeparatorPrefab != null) {
            GameObject newTitle = Instantiate (BuyHandler.instance.buyListTitleSeparatorPrefab) as GameObject;
            newTitle.transform.SetParent (modulesParent);
            Buy_TitleSeparator bts = newTitle.GetComponent<Buy_TitleSeparator>();
            if (bts != null && bts.titleTextfield != null) {
                bts.titleTextfield.text = "Sell";
            }
            modulesButtonized += 1;
        }

        // "Sell" modulebutton ; detect if hardpointCurrentlyEditing has a module, and if so, create 'sell-button'
        Module curmod = hardpointCurrentlyEditing.GetComponentInChildren<Module>();
        if (curmod != null) {
            GameObject sellBtn = Instantiate (moduleButtonPrefab) as GameObject;
            sellBtn.transform.SetParent (modulesParent);
            sellBtn.GetComponent<Module_ChooseButton>().Set_Associated_Module (null);
        }
        // -------------------------------------------------------------------------------------------------------

        // <typesToList> title
        if (BuyHandler.instance && BuyHandler.instance.buyListTitleSeparatorPrefab != null) {
            GameObject newTitle = Instantiate (BuyHandler.instance.buyListTitleSeparatorPrefab) as GameObject;
            newTitle.transform.SetParent (modulesParent);
            Buy_TitleSeparator bts = newTitle.GetComponent<Buy_TitleSeparator>();
            if (bts != null && bts.titleTextfield != null) {
                bts.titleTextfield.text = typesToList.ToString();
            }
            modulesButtonized += 1;
        }

        
        Module[] resModules = Resources.LoadAll <Module>("Modules");
        //Debug.Log ("Found modules in Resources count: " + resModules.Length);
        // Iterate through and create buttons if type fits
        for (int m = 0; m < resModules.Length; m++) {
            //Debug.Log ("Iteration " + m  + " // Module: " + resModules[m]);
            if (resModules[m].moduleType == typesToList && moduleButtonPrefab != null && modulesParent != null && SameOrBelow_ModuleSize (resModules[m].moduleSize, maxSize)) {
                GameObject newModBtn = Instantiate (moduleButtonPrefab) as GameObject;
                newModBtn.transform.SetParent (modulesParent);
                newModBtn.GetComponent<Module_ChooseButton>().Set_Associated_Module (resModules[m]);
                modulesButtonized += 1;
            }
        }

        // Resize according to module button count
        if (modulesResizeable != null) {
            float heightPerButton = 30f;
            float spacingPerButton = 0f;
            float finalHeight = (heightPerButton * modulesButtonized) + (Mathf.Clamp (modulesButtonized - 1, 0, 99999) * spacingPerButton);
            modulesResizeable.sizeDelta = new Vector2 (modulesResizeable.sizeDelta.x, finalHeight);
        }

        // Show modules list, finalized
        if (modulesListObject != null) {
            modulesListObject.SetActive (true);
        }
    }

    bool SameOrBelow_ModuleSize (Module.ModuleSize comparingSize, Module.ModuleSize compareMaximum) {
        if ((int)comparingSize <= (int)compareMaximum) {
            //Debug.Log (comparingSize + " is same or below as " + compareMaximum);
            return true;
        }
        //Debug.Log (comparingSize + " is NOT same or below as " + compareMaximum);
        return false;
    }

    public void Merge_Similar_And_Mergeable_Weapons () {
        if (ReferenceHandler.instance && ReferenceHandler.instance.playerIFF != null) {
            Dictionary<string, int> wepCount = new Dictionary<string, int>();
            Weapon_Generic[] wepgens = ReferenceHandler.instance.playerIFF.transform.root.GetComponentsInChildren<Weapon_Generic>();
            // Add all mergeables to wepcount
            for (int wgs = 0; wgs < wepgens.Length; wgs++) {
                if (wepgens[wgs].mergeable == true && wepCount.ContainsKey (wepgens[wgs].weaponName) == false) {
                    wepCount.Add (wepgens[wgs].weaponName, 1);
                } else if (wepgens[wgs].mergeable == true && wepCount.ContainsKey (wepgens[wgs].weaponName) == true) {
                    wepCount [wepgens[wgs].weaponName] += 1;
                }
            }
            // Iterate through and merge mergeables
            for (int wgs = 0; wgs < wepCount.Count; wgs++) {
                Weapon_Generic mergeFirst = null;
                for (int wg = 0; wg < wepgens.Length; wg++) {
                    if (wepCount.ContainsKey (wepgens[wg].weaponName) && mergeFirst == null) {
                        mergeFirst = wepgens[wg];
                    } else if (wepCount.ContainsKey (wepgens[wg].weaponName) && mergeFirst != null && wepgens[wg].weaponName == mergeFirst.weaponName) {
                        // Merge muzzles from this one into mergeFirst-muzzles & disable this one afterwards
                        for (int m = 0; m < wepgens[wg].muzzles.Count; m++) {
                            mergeFirst.muzzles.Add (wepgens[wg].muzzles[m]);
                        }
                        wepgens[wg].enabled = false;
                    }
                }

                // Need to re-update Visuals, for merged weapons, since their muzzles will have changed
                if (mergeFirst != null) {
                    mergeFirst.Acquire_References ();
                }
            }
        }
    }

}
