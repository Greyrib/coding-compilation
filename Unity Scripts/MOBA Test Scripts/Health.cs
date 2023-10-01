using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100f;
    public float healthMax = 100f;

    void Update () {
        if (Input.GetKeyDown (KeyCode.Backspace)) {
            health -= 25f;
        }
    }

    public void Damage (float amount) {
        health -= amount;

        if (health < 0f) {
            Die ();
        }
    }

    void Die () {
        // If we're a HERO unit, ASPLODE and start our respawntimer for our owner

        // All else : vanisj!
        StartCoroutine (Disable_Sequence ());
    }

    IEnumerator Disable_Sequence () {
        // TODO Calc minDisableWait
        Destroy (gameObject);
        yield return null;
    }
    
}
