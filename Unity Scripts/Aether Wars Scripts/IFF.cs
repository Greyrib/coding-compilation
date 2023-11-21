using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFF : MonoBehaviour
{
    public enum Faction {Alliance, Federation, Civilian, Galaxy, Republic, Legion, Empire, Mercenaries, Pyrates, Salvagers};

    [Header ("IFF Data")]
    public Faction faction;
    public string unitType;
    public bool autoUntargetable;
    [Space (5)]
    public GameObject[] factionWarpEffects; // When warping, only if array-length matches faction & that spot's effect isn't null, will it be used
    public bool usesSpeech;

    [Header ("Track Visibility")]
    public bool doNotRequestTrack; // Used by flares, as they need an IFF to continously redirect projectiles
    [Space (5)]
    public bool showClampedCorners;
    public bool showClampedDiamond = true;
    public bool showClampedText;
    public bool showClampedBackground;
    public bool showClampedHealthbar;
    public bool showClampedShieldbar;
    [Space (5)]
    public bool showUnclampedCorners = true;
    public bool showUnclampedDiamond;
    public bool showUnclampedText = true;
    public bool showUnclampedBackground = true;
    public bool showUnclampedHealthbar = true;
    public bool showUnclampedShieldbar = true;

    [Header ("Special Parameters")]
    public bool dontDespawnOnWarp;

    [Header ("Dynamics")]
    public Radar_Track track;

    Health hp;
    Rigidbody rb;
    public Controls con;
    Controls_Lights lightcon;
    Ship_Description shipdesc;
    Cargo cargo;

    AI_Pilot aip;

    Speech_Unit speech;

    void Awake () {
        Acquire_References ();
    }

    public void Acquire_References () {
        hp = transform.root.GetComponent<Health>();
        rb = transform.root.GetComponent<Rigidbody>();
        con = transform.root.GetComponent<Controls>();
        lightcon = transform.root.GetComponentInChildren<Controls_Lights>();
        shipdesc = transform.root.GetComponent<Ship_Description>();
        cargo = transform.root.GetComponent<Cargo>();

        aip = GetComponent<AI_Pilot>();
    }

    void Start () {
        Get_Radar_Track ();
        if (transform.root.tag != "Player" && dontDespawnOnWarp == false) {
            EventSystem_Custom.OnSectorSwitch += DeExistify;
            if (usesSpeech == true && SpeechHandler.instance != null) {
                speech = SpeechHandler.instance.Request_Unit_Speech (transform.root.gameObject);
            }
        }
    }

    void OnDisable () {
        if (transform.root.tag != "Player" && dontDespawnOnWarp == false) {
            EventSystem_Custom.OnSectorSwitch -= DeExistify;
        }
    }

    void DeExistify () {
        // Remove ourselves from radartracks, if applicable
        if (RadarHandler.instance != null && RadarHandler.instance.tracks != null && RadarHandler.instance.tracks.Count > 0) {
            for (int t = 0; t < RadarHandler.instance.tracks.Count; t++) {
                if (RadarHandler.instance.tracks[t] != null && RadarHandler.instance.tracks[t].target != null && RadarHandler.instance.tracks[t].target == this) {
                    RadarHandler.instance.tracks[t].Clear_Target ();
                    break;
                }
            }
        }

        // De-register us as an activeSpeaker
        if (speech != null && SpeechHandler.instance != null && SpeechHandler.instance.activeSpeakers != null) {
            if (SpeechHandler.instance.activeSpeakers.Contains (speech)) {
                SpeechHandler.instance.activeSpeakers.Remove (speech);
            }
        }
        

        Destroy (gameObject);
    }

    #region references
    public Health HP_Ref () {
        return hp;
    }

    public Rigidbody RB_Ref () {
        return rb;
    }

    public Controls CON_Ref () {
        return con;
    }

    public Controls_Lights LIGHTCON_Ref () {
        return lightcon;
    }

    public Ship_Description SHIPDESC () {
        return shipdesc;
    }

    public Cargo CARGO () {
        return cargo;
    }

    public Speech_Unit SPEECH () {
        return speech;
    }

    public AI_Pilot AIP () {
        return aip;
    }
    #endregion references

    #region radar
    void Get_Radar_Track () {
        if (doNotRequestTrack) {
            return;
        }

        if (RadarHandler.instance && track == null && transform.root.tag != "Player") {
            track = RadarHandler.instance.Request_Tracker (this);

            if (track) {
                track.showClampedCorners = showClampedCorners;
                track.showClampedDiamond = showClampedDiamond;
                track.showClampedText = showClampedText;
                track.showClampedBackground = showClampedBackground;
                track.showClampedHealthbar = showClampedHealthbar;
                track.showClampedShieldbar = showClampedShieldbar;

                track.showUnclampedCorners = showUnclampedCorners;
                track.showUnclampedDiamond = showUnclampedDiamond;
                track.showUnclampedText = showUnclampedText;
                track.showUnclampedBackground = showUnclampedBackground;
                track.showUnclampedHealthbar = showUnclampedHealthbar;
                track.showUnclampedShieldbar = showUnclampedShieldbar;
            }
        }
    }
    #endregion radar

}
