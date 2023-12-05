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
    public AudioClip tickClip;
    public TextMeshProUGUI transferringText;
    [Space (5)]
    public Camera texCam;
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
                if (CrosshairControl.instance != null) {
                    CrosshairControl.instance.Set_CursorMode (1);
                }
            }
        }

        Lerp_FundsTransferringText_IfEnabled ();
    }

    void Lerp_FundsTransferringText_IfEnabled () {
        if (transferringText != null && transferringText.enabled == true && menuBooting) {
            float alpha = Mathf.Abs (Mathf.Sin (Time.time * 3f));
            transferringText.color = new Color (transferringText.color.r, transferringText.color.g, transferringText.color.b, alpha);
        } else if (transferringText != null && transferringText.enabled == true) {
            transferringText.enabled = false;
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

        // ------------ INITIAL SETUP ------------
        hangarMenuObject.SetActive (true);
        if (CrosshairControl.instance != null) {
            CrosshairControl.instance.Set_CursorMode (2);
        }

        Position_TexCam ();

        // TODO Create_Selection_List() || E.g. 'Switch Aeroframe - Primary - Secondary - Secondary - Tertiary - Auxilliary' etc.
        // > Follow-up to above ; make Create_ModuleChoices () || Left-wise expanding (more appearing, really) menu, of choices of modules available for selected moduleslot / modulegroup [NOTE this isn't really for calling here - this is just a note for making it]
        // ---------------------------------------

        // Commence moneytransferring
        fundsEarned.text = PlayerProfile.instance.scoreAttained.ToString();
        fundsBanked.text = "0$";
        if (transferringText != null && transferringText.enabled == false)
            transferringText.enabled = true;
        int fundsTransferred = 0;
        while (PlayerProfile.instance.scoreAttained > 0) {
            int rate = 1;
            fundsTransferred += rate;
            PlayerProfile.instance.scoreAttained -= rate;
            fundsBanked.text = $"{fundsTransferred}$";
            fundsEarned.text = PlayerProfile.instance.scoreAttained.ToString();

            // TODO credit tick SFX
            if (SFX.instance != null && tickClip != null) {
                SFX.instance.Play_UI_Sound (tickClip);
            }
            //yield return new WaitForEndOfFrame ();
            yield return new WaitForSeconds (0.01f); // NOTE Beware tickClip's length compared to this wait-value
        }
        PlayerProfile.instance.creditsBanked += fundsTransferred;
        PlayerProfile.instance.scoreAttained = 0;
        // TODO Save (PlayerProfile) data here ?
        yield return new WaitForSeconds (1f);

        // Might want to adjust this and/or the delay above [currently using 'Transferring' text as indicator for when menuBooting turns to false so we can interact again]
        if (transferringText != null && transferringText.enabled == true)
            transferringText.enabled = false;

        menuBooting = false;
        yield return null;
    }

    void Position_TexCam () {
        if (texCam != null && MatchHandler.instance != null && MatchHandler.instance.player != null) {
            Transform player = MatchHandler.instance.player.transform;
            texCam.transform.position = player.position + (player.forward * 0f) + (player.up * 5f);
        }
    }

}
