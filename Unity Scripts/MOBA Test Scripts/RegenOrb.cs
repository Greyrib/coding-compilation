using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenOrb : MonoBehaviour
{
    // TODO Detect faction of Triggerist, scan for WithinRange(SimilarFactioneers) & apply health- & energy-regen

    public IFF.Faction orbFaction = IFF.Faction.Neutral;

    void OnTriggerEnter (Collider col) {
        Debug.Log ("Someone entered a RegenOrb's TriggerArea...");
        IFF ciff = col.transform.root.GetComponent<IFF>();
        if (ciff != null && FactionCanTriggerOrb (ciff.faction)) {
            Activate_Orb_ForFaction (ciff.faction);
        }
    }

    bool FactionCanTriggerOrb (IFF.Faction triggererFaction) {
        if (triggererFaction == IFF.Faction.Blue && orbFaction == IFF.Faction.Blue) {
            return true;
        } else if (triggererFaction == IFF.Faction.Red && orbFaction == IFF.Faction.Red) {
            return true;
        } else if (triggererFaction == IFF.Faction.Third && orbFaction == IFF.Faction.Third) {
            return true;
        } else if (orbFaction == IFF.Faction.Neutral) {
            return true;
        }
        return false;
    }

    void Activate_Orb_ForFaction (IFF.Faction triggerFaction) {
        Collider[] possibles = Physics.OverlapSphere (transform.position, 10f, ReferenceHandler.instance.unitLayers);
        List<Transform> ignores = new List<Transform>();
        List<IFF> affecteds = new List<IFF>();
        for (int p = 0; p < possibles.Length; p++) {
            if (!ignores.Contains (possibles[p].transform.root)) {
                IFF piff = possibles[p].transform.root.GetComponent<IFF>();
                if (piff != null && piff.faction == triggerFaction && affecteds.Contains (piff) == false) {
                    affecteds.Add (piff);
                }
                ignores.Add (possibles[p].transform.root);
            }
        }

        // TODO Apply regen buff to health & mana for all _affecteds_

        // TODO Activation VFX (HOTS = miniorb-flytowards-allaffectedheroes)

        Destroy (gameObject);
    }

}
