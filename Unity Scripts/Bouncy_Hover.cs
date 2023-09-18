using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncy_Hover : MonoBehaviour
{
    // A class for simply "bouncy_hovering" an object up and down, based off its Awake world position
    [Header ("Parameters")]
    public float amplitude = 0.5f;
    public float speed = 1.5f;

    Vector3 originPos;

    void Awake () {
        originPos = transform.position;
    }

    void Update () {
        float sin = Mathf.Sin (Time.time * speed) * amplitude;
        transform.position = originPos + (Vector3.up * sin);
    }

}
