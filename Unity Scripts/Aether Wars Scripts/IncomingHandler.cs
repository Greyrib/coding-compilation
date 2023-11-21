using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingHandler : MonoBehaviour
{
    public static IncomingHandler instance;

    [Header ("References")]
    public GameObject incomingPrefab;
    public Transform parent;
    [Space (5)]
    public GameObject warningObject;
    public AudioSource asourceWarning;

    [Header ("Parameters")]
    public float scanrangePerLevel = 50f;

    [Header ("Dynamics")]
    public int systemLevel; // Higher level = more warnings/data on incoming threats
    public bool incomingDetected; // Used for blinking HUD warning
    [Space (5)]
    public List<Incoming_Track> tracks;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        tracks = new List<Incoming_Track>();
        if (asourceWarning) {
            asourceWarning.volume = 0f;
            asourceWarning.loop = true;
            asourceWarning.Play ();
        }
    }

    void Start () {
        StartCoroutine (Warning_Flash_Tick ());
        StartCoroutine (Incoming_Tick ());
    }

    IEnumerator Warning_Flash_Tick () {
        if (warningObject) {
            if (incomingDetected) {
                warningObject.SetActive (true);
            }
            yield return new WaitForSeconds (0.4f);
            warningObject.SetActive (false);
        }
        yield return new WaitForSeconds (0.2f);
        StartCoroutine (Warning_Flash_Tick ());
        yield return null;
    }

    IEnumerator Incoming_Tick () {
        if (ReferenceHandler.instance && ReferenceHandler.instance.playerIFF != null && ReferenceHandler.instance.playerIFF.RB_Ref () != null) {
            Collider[] possibles = Physics.OverlapSphere (ReferenceHandler.instance.playerIFF.RB_Ref ().position, scanrangePerLevel * systemLevel, ReferenceHandler.instance.projectileLayers);
            List<Transform> ignores = new List<Transform>();
            List<Projectile> incoming = new List<Projectile>();
            for (int p = 0; p < possibles.Length; p++) {
                if (!ignores.Contains (possibles[p].transform.root)) {
                    Projectile pro = possibles[p].transform.root.GetComponent<Projectile>();
                    if (pro && pro.turnrate > 0f && !pro.HasHit () && pro.homingTarget != null && pro.homingTarget.transform.root.tag == "Player" && HasHomingTimeLeft (pro)) {
                        incoming.Add (pro);
                    }
                    ignores.Add (possibles[p].transform.root);
                }
            }
            //Debug.Log ("Incoming_Tick | Possibles " + possibles.Length + " | Incoming " + incoming.Count + " @ " + Time.time.ToString ("F2"));

            if (incoming.Count > 0) {
                incomingDetected = true;

                for (int i = 0; i < incoming.Count; i++) {
                    Assign_Projectile_Tracker (incoming[i]);
                }
            } else {
                incomingDetected = false;
            }
        }

        if (incomingDetected) {
            if (asourceWarning && asourceWarning.volume < 0.3f) {
                asourceWarning.volume = 0.3f;
            }
        } else if (!incomingDetected) {
            if (asourceWarning && asourceWarning.volume > 0f) {
                asourceWarning.volume = 0f;
            }
        }

        yield return new WaitForSeconds (0.1f);
        StartCoroutine (Incoming_Tick ());
        yield return null;
    }

    bool HasHomingTimeLeft (Projectile proj) {
        if (proj != null && proj.homingTimeDecay == true && proj.homingTime > 0f) {
            return true;
        } else if (proj != null && proj.homingTimeDecay == false) { // Considered having 'proj.turnrate > 0' here, but since this method is for homingTimeLeft only, no reason (besides, it's in the coroutine above anyway)
            return true;
        }

        return false;
    }

    public void Assign_Projectile_Tracker (Projectile newTrack) {
        bool wasAssigned = false;

        if (tracks.Count > 0) {
            // Check if newTrack is already assigned somewhere
            for (int a = 0; a < tracks.Count; a++) {
                if (tracks[a].track != null && tracks[a].track == newTrack) {
                    wasAssigned = true;
                    break;
                }
            }

            // If we're not already assigned, look for tracks with no current track and assign us there
            if (!wasAssigned) {
                for (int t = 0; t < tracks.Count; t++) {
                    if (tracks[t].track == null) {
                        tracks[t].Assign_New_Track (newTrack);
                        wasAssigned = true;
                        break;
                    }
                }
            }
        }

        // If we reach here unassigned, we need a new tracker specifically for newTrack, so create it, add to list & assign newTrack as new track for that
        if (!wasAssigned) {
            GameObject newTracker = Instantiate (incomingPrefab, parent) as GameObject;
            newTracker.name = incomingPrefab.name;
            //newTrack.transform.SetParent (parent);
            Incoming_Track ic = newTracker.GetComponent<Incoming_Track>();
            if (ic) {
                tracks.Add (ic);
                ic.Assign_New_Track (newTrack);
            }
        }
    }

    public void ProjectileLostTarget_ClearFromTracks_IfApplicable (Projectile proj) {
        if (proj != null && tracks != null && tracks.Count > 0) {
            for (int t = 0; t < tracks.Count; t++) {
                if (tracks[t] != null && tracks[t].track != null && tracks[t].track == proj) {
                    tracks[t].track = null;
                    break;
                }
            }
        }
    }

}
