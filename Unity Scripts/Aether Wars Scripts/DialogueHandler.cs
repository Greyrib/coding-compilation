using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class DialogueHandler : MonoBehaviour
{
    public static DialogueHandler instance;

    [Header ("References")]
    public TextMeshProUGUI subtitleSpeakerText;
    public TextMeshProUGUI subtitlesText;
    public AudioSource dialogueAsource;

    [Header ("Dynamics")]
    public bool dialogueSessionInProgress; // Whether we're currently playing through clips
    public Dialogue_Session currentSession; // Which dialogue_session we're currently playing
    public int currentSessionChoice; // Which of the currentSession's steps we are at; if above clip-length, we are finished

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        if (subtitlesText) {
            subtitlesText.text = "";
        }
        if (subtitleSpeakerText) {
            subtitleSpeakerText.text = "";
        }
    }

    public void Play_Dialogue_Session (Dialogue_Session newSession) {
        if (dialogueSessionInProgress) {
            return;
        } else {
            currentSession = newSession;
            currentSessionChoice = -1;
            dialogueSessionInProgress = true;
            subtitleSpeakerText.color = new Color (subtitleSpeakerText.color.r, subtitleSpeakerText.color.g, subtitleSpeakerText.color.b, 1f);
            subtitlesText.color = new Color (subtitlesText.color.r, subtitlesText.color.g, subtitlesText.color.b, 1f);
            StartCoroutine (Session_Play_Sequence ());
        }
    }

    IEnumerator Session_Play_Sequence () {
        currentSessionChoice += 1;
        if (dialogueAsource) {
            dialogueAsource.PlayOneShot (currentSession.clips [currentSessionChoice]);
            subtitleSpeakerText.text = currentSession.speakerNames [currentSessionChoice];
            subtitlesText.text = currentSession.subtitles [currentSessionChoice];
            yield return new WaitForSeconds (currentSession.clips [currentSessionChoice].length);
        }
        if (currentSessionChoice < currentSession.clips.Length - 1) {
            StartCoroutine (Session_Play_Sequence ());
        } else {
            yield return new WaitForSeconds (1f);
            float alpha = 1f;
            while (alpha > 0f) {
                alpha -= 0.3f * Time.deltaTime;
                if (subtitleSpeakerText != null) {
                    subtitleSpeakerText.color = new Color (subtitleSpeakerText.color.r, subtitleSpeakerText.color.g, subtitleSpeakerText.color.b, alpha);
                }
                if (subtitlesText != null) {
                    subtitlesText.color = new Color (subtitlesText.color.r, subtitlesText.color.g, subtitlesText.color.b, alpha);
                }
                yield return new WaitForEndOfFrame ();
            }
            dialogueSessionInProgress = false;
        }
        yield return null;
    }

}
