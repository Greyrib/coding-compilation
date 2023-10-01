using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header ("References")]
    public ParticleSystem[] particles;
    public TrailRenderer trail; // To calculate minDieWaitTime
    public MeshRenderer[] meshrenders;

    [Header ("Parameters")]
    public float damage = 1f;
    public float progressRate = 1f;

    [Header ("End Target")]
    public Vector3 startPos;
    public Vector3 endPos; // For non-targeted skillshots
    public Transform endUnit;

    [Header ("Dynamics")]
    [Range (0f, 1f)] public float projectileProgress;
    public bool projectileActive; // Whether we're progressing (from start to end/endUnit && 0-1 [projectileProgress])
    public float startToEndDist;

    float minDieWaitTime; // The minimum amount of time from ACTIVATING to DESTROY (dependant on trails, particles etc.)

    public void Activate_Projectile (Vector3 newStartPos, Vector3 newEndPos, Transform newEndUnit) {
        startPos = new Vector3 (newStartPos.x, 0f, newStartPos.z);
        endPos = new Vector3 (newEndPos.x, 0f, newEndPos.z);
        endUnit = newEndUnit;
        startToEndDist = Vector3.Distance (startPos, endPos);

        Calculate_MinDieWaitTime ();

        projectileProgress = 0f;
        projectileActive = true;
    }

    void Calculate_MinDieWaitTime () {
        minDieWaitTime = 0f;

        for (int p = 0; p < particles.Length; p++) {
            if (particles[p].main.startLifetime.constantMax > minDieWaitTime) {
                minDieWaitTime = particles[p].main.startLifetime.constantMax;
            }
        }

        if (trail != null && trail.time > minDieWaitTime) {
            minDieWaitTime = trail.time;
        }
    }

    void FixedUpdate () {
        if (projectileActive == true) {
            projectileProgress += progressRate * Time.deltaTime;
            projectileProgress = Mathf.Clamp (projectileProgress, 0f, 1f);
            Position_Projectile();
            if (projectileProgress >= 1f) {
                Projectile_EndReached ();
            }
        }
    }

    void Position_Projectile () {
        Ray dirRay = new Ray (startPos, endPos - startPos);
        Debug.DrawLine (startPos, dirRay.GetPoint (startToEndDist), Color.yellow, 0.05f); // Debug-Draw the dirRay
        if (endUnit != null) {
            float startToEndDist_TargetDynamic = Vector3.Distance (startPos, endUnit.transform.position);
            transform.position = dirRay.GetPoint (startToEndDist_TargetDynamic * projectileProgress);
        } else {
            transform.position = dirRay.GetPoint (startToEndDist * projectileProgress);
        }
        
        Debug.DrawLine (endPos, endPos + Vector3.up * 0.5f, Color.magenta, 0.05f); // Debug-Draw the end position
    }

    void Projectile_EndReached () {
        Deactivate_Visuals ();

        Deal_Damage ();

        //Debug.Log ("Debug - Projectile reached its endpoint.");
        StartCoroutine (Disable_Sequence ());
    }

    void Deactivate_Visuals () {
        for (int ps = 0; ps < particles.Length; ps++) {
            ReferenceHandler.instance.SetEmissionRate (particles[ps], 0f);
            ReferenceHandler.instance.SetEmissionRate2 (particles[ps], 0f);
            ReferenceHandler.instance.SetEmissionRateOverDistance (particles[ps], 0f);
        }

        for (int mr = 0; mr < meshrenders.Length; mr++) {
            meshrenders[mr].enabled = false;
        }
    }

    void Deal_Damage () {
        // TODO Add collided-with enemy-targets to damageCollidees-List

        if (endUnit != null) {
            Health ehp = endUnit.transform.root.GetComponent<Health>();
            if (ehp != null) {
                ehp.Damage (damage);
            }
        }
    }

    IEnumerator Disable_Sequence () {
        yield return new WaitForSeconds (minDieWaitTime);
        Destroy (gameObject);
        yield return null;
    }

}
