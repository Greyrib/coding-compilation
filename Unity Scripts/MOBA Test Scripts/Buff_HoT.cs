using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff_HoT : MonoBehaviour
{
    [Header ("Dynamics")]
    public string buffOrigin;
    public float healAmount;
    public int healTicksLeft;
    [Space (5)]
    public Health rootHealth;

    public void Activate_HoT (string newBuffOrigin, float howMuchHealingTotal, int howManyTicks) {
        buffOrigin = newBuffOrigin;
        healAmount = howMuchHealingTotal;
        healTicksLeft = howManyTicks;
        if (transform.root != null && transform.root != transform) {
            rootHealth = transform.root.GetComponent<Health>();
            StartCoroutine (HoT_Tick ());
        } else {
            Debug.LogError ("Buff_HoT couldn't find a root - Self-destroying");
            Destroy (gameObject);
        }
    }

    IEnumerator HoT_Tick () {
        yield return new WaitForSeconds (1f);
        if (healTicksLeft > 0) {
            healTicksLeft -= 1;
            if (rootHealth != null) {
                rootHealth.health += healAmount;
            }
        }
        if (healTicksLeft > 0) {
            StartCoroutine (HoT_Tick ());
        } else {
            Destroy (gameObject);
        }
        yield return null;
    }

}
