using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    // Inheritable class for specific abilities, similar to in HOTS

    public enum AbilityButton {Q, E, R, C, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Alpha0};

    [Header ("Ability Parameters")]
    public string abilityName = "Generic Ability"; // Not sure if this is needed for anything, but whatevs
    public AbilityButton abilityButton; // Which slot on the UI & associated hotkey we use
    public float heatCost = 10f;
    public float cooldown = 5f;
    public float cooldownTimer;

    public IFF iff;
    public Health hp;
    public Heating heat;

    // TODO Event other things can subscribe to ? E.g. a Talent grants HoT on every Ability-activation

    void Start () {
        Ability_Setup (); // DOES NOT ACTUALLY RUN IN INHERITED CLASSES; MUST BE CALLED MANUALLY
    }

    public void Ability_Setup () {
        Debug.Log ("Running Ability_Setup() for " + abilityName + " @ " + Time.time.ToString ("F2"));
        iff = transform.root.GetComponent<IFF>();
        hp = transform.root.GetComponent<Health>();
        heat = transform.root.GetComponent<Heating>();
    }

    // DOES NOT ACTUALLY RUN IN INHERITED CLASSES; MUST BE CALLED MANUALLY
    /*
    void Update () {
        if (Ability_Input_Detection () && EnergyCheck ()) {
            Activate_Ability ();
        }

        Ability_Cooldown_And_HUD_Update ();
    }
    */

    public virtual void Activate_Ability () {
        cooldownTimer = cooldown;
    }

    public bool Ability_Input_Detection () {
        if (abilityButton == AbilityButton.Q && Input.GetKeyDown (KeyCode.Q)) {
            return true;
        } else if (abilityButton == AbilityButton.E && Input.GetKeyDown (KeyCode.E)) {
            return true;
        } else if (abilityButton == AbilityButton.C && Input.GetKeyDown (KeyCode.C)) {
            return true;
        } else if (abilityButton == AbilityButton.R && Input.GetKeyDown (KeyCode.R)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha1 && Input.GetKeyDown (KeyCode.Alpha1)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha2 && Input.GetKeyDown (KeyCode.Alpha2)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha3 && Input.GetKeyDown (KeyCode.Alpha3)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha4 && Input.GetKeyDown (KeyCode.Alpha4)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha5 && Input.GetKeyDown (KeyCode.Alpha5)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha6 && Input.GetKeyDown (KeyCode.Alpha6)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha7 && Input.GetKeyDown (KeyCode.Alpha7)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha8 && Input.GetKeyDown (KeyCode.Alpha8)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha9 && Input.GetKeyDown (KeyCode.Alpha9)) {
            return true;
        } else if (abilityButton == AbilityButton.Alpha0 && Input.GetKeyDown (KeyCode.Alpha0)) {
            return true;
        }

        return false;
    }

    public bool HeatCheck () {
        if (heat) {
            return heat.Heat_Check (heatCost);
        } else if (heat == null) {
            return true;
        }

        return false;
    }

    public bool Ability_Ready () {
        if (cooldownTimer <= 0f) {
            return true;
        }

        return false;
    }

    // Functionized, so inheritants can simply stuff it into Update()
    public void Ability_Cooldown_And_HUD_Update () {
        //Debug.Log ("Base-Ability Update Loop for " + abilityName + " @ " + Time.time.ToString ("F2"));
        if (cooldownTimer > 0f) {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) {
                cooldownTimer = 0f;
            }
        }
    }

    bool Sufficient_Heat () {
        if (heat != null && (heat.heatMax - heat.heat) < heatCost) {
            return false;
        }
        return true;
    }

}
