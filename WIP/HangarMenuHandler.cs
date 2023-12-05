using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HangarMenuHandler : MonoBehaviour
{
    public static HangarMenuHandler instance;

    [Header ("References")]
    public TextMeshProUGUI homebaseInteractText;
    public GameObject hangarMenuObject;
    [Space (5)] // Stuff for funds-handling
    public TextMeshProUGUI fundsEarned;
    public TextMeshProUGUI fundsBanked;
    [Space (5)] // Stuff for initial list
    public GameObject genericButtonPrefab;

    [Header ("Dynamics")]
    public bool playerSecured; // Whether player is parked on homebase-platform and at zero-inputs & zero-velocity'd

    public bool menuBooting;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        menuBooting = false;
        if (hangarMenuObject && hangarMenuObject.activeSelf)
            hangarMenuObject.SetActive (false);
    }

    void Update () {
        if (Input.GetKeyDown (KeyCode.E) && homebaseInteractText != null && homebaseInteractText.enabled == true) {
            if (hangarMenuObject && hangarMenuObject.activeSelf == false && menuBooting == false) {
                StartCoroutine (Menu_BootSequence ());
            } else if (hangarMenuObject && hangarMenuObject.activeSelf == true && menuBooting == false) {
                hangarMenuObject.SetActive (false);
            }
        }
    }

    void LateUpdate () {
        if (playerSecured == true && homebaseInteractText != null && homebaseInteractText.enabled == false) {
            homebaseInteractText.enabled = true;
        } else if (playerSecured == false && homebaseInteractText != null && homebaseInteractText.enabled == true) {
            homebaseInteractText.enabled = false;
        }
    }

    IEnumerator Menu_BootSequence () {
        menuBooting = true;
        hangarMenuObject.SetActive (true);

        // Commence moneytransferring
        fundsEarned.text = PlayerProfile.instance.scoreAttained.ToString();
        fundsBanked.text = "0$";
        int fundsTransferred = 0;
        while (PlayerProfile.instance.scoreAttained > 0) {
            int rate = 1;
            fundsTransferred += rate;
            PlayerProfile.instance.scoreAttained -= rate;
            fundsBanked.text = $"{fundsTransferred}$";
            // TODO credit tick SFX
            yield return new WaitForEndOfFrame ();
        }
        PlayerProfile.instance.creditsBanked += fundsTransferred;
        PlayerProfile.instance.scoreAttained = 0;
        // TODO Save (PlayerProfile) data here ?
        yield return new WaitForSeconds (1f);


        menuBooting = false;
        yield return null;
    }

}
