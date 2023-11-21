using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Morse : MonoBehaviour
{
    // LEGACY NOTE Testing sequence used 'GAL367' - maybe use this at some later stage (i.e. special mission's code or something)
    [Header ("References")]
    public AudioSource asource;
    public string sequence; // Letters denoting morse to 'transmit'; S for short, L for long, D for delay

    [Header ("Parameters")]
    public float interval = 3f; // Inbetween how long do we retransmit the sequence
    public float asourceVolumeWhenMorseing = 0.3f;
    [Space (5)]
    public float shortDelay = 0.08f;
    public float longDelay = 0.3f;
    public float delayDelay = 0.3f;

    string codeSequence;

    void Start () {
        Reboot_Morse ();
    }

    void OnEnable () {
        Reboot_Morse ();
        EventSystem_Custom.OnSectorSwitch += Reboot_Morse;
    }

    void OnDisable () {
        EventSystem_Custom.OnSectorSwitch -= Reboot_Morse;
    }

    public void Reboot_Morse () {
        StopAllCoroutines ();
        codeSequence = Translate (sequence);
        StartCoroutine (Interpret_Sequence ());
    }

    IEnumerator Interpret_Sequence () {
        for (int s = 0; s < codeSequence.Length; s++) {
            if (codeSequence[s].ToString() == "S") {
                asource.volume = asourceVolumeWhenMorseing;
                yield return new WaitForSeconds (shortDelay);
            } else if (codeSequence[s].ToString() == "L") {
                asource.volume = asourceVolumeWhenMorseing;
                yield return new WaitForSeconds (longDelay);
            } else if (codeSequence[s].ToString() == "D") {
                yield return new WaitForSeconds (delayDelay);
            }
            asource.volume = 0f;
            yield return new WaitForSeconds (0.1f);
        }

        yield return new WaitForSeconds (interval);
        StartCoroutine (Interpret_Sequence ());
        yield return null;
    }

    public string Translate (string input) {
        string result = "";
        string[] dictionary = new string[] {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"};
        string[] morsecode = new string [] {"SL", "LSSS", "LSLS", "LSS", "S", "SSLS", "LLS", "SSSS", "SS", "SLLL", "LSL", "SLSS", "LL", "LS", "LLL", "SLLS", "LLSL", "SLS", "SSS", "L", "SSL", "SSSL", "SLL", "LSSL", "LSLL", "LLSS", "SLLLL", "SSLLL", "SSSLL", "SSSSL", "SSSSS", "LSSSS", "LLSSS", "LLLSS", "LLLLS", "LLLLL"};
        for (int l = 0; l < input.Length; l++) {
            //Debug.Log ("Going FROM: " + result + " ...");
            for (int d = 0; d < dictionary.Length; d++) {
                if (dictionary[d] == input[l].ToString()) {
                    result = result + morsecode[d];
                    break;
                }
            }
            result = result + "D";
            //Debug.Log ("... TO: " + result);
        }

        //Debug.Log ("Converted input: " + input + " to " + result);

        return result;
    }

}
