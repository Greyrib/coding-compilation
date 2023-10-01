using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem_Custom : MonoBehaviour
{
    public static EventSystem_Custom instance;

    void Awake ()
    {
        if (instance == null) instance = this; else Destroy (this);
    }

    void Start ()
    {
        
    }

    #region events
    // GENERIC TEMPLATE BEGIN ------------------------
    public delegate void Generic ();
    public static event Generic OnGeneric;

    public void Event_Generic ()
    {
        if (OnGeneric != null)
            OnGeneric ();
    }
    // GENERIC TEMPLATE END --------------------------

    public delegate void MatchStart ();
    public static event MatchStart OnMatchStart;

    public void Event_MatchStart ()
    {
        if (OnMatchStart != null) {
            OnMatchStart ();
            Debug.Log ("Event_MatchStart");
        }
    }

    #endregion events

}
