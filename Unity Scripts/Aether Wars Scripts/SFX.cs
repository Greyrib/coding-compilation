using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SFX : MonoBehaviour
{
    public static SFX instance;

    [Header ("Parameters")]
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup dialogueGroup;
    public AudioMixerGroup ambienceGroup;

    [Header ("Generic Sounds")]
    public AudioClip errorSound;
    public AudioClip[] asteroidShipCollisionSounds;
    public AudioClip switchShipLights;
    public AudioClip[] jettisonSounds;
    public AudioClip itemEquipSound;
    public AudioClip[] noAmmoSounds;
    public AudioClip countermeasureLaserDefend;
    
    public AudioClip nanokitUsed;
    public AudioClip shieldbatUsed;
    [Space (5)]
    public AudioClip[] robotEmoteSounds;
    public AudioClip hardpointSelectSound;
    public AudioClip itemPickupPlayerSound;

    [Header ("HUD Sounds")]
    public AudioClip uiOpen;
    public AudioClip uiClose;
    public AudioClip uiError;
    public AudioClip uiToggleSound;
    public AudioClip purchaseItemSound;
    public AudioClip sellItemSound;
    
    AudioSource[] asources;
    int asourceCurrent;
    List<AudioClip> blacklisteds;

    AudioSource[] asourcesUi;
    int asourceCurrentUi;

    AudioSource[] asourcesSpeech;
    int asourceCurrentSpeech;

    // NOTE TO SELF - Expose parameters by right-clicking their name in inspector, on given audio mixer selected (e.g. 'Volume', rightclick on name to expose parameter [Additionally, rename parameter in 'Exposed Parameters' in Audio Mixer panel])

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        Initial_Setup ();

        if (masterGroup != null) {
            float masterVol;
            masterGroup.audioMixer.GetFloat ("Master Volume", out masterVol);
            //Debug.Log ("Master Volume: " + masterVol);
            if (masterVol < 0f) {
                Debug.Log ("ATTENTION - Master Volume is altered, you may want to reset this at some point ?");
            }
        }
    }

    void Initial_Setup () {
        GameObject parent = new GameObject ("AudioSources_");
        parent.transform.SetParent (transform);

        int amount = 25;
        asources = new AudioSource[amount];
        for (int a = 0; a < amount; a++) {
            GameObject newSource = new GameObject ("AudioSource_" + a) as GameObject;
            newSource.transform.SetParent (parent.transform);
            newSource.AddComponent<AudioSource>();
            asources[a] = newSource.GetComponent<AudioSource>();
            asources[a].outputAudioMixerGroup = sfxGroup;
            
            //asources[a].rolloffMode = AudioRolloffMode.Linear;
            //asources[a].maxDistance = 250f;
            //asources[a].spatialBlend = 1f;
            asources[a].dopplerLevel = 0f;
        }

        asourceCurrent = 0;

        blacklisteds = new List<AudioClip>();

        int uiAmount = 10;
        asourcesUi = new AudioSource[uiAmount];
        for (int u = 0; u < uiAmount; u++) {
            GameObject newSourceUi = new GameObject ("AudioSource_UI_" + u) as GameObject;
            newSourceUi.transform.SetParent (parent.transform);
            newSourceUi.AddComponent<AudioSource>();
            asourcesUi[u] = newSourceUi.GetComponent<AudioSource>();
            asourcesUi[u].outputAudioMixerGroup = sfxGroup;
            asourcesUi[u].volume = 0.3f;
            asourcesUi[u].rolloffMode = AudioRolloffMode.Linear;
            asourcesUi[u].maxDistance = 250f;
            asourcesUi[u].spatialBlend = 1f;
            asourcesUi[u].dopplerLevel = 0f;
        }

        asourceCurrentUi = 0;

        int speechAmount = 10;
        asourcesSpeech = new AudioSource[speechAmount];
        for (int s = 0; s < speechAmount; s++) {
            GameObject newSourceSpeech = new GameObject ("AudioSource_Speech_" + s) as GameObject;
            newSourceSpeech.transform.SetParent (parent.transform);
            asourcesSpeech[s] = newSourceSpeech.AddComponent<AudioSource>();
            asourcesSpeech[s].outputAudioMixerGroup = dialogueGroup;

            asourcesSpeech[s].rolloffMode = AudioRolloffMode.Linear;
            asourcesSpeech[s].maxDistance = 250f;
            asourcesSpeech[s].spatialBlend = 1;
            asourcesSpeech[s].dopplerLevel = 0f;
        }

        asourceCurrentSpeech = 0;
    }

    #region play_sounds
    public void Play_SFX (AudioClip clip, Vector3 worldPos) {
        if (!blacklisteds.Contains (clip)) {
            asources[asourceCurrent].transform.position = worldPos;
            asources[asourceCurrent].PlayOneShot (clip);
            StartCoroutine (Blacklist_Clip (clip));
            Increment_Asource_Current ();
        }
    }

    public void Play_SFX_UI (AudioClip clip) {
        asourcesUi[asourceCurrentUi].transform.position = CameraControl.instance.transform.position;
        asourcesUi[asourceCurrentUi].volume = 1f;
        asourcesUi[asourceCurrentUi].PlayOneShot (clip);
        Increment_Asource_Current_UI ();
    }

    public void Play_Speech (AudioClip speechClip, Vector3 worldPos) {
        asourcesSpeech[asourceCurrentSpeech].transform.position = worldPos;
        asourcesSpeech[asourceCurrentSpeech].PlayOneShot (speechClip);
        Increment_Asource_Current_Speech ();
    }
    
    public void Play_SFX_AtVolume (AudioClip clip, float atVolume) {
        if (!blacklisteds.Contains (clip)) {
            StartCoroutine (Blacklist_Clip (clip));
            StartCoroutine (Play_Clip_At_Volume (clip, atVolume));
        }
    }

    IEnumerator Play_Clip_At_Volume (AudioClip clip, float atVolume) {
        int asourceUsed = asourceCurrent;
        Increment_Asource_Current ();
        asources[asourceUsed].volume = atVolume;
        asources[asourceUsed].PlayOneShot (clip);
        yield return new WaitForSeconds (clip.length);
        asources[asourceUsed].volume = 1f;
        yield return null;
    }

    public void Play_SFX_PitchVolRandomnized (AudioClip clip, Vector3 worldPos) {
        if (!blacklisteds.Contains (clip)) {
            float cachedVol = asources[asourceCurrent].volume;
            float cachedPitch = asources[asourceCurrent].pitch;

            asources[asourceCurrent].transform.position = worldPos;

            asources[asourceCurrent].pitch = Random.Range (0.95f, 1.05f);
            asources[asourceCurrent].volume = Random.Range (0.9f, 1f);
            asources[asourceCurrent].PlayOneShot (clip);

            StartCoroutine (Blacklist_Clip (clip));

            StartCoroutine (PitchVolReset (asourceCurrent, cachedVol, cachedPitch, clip.length));
        }
    }

    IEnumerator PitchVolReset (int asourceToReset, float resetVolume, float resetPitch, float clipDelayLength) {
        yield return new WaitForSeconds (clipDelayLength);
        asources[asourceCurrent].pitch = resetPitch;
        asources[asourceCurrent].volume = resetVolume;
        yield return null;
    }

    void Increment_Asource_Current () {
        asourceCurrent += 1;
        if (asourceCurrent > asources.Length - 1) {
            asourceCurrent = 0;
        }
    }

    void Increment_Asource_Current_UI () {
        asourceCurrentUi += 1;
        if (asourceCurrentUi > asourcesUi.Length - 1) {
            asourceCurrentUi = 0;
        }
    }

    void Increment_Asource_Current_Speech () {
        asourceCurrentSpeech += 1;
        if (asourceCurrentSpeech > asourcesSpeech.Length - 1) {
            asourceCurrentSpeech = 0;
        }
    }

    IEnumerator Blacklist_Clip (AudioClip blacklistClip) {
        if (!blacklisteds.Contains (blacklistClip)) {
            blacklisteds.Add (blacklistClip);
            yield return new WaitForSeconds (0.05f);
            if (blacklisteds.Contains (blacklistClip)) {
                blacklisteds.Remove (blacklistClip);
            }
        }
        yield return null;
    }

    public void Play_Robot_Random_Emote () {
        if (robotEmoteSounds.Length > 0) {
            Play_SFX (robotEmoteSounds [Random.Range (0, robotEmoteSounds.Length)], Camera.main.transform.position);
        }
    }
    #endregion play_sounds

}
