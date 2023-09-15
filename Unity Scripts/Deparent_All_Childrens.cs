using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deparent_All_Childrens : MonoBehaviour
{
    public enum OccurMoment {None, Awake, Start, OnEnable};

    public OccurMoment occurMoment;

    void Awake () {
        if (occurMoment == OccurMoment.Awake) {
            transform.DetachChildren ();
        }
    }

    void Start () {
        if (occurMoment == OccurMoment.Start) {
            transform.DetachChildren ();
        }
    }

    void OnEnable () {
        if (occurMoment == OccurMoment.OnEnable) {
            transform.DetachChildren ();
        }
    }

}
