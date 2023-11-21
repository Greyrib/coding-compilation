using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AudioSpectrumConverter : MonoBehaviour
{
    [Header ("References")]
    public Transform barParent;
    public Image[] dialogueEqualizerBarImages;
    public Image[] dialogueEqualizerBarImagesInverted;

    [Header ("Parameters")]
    public float frequencyInterval = 0.15f;
    public Gradient frequencyGradient;

    [Header ("Dynamics")]
    public List<GameObject> children;
    public AnimationCurve spectrum;

    void Start () {
        //Invoke ("Acquire_Child_Sources", 3f);
        Acquire_Child_Sources ();
    }

    void Acquire_Child_Sources () {
        children = new List<GameObject>();
        foreach (Transform child in barParent) children.Add(child.gameObject);

        Hide_MeshRenderers_For_Children ();
    }

    void Hide_MeshRenderers_For_Children () {
        for (int c = 0; c < children.Count; c++) {
            MeshRenderer childmesh = children[c].transform.GetChild (0).GetComponent<MeshRenderer>();
            if (childmesh != null) {
                childmesh.enabled = false;
            }
        }

        StartCoroutine (Spectrum_Update_Tick ());
    }

    IEnumerator Spectrum_Update_Tick () {
        Update_Spectrum ();
        yield return new WaitForSeconds (frequencyInterval);
        StartCoroutine (Spectrum_Update_Tick ());
        yield return null;
    }

    void Update_Spectrum () {
        Keyframe[] keys = new Keyframe [children.Count];
        for (int c = 0; c < children.Count; c++) {
            keys [c] = new Keyframe ((1f / children.Count) * c, children[c].transform.localScale.y);
            //Debug.Log (children[c].transform.localScale.y);
        }

        spectrum = new AnimationCurve (keys);

        // Update DialogueBarImages if any
        if (dialogueEqualizerBarImages.Length > 0) {
            for (int i = 0; i < dialogueEqualizerBarImages.Length; i++) {
                dialogueEqualizerBarImages[i].fillAmount = spectrum.Evaluate ((float)i / dialogueEqualizerBarImages.Length) / 15f;
                dialogueEqualizerBarImages[i].color = frequencyGradient.Evaluate (dialogueEqualizerBarImages[i].fillAmount);
            }
        }

        if (dialogueEqualizerBarImagesInverted.Length > 0) {
            for (int i = 0; i < dialogueEqualizerBarImagesInverted.Length; i++) {
                dialogueEqualizerBarImagesInverted[i].fillAmount = spectrum.Evaluate ((float)i / dialogueEqualizerBarImagesInverted.Length) / 15f;
                dialogueEqualizerBarImagesInverted[i].color = frequencyGradient.Evaluate (dialogueEqualizerBarImagesInverted[i].fillAmount);
            }
        }
    }

}
