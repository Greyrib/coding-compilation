using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Skillshot_DirectionNonhoming : Ability
{
    [Header ("References")]
    public GameObject projectilePrefab;
    public Transform muzzle;

    void Awake () {

    }

    void Start () {
        Presetup_Ability ();
    }

    void Update () {
        CooldownTimer_Reduce ();
        if (Input_Detected()) {
            Activate_Ability ();
        }
    }

    public override void Activate_Ability()
    {
        if (Requirements_Met () == false) {
            // Error SFX
            //Debug.Log ("Couldn't activate ability " + abilityName + " @ " + Time.time.ToString ("F2"));
            return;
        } else if (Requirements_Met() == true) {
            base.Activate_Ability(); // cooldownTime set to cooldown
            //Debug.Log (abilityName + " activated @ " + Time.time.ToString ("F2"));
            if (projectilePrefab != null && CrosshairControl.instance != null && muzzle != null) {
                GameObject newProj = Instantiate (projectilePrefab, muzzle.position, muzzle.rotation) as GameObject; // Might want to rotate to face crosshairpos_on_activation ?
                newProj.name = projectilePrefab.name + "_FromAbility_" + abilityName + "_@_" + Time.time.ToString ("F3");

                Projectile pro = newProj.GetComponent<Projectile>();
                if (pro != null) {
                    //pro.Activate_Projectile (muzzle.position, CrosshairControl.instance.transform.position, null);
                    pro.Activate_Projectile (muzzle.position, muzzle.position + (muzzle.forward * Clamped_Distance_To_CrosshairPos ()), null);
                }
            }
        }

        float Clamped_Distance_To_CrosshairPos () {
            float distance = abilityMaxDistance;
            if (CrosshairControl.instance != null) {
                float crossDist = Vector3.Distance (transform.root.position, CrosshairControl.instance.transform.position);
                crossDist = Mathf.Clamp (crossDist, 0f, abilityMaxDistance);
                distance = crossDist;
            }
            return distance;
        }
    }

    bool Requirements_Met () {
        if (cooldownTime <= 0f && ((energy != null && energy.EnergyCheck (energyCost, true)) || (energy == null))) {
            return true;
        }

        return false;
    }

}
