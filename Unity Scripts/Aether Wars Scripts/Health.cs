using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header ("References")]
    public Health_Shield shield;

    [Header ("Parameters")]
    public float health = 100f;
    public float healthMax = 100f;
    public float regenRate;
    [Space (3)]
    public int xpValue = 1;

    [Header ("DMG FX")]
    public ParticleSystem[] slightParticles;
    public ParticleSystem[] mediumParticles;
    public ParticleSystem[] criticalParticles;
    float[] mediumRates;
    float[] mediumRatesOverDistance;
    float[] criticalRates;
    
    [Header ("Death FX")]
    public int ragdollsMin;
    public int ragdollsMax;
    public GameObject[] deathEffects;
    public AudioClip[] deathSounds;
    public Transform[] explodePoints;
    public float intermediateWait; // Custom-set wait time inbetween explodePoint-explosions & progressing death_termination_sequence
    public GameObject[] debris;
    public Asteroid asteroid;
    public LootTable lootTable;

    float minimumWait; // minimum wait time when dying to permit all particlesystems to finish their particle-lifetimes, after being set to 0 emissions

    bool hasDied;
    IFF iff;
    Rigidbody rootrb;
    List<Transform> damagers;
    Dictionary<string, float> damageAmounts;
    Debris_Sub[] subdebris;

    void Awake () {
        hasDied = false;
        damagers = new List<Transform>();
        damageAmounts = new Dictionary<string, float>();
        if (debris != null && debris.Length > 0) {
            for (int d = 0; d < debris.Length; d++) {
                debris[d].SetActive (false);
            }
        }
        iff = transform.GetComponent<IFF>();
        rootrb = GetComponent<Rigidbody>();
        Acquire_SubDebris ();
        Setup_Damage_Particles ();
        Calculate_MinimumWait ();
    }

    // Public becows it is also called from EquipmentHandler when NPC-Iglapods are created & given an aeroframe (and/or other parts which may have subdebris; hence, need to update subdebris here)
    public void Acquire_SubDebris () {
        subdebris = GetComponentsInChildren<Debris_Sub>();
        //Debug.Log ("Found subdebris amount: " + subdebris.Length);
    }

    #region damage_particles
    void Setup_Damage_Particles () {
        if (mediumParticles != null && mediumParticles.Length > 0) {
            mediumRates = new float[mediumParticles.Length];
            mediumRatesOverDistance = new float[mediumParticles.Length];
            for (int m = 0; m < mediumParticles.Length; m++) {
                mediumRates[m] = mediumParticles[m].emission.rateOverTime.constantMax;
                mediumRatesOverDistance[m] = mediumParticles[m].emission.rateOverDistance.constantMax;
            }
        }
        
        if (criticalParticles != null && criticalParticles.Length > 0) {
            criticalRates = new float[criticalParticles.Length];

            for (int c = 0; c < criticalParticles.Length; c++) {
                criticalRates[c] = criticalParticles[c].emission.rateOverTime.constantMax;
            }
        }

        Update_Damage_Particles ();
    }

    void Update_Damage_Particles () {
        if (!hasDied && health < healthMax * 0.75f && slightParticles != null) {
            for (int s = 0; s < slightParticles.Length; s++) {
                if (!slightParticles[s].isPlaying) {
                    slightParticles[s].Play ();
                }
            }
        } else if (slightParticles != null) {
            for (int s = 0; s < slightParticles.Length; s++) {
                if (slightParticles[s].isPlaying) {
                    slightParticles[s].Stop ();
                }
            }
        }

        if (!hasDied && health < healthMax * 0.50f && mediumParticles != null) {
            for (int s = 0; s < mediumParticles.Length; s++) {
                ReferenceHandler.instance.SetEmissionRate (mediumParticles[s], mediumRates[s]);
                ReferenceHandler.instance.SetEmissionRateOverDistance (mediumParticles[s], mediumRatesOverDistance[s]);
            }
        } else if (mediumParticles != null) {
            for (int s = 0; s < mediumParticles.Length; s++) {
                ReferenceHandler.instance.SetEmissionRate (mediumParticles[s], 0f);
                    ReferenceHandler.instance.SetEmissionRateOverDistance (mediumParticles[s], 0f);
            }
        }

        if (!hasDied && health < healthMax * 0.25f && criticalParticles != null) {
            for (int s = 0; s < criticalParticles.Length; s++) {
                ReferenceHandler.instance.SetEmissionRate (criticalParticles[s], criticalRates[s]);
            }
        } else if (criticalParticles != null) {
            for (int s = 0; s < criticalParticles.Length; s++) {
                ReferenceHandler.instance.SetEmissionRate (criticalParticles[s], 0f);
            }
        }
    }
    #endregion damage_particles

    void Calculate_MinimumWait () {
        minimumWait = 0f;
        for (int s = 0; s < slightParticles.Length; s++) {
            if (slightParticles[s].main.startLifetime.constantMax > minimumWait) {
                minimumWait = slightParticles[s].main.startLifetime.constantMax;
            }
        }

        for (int m = 0; m < mediumParticles.Length; m++) {
            if (mediumParticles[m].main.startLifetime.constantMax > minimumWait) {
                minimumWait = mediumParticles[m].main.startLifetime.constantMax;
            }
        }

        for (int c = 0; c < criticalParticles.Length; c++) {
            if (criticalParticles[c].main.startLifetime.constantMax > minimumWait) {
                minimumWait = criticalParticles[c].main.startLifetime.constantMax;
            }
        }
    }

    void Start () {
        Update_HUD_If_Applicable ();
        StartCoroutine (HoTs_Tick ());
    }

    IEnumerator HoTs_Tick () {
        Buff_HoT[] hots = transform.root.GetComponentsInChildren<Buff_HoT>();
        float totalHealing = 0f;
        for (int h = 0; h < hots.Length; h++) {
            totalHealing += hots[h].tickHealing;
            hots[h].Reduce_Tick ();
        }
        health += totalHealing;
        if (health > healthMax) {
            health = healthMax;
        }

        Update_HUD_If_Applicable ();
        Update_HUD_RegenHealingBar ();

        yield return new WaitForSeconds (1f);
        StartCoroutine (HoTs_Tick ());
        yield return null;
    }

    public void Update_HUD_RegenHealingBar () {
        if (transform.root.tag == "Player" && HUDHandler.instance && HUDHandler.instance.healthRegenbar != null && healthMax > 0f) {
            Buff_HoT[] hots = transform.root.GetComponentsInChildren<Buff_HoT>(); // TODO Optimize this to be calculated in HoTs_Tick, instead of GetComponents twice, but whatevs
            float remainingRegen = 0f;
            for (int h = 0; h < hots.Length; h++) {
                remainingRegen += hots[h].tickHealing * hots[h].ticksRemaining;
            }
            
            if (remainingRegen > 0f) {
                HUDHandler.instance.healthRegenbar.fillAmount = Mathf.Clamp ((health + remainingRegen) / healthMax, 0f, 1f);
            } else {
                HUDHandler.instance.healthRegenbar.fillAmount = 0f;
            }
        }
    }

    void Update () {
        if (!hasDied && regenRate > 0f && health < healthMax) {
            health += regenRate * Time.deltaTime;
            if (health > healthMax) {
                health = healthMax;
            }
            Update_HUD_If_Applicable ();
        }

        Player_NanokitShieldBat_Input ();
    }

    void Player_NanokitShieldBat_Input () {
        bool playerInputDetected = false;

        if (transform.root.tag == "Player" && Input.GetKeyDown (KeyCode.F)) {
            // Use a shield battery
            if (InventoryHandler.instance) {
                for (int i = 0; i < InventoryHandler.instance.listItems.Count; i++) {
                    if (InventoryHandler.instance.listItems != null && InventoryHandler.instance.listItems[i] != null && InventoryHandler.instance.listItems[i].linkedItem.itemName == "Shield Battery") {
                        InventoryHandler.instance.listItems[i].linkedItem.Adjust_Item_Quantity (-1);
                        Update_HUD_If_Applicable ();
                        if (SFX.instance && SFX.instance.shieldbatUsed) {
                            SFX.instance.Play_SFX (SFX.instance.shieldbatUsed, Camera.main.transform.position);
                        }
                        break; // Successfully deployed a shieldbat! :D
                    }
                }
            }
            playerInputDetected = true;
        }

        if (transform.root.tag == "Player" && Input.GetKeyDown (KeyCode.G)) {
            // Use a nanokit
            if (InventoryHandler.instance) {
                for (int i = 0; i < InventoryHandler.instance.listItems.Count; i++) {
                    if (InventoryHandler.instance.listItems != null && InventoryHandler.instance.listItems[i] != null && InventoryHandler.instance.listItems[i].linkedItem.itemName == "Nanokit") {
                        health += 100;
                        if (health > healthMax) {
                            health = healthMax;
                        }
                        InventoryHandler.instance.listItems[i].linkedItem.Adjust_Item_Quantity (-1);
                        Update_HUD_If_Applicable ();
                        if (SFX.instance && SFX.instance.nanokitUsed) {
                            SFX.instance.Play_SFX (SFX.instance.nanokitUsed, Camera.main.transform.position);
                        }
                        break; // Successfully deployed a nanokit! ^_^
                    }
                }
            }
            playerInputDetected = true;
        }

        if (HUDHandler.instance && playerInputDetected) {
            HUDHandler.instance.Update_NanokitShieldbat_Counters ();
        }
    }

    public void Damage (float amount, Vector3 impactPoint, Transform damager) {
        if (hasDied) {
            return;
        }

        if (!damagers.Contains (damager)) {
            //Debug.Log (transform.name + " doesn't contain " + damager.ToString () + " - adding to damagers");
            damagers.Add (damager);
        }
        if (damageAmounts.ContainsKey (damager.ToString())) {
            //Debug.Log (transform.name + " editing " + damageAmounts[damager.ToString()] + " to new value...");
            damageAmounts[damager.ToString()] = damageAmounts[damager.ToString()] + amount;
            //Debug.Log (transform.name + " - New damagers-Value for " + damager.ToString () + " = " + damageAmounts[damager.ToString()]);
        } else {
            //Debug.Log (transform.name + " - Adding " + damager.ToString() + " to damageAmounts, new entry.");
            damageAmounts.Add (damager.ToString(), amount);
        }

        float remainingAmount = amount;

        // Difficulty-based damage-scaling, if applicable
        if (Settings.instance != null && transform.root.tag == "Player") {
            remainingAmount = remainingAmount * Settings.instance.DifficultyBased_DamageFactor ();
        }
        
        if (shield != null) {
            remainingAmount = shield.Shield_Damage (remainingAmount, impactPoint);
        }

        // TODO Check for shields on this unit here & adjust damage accordingly

        // TODO Armor damage-reduction here (e.g. like HOTS, 1% reduction per armorpoint)

        if (remainingAmount != 0f) {
            health -= remainingAmount;
        }

        if (health > healthMax) {
            health = healthMax; // Health_Regen dues inverted-Damage, results in overhealing - fixed here
        }

        Update_HUD_If_Applicable ();
        Update_Damage_Particles ();

        if (health <= 0f && Unit_Can_Die ()) {
            Die ();
        } else if (iff != null && iff.SPEECH() != null) {
            iff.SPEECH ().Im_Damaged ();
        }
    }

    bool Unit_Can_Die () {
        if (hasDied) {
            return false;
        } else if (transform.root.tag == "Player" && DebugKeys.instance && DebugKeys.instance.playerImmortal) {
            return false;
        }

        return true;
    }

    void Update_HUD_If_Applicable () {
        if (transform.root.tag == "Player" && HUDHandler.instance && healthMax > 0f) {
            HUDHandler.instance.Update_Healthbar (this);
        }
    }

    #region dying
    void Die () {
        hasDied = true;
        if (TargetLockHandler.instance && iff != null && TargetLockHandler.instance.target == iff) {
            TargetLockHandler.instance.target = null; // Clear us from targetlock since we're DED X_X
        }

        Clear_From_Targeters_Via_Damagers ();

        Update_Damage_Particles ();

        bool playerDamaged = false;
        for (int d = 0; d < damagers.Count; d++) {
            // NOTE damagers[d] can be null, if it references a non-player who's been terminated before this moment
            if (damagers[d] != null && damagers[d].transform.root.tag == "Player") {
                playerDamaged = true;
                break;
            }
        }
        if (playerDamaged) {
            //Debug.Log ("PLAYER HAS DAMAGED DYING UNIT: " + transform.root.name + " @ " + Time.time.ToString ("F2"));
            Progression_Incrementer pi = transform.root.GetComponentInChildren<Progression_Incrementer>();
            if (pi) {
                pi.Trigger_Progression_For_This ();
            }

            if (xpValue > 0 && LevelingHandler.instance != null) {
                LevelingHandler.instance.Reward_XP (xpValue);
            }
        }

        if (transform.root.tag == "Player" && CameraControl.instance != null && rootrb != null) {
            CameraControl.instance.DeathUpdate_CameraHelper (rootrb);
        }
        
        StartCoroutine (Termination_Sequence ());
    }

    void Clear_From_Targeters_Via_Damagers () {
        
        for (int d = 0; d < damagers.Count; d++) {
            if (damagers[d] != null) {
                // Clear from AIPs, if isTarget of them (since we're dead now)
                AI_Pilot[] aips = damagers[d].transform.root.GetComponentsInChildren<AI_Pilot>();
                foreach (AI_Pilot aip in aips) {
                    if (iff != null && aip.target == this.iff) {
                        aip.target = null;
                        //Debug.Log (transform.root.name + " notified " + aip + " that it is deded.");
                    }
                }

                // Clear from anti-asteroid lasers
                Utility_AutomatedAntiAsteroidLaser[] aals = damagers[d].transform.root.GetComponentsInChildren<Utility_AutomatedAntiAsteroidLaser>();
                foreach (Utility_AutomatedAntiAsteroidLaser aal in aals) {
                    if (aal != null && aal.asteroidTarget != null && aal.asteroidTarget == transform.root) {
                        //Debug.Log (transform.root.name + " is target of " + aal + " -> Clearing as asteroidTarget. " + Time.time.ToString ("F2"));
                        aal.asteroidTarget = null;
                    }
                }
            }
        }
        
    }

    IEnumerator Termination_Sequence () {
        if (explodePoints != null && explodePoints.Length > 0) {
            int explodeAttempts = Random.Range (3, 6);
            for (int e = 0; e < explodeAttempts; e++) {
                if (ReferenceHandler.instance && ReferenceHandler.instance.CalculateChance (0.5f) && Objectpooler.instance && Objectpooler.instance.explosionSmall) {
                    Instantiate (Objectpooler.instance.explosionSmall, explodePoints[e].position, Quaternion.identity);
                }
                yield return new WaitForSeconds (Random.Range (0.1f, 0.5f));
            }
        }

        yield return new WaitForSeconds (intermediateWait);

        if (deathEffects != null && deathEffects.Length > 0) {
            for (int d = 0; d < deathEffects.Length; d++) {
                Instantiate (deathEffects [d], transform.position, Quaternion.identity);
            }
        }

        if (SFX.instance && deathSounds != null && deathSounds.Length > 0) {
            SFX.instance.Play_SFX (deathSounds [Random.Range (0, deathSounds.Length)], transform.root.position);
        }

        Trigger_KillScored_Speech ();
        Deploy_Debris ();
        Deploy_SubDebris ();
        Deploy_Ragdoll ();
        Disable_Everything ();

        if (asteroid) {
            asteroid.Fracture_Asteroid ();
        }

        if (lootTable != null && Damaged_By_Player_Sufficiently () == true) {
            lootTable.transform.position = transform.position;
            lootTable.Trigger_Loot_Table ();
        }

        yield return new WaitForSeconds (minimumWait);

        yield return new WaitForSeconds (3f);

        if (transform.root.tag == "Player" && DeathHandler.instance) {
            DeathHandler.instance.Player_Death ();
        }

        Destroy (gameObject);
        yield return null;
    }

    void Trigger_KillScored_Speech () {
        var IFFs = FindObjectsOfType<IFF>();
        List<Speech_Unit> attackSpeakers = new List<Speech_Unit>();;
        for (int i = 0; i < IFFs.Length; i++) {
            if (iff != null && IFFs[i].faction != iff.faction && IFFs[i].AIP() != null && IFFs[i].AIP().target != null && IFFs[i].AIP().target == iff && IFFs[i].SPEECH() != null && IFFs[i].SPEECH().enemyKillScored.Length > 0) {
                attackSpeakers.Add (IFFs[i].SPEECH());
            }
        }

        if (attackSpeakers.Count > 0 && ReferenceHandler.instance.CalculateChance (0.4f)) {
            int attackSpeakerChoice = Random.Range (0, attackSpeakers.Count);
            SFX.instance.Play_Speech (attackSpeakers[attackSpeakerChoice].enemyKillScored [Random.Range (0, attackSpeakers[attackSpeakerChoice].enemyKillScored.Length)], attackSpeakers[attackSpeakerChoice].transform.position);
        }
    }

    public bool HasDied () {
        return hasDied;
    }

    bool Damaged_By_Player_Sufficiently () {
        // Whether we've been damaged by player significantly enough to drop loot
        // + Implemented to prevent B01 Miner Drone Controller boss from helping with farming asteroids
        float totalDamage = 0f;
        float playerDamage = 0f;
        for (int d = 0; d < damagers.Count; d++) {
            if (damageAmounts.ContainsKey (damagers[d].ToString())) {
                totalDamage += damageAmounts[damagers[d].ToString()];
                if (damagers[d].ToString() == ReferenceHandler.instance.playerIFF.transform.root.ToString ()) {
                    playerDamage += damageAmounts [damagers[d].ToString()];
                }
            }
        }
        float playerDamagePercent = 0f;
        if (totalDamage > 0f) {
            playerDamagePercent = playerDamage / totalDamage;
        }
        Debug.Log ("Player has damaged " + transform.name + " for " + playerDamage + " out of " + totalDamage + " [percent: " + playerDamagePercent + "]");
        if (playerDamagePercent >= 0.2f) {
            return true;
        }
        return false;
    }

    void Deploy_Debris () {
        if (debris == null || (debris != null && debris.Length <= 0)) {
            return;
        }

        // DEBRIS HANDLING
        List<Rigidbody> debrisRigidbodies = new List<Rigidbody>();
        for (int d = 0; d < debris.Length; d++) {
            if (ReferenceHandler.instance.CalculateChance (1f)) {
                debris[d].SetActive (true);
                debris[d].transform.SetParent (null);
                debris[d].AddComponent<Rigidbody>();
                debrisRigidbodies.Add (debris[d].GetComponent<Rigidbody>());
                float height = 10f;
                if (ReferenceHandler.instance.CalculateChance (0.5f)) {
                    height = -height;
                }
                if (ReferenceHandler.instance.CalculateChance (0.25f)) {
                    height = 0f;
                }
                debris[d].transform.position = new Vector3 (debris[d].transform.position.x, height, debris[d].transform.position.z);
                debris[d].AddComponent<SectorSwitch_Despawn>();
            }
        }
        foreach (Rigidbody drb in debrisRigidbodies) {
            drb.useGravity = false;
            drb.constraints = RigidbodyConstraints.FreezePositionY;
            drb.drag = Random.Range (0.1f, 0.2f);

            if (rootrb != null) {
                drb.velocity = rootrb.velocity * Random.Range (0.3f, 1f);
            }

            float randomForce = 100f;
            drb.AddForce (new Vector3 (Random.Range (-randomForce, randomForce), 0f, Random.Range (-randomForce, randomForce)) * Time.deltaTime, ForceMode.VelocityChange);

            float torque = 1000f;
            drb.AddTorque (new Vector3 (Random.Range (-torque, torque), Random.Range (-torque, torque), Random.Range (-torque, torque)) * Time.deltaTime);
        }
    }
    
    void Deploy_SubDebris () {
        if (subdebris.Length > 0) {
            for (int s = 0; s < subdebris.Length; s++) {
                subdebris[s].Deploy_SubDebris ();
            }
        }
    }

    void Deploy_Ragdoll () {
        if (ReferenceHandler.instance != null && ReferenceHandler.instance.ragdollPrefab != null) {
            int ragdollAmount = Random.Range (ragdollsMin, ragdollsMax);
            for (int r = 0; r < ragdollAmount; r++) {
                Quaternion spawnRot = Quaternion.Euler (Random.Range (-180f, 180f), Random.Range (-180f, 180f), Random.Range (-180f, 180f));
                GameObject ragdoll = Instantiate (ReferenceHandler.instance.ragdollPrefab, transform.position, spawnRot) as GameObject;
                ragdoll.name = ReferenceHandler.instance.ragdollPrefab.name;
                // RootRB-Velocity ?
                // Tumbling-Spin ?
            }
        }
    }

    void Disable_Everything () {
        if (iff != null && iff.CON_Ref() != null) {
            iff.CON_Ref().Set_Vertical (0f);
        } else {
            Controls con = transform.GetComponent<Controls>();
            if (con) {
                con.Set_Vertical (0f);
                con.enabled = false;
            }
        }

        Collider[] cols = transform.GetComponentsInChildren<Collider>();
        for (int c = 0; c < cols.Length; c++) {
            cols[c].enabled = false;
        }

        Rigidbody rb = transform.GetComponent<Rigidbody>();
        if (rb) {
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        MeshRenderer[] meshes = transform.GetComponentsInChildren<MeshRenderer>();
        for (int m = 0; m < meshes.Length; m++) {
            meshes[m].enabled = false;
        }

        SkinnedMeshRenderer[] skins = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int s = 0; s < skins.Length; s++) {
            skins[s].enabled = false;
        }

        if (iff != null && iff.track != null) {
            //iff.track.Assign_Target (null); // This breaks on asteroid-destroy for some reason :/
            iff.track.Clear_Target ();
        }

        AI_Pilot[] aips = transform.root.GetComponentsInChildren<AI_Pilot>();
        for (int aip = 0; aip < aips.Length; aip++) {
            Destroy (aips[aip]);
        }

        AI_Weapon[] aiweps = transform.root.GetComponentsInChildren<AI_Weapon>();
        for (int aiw = 0; aiw < aiweps.Length; aiw++) {
            aiweps[aiw].StopAllCoroutines ();
            aiweps[aiw].enabled = false;
        }

        AI_Weapon_Flak[] aiflaks = transform.root.GetComponentsInChildren<AI_Weapon_Flak>();
        for (int aif = 0; aif < aiflaks.Length; aif++) {
            aiflaks[aif].StopAllCoroutines ();;
            aiflaks[aif].enabled = false;
        }

        // Find all extra particlesystems and ensure they're zeroed out, since we're dying ? e.g. maneuverThrusters
    }

    #endregion dying

}
