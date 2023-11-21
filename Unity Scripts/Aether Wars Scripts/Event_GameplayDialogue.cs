using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_GameplayDialogue : MonoBehaviour
{
    [Header ("References")]
    public string descriptionText;
    public string[] optionTexts = new string[5];
    public Event_GameplayDialogue[] optionFollowups = new Event_GameplayDialogue[5];

    [Header ("Rewards")] // When Give_Rewards () is triggered, these items are granted to the player
    public GameObject[] spawnables;
    public int creditsAdjust;
    public AudioClip[] conclusionSounds;
    public bool sequentialSounds;

    public void Activate_Event () {
        if (EventHandler.instance) {
            EventHandler.instance.Setup_Event (descriptionText, optionTexts, optionFollowups);

            EventHandler.instance.currentEvent = this;

            EventHandler.instance.Start_Event ();
        }
    }

    public void Give_Rewards () {
        if (PlayerProfile.instance) {
            PlayerProfile.instance.playerCredits += creditsAdjust;
        }

        StartCoroutine (Conclusion_Sounds_Sequence ());
    }

    IEnumerator Conclusion_Sounds_Sequence () {
        if (SFX.instance) {
            for (int s = 0; s < conclusionSounds.Length; s++) {
                SFX.instance.Play_SFX (conclusionSounds[s], Camera.main.transform.position);
                if (sequentialSounds) {
                    yield return new WaitForSeconds (conclusionSounds[s].length + 0.5f);
                }
            }
        }

        yield return null;
    }

}
