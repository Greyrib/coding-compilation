using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairControl : MonoBehaviour
{
    public static CrosshairControl instance;

    public LayerMask crosshairLayers;

    [Header ("Parameters")]
    public LayerMask unitRaycastLayers;

    [Header ("Dynamics")]
    public Transform cursorFriendlyUnit;
    public Transform cursorHostileUnit;

    Plane zeroplane; // Used for cursor-placing [Method 2]

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        zeroplane = new Plane (Vector3.up, Vector3.zero); // Create zeroplane for Method 2
    }

    void Update () {
        Raycast_Positioning ();
        Raycast_UnitIdentifying ();
        Unit_Inputs ();
    }

    void Raycast_Positioning () {
        // New Method - based on (infinite) zero-plane
        Ray mouseRay = Camera.main.ScreenPointToRay (Input.mousePosition);
        float distance = 0f;
        if (zeroplane.Raycast (mouseRay, out distance)) {
            transform.position = mouseRay.GetPoint (distance);
        }

        // Old Method - Only raycast on terrain-ground
        /*
        RaycastHit obstacle;
        if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out obstacle, 1000f, crosshairLayers) && obstacle.transform != null) {
            transform.position = obstacle.point;
        }
        */
    }

    void Raycast_UnitIdentifying () {
        // Probs need to clear both before iterating
        cursorFriendlyUnit = null;
        cursorHostileUnit = null;
        // Raycast to find present friendly & hostile unit-transforms at cursor current position
        RaycastHit[] obstacles = Physics.RaycastAll (Camera.main.ScreenPointToRay (Input.mousePosition), 500f, unitRaycastLayers);
        // TODO Distance-Sorting down the line ? Simply taking first encountered that fits friendly/hostile for now
        List<Transform> ignores = new List<Transform>();
        for (int o = 0; o < obstacles.Length; o++) {
            if (!ignores.Contains (obstacles[o].transform.root)) {
                IFF oiff = obstacles[o].transform.root.GetComponent<IFF>();
                // TODO Are we interested in separating 'Player'-tagged, in relation to self-cast ?
                if (oiff != null && cursorFriendlyUnit == null && oiff.faction == IFF.Faction.Blue) {
                    cursorFriendlyUnit = obstacles[o].transform.root;
                } else if (oiff != null && cursorHostileUnit == null && oiff.faction == IFF.Faction.Red) {
                    cursorHostileUnit = obstacles[o].transform.root;
                }
                ignores.Add (obstacles[o].transform.root);
                if (cursorFriendlyUnit != null && cursorHostileUnit != null) {
                    break; // We found both, so no need to keep iterating
                }
            }
        }
    }

    void Unit_Inputs () {
        if (Input.GetKeyDown (KeyCode.Mouse0)) {
            // Move order
            if (PlayerProfile.instance != null && PlayerProfile.instance.playersUnit != null) {
                PlayerProfile.instance.playersUnit.Move_Order (transform.position);
            }
        }
    }

}
