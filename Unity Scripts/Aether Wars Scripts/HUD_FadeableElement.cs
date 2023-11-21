using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HUD_FadeableElement : MonoBehaviour
{
    [Header ("Parameters")]
    public bool updateRaycastStatus = true;
    public bool updateButtonInteractability = true;

    [Header ("Dynamics")]
    public bool targetFactor;
    public float factor = 1f;
    [Space (5)]
    public TextMeshProUGUI text;
    public float textOpacity;
    public Image image;
    public float imageOpacity;
    public Button button;

    void Start () {
        text = GetComponent<TextMeshProUGUI>();
        if (text != null) {
            textOpacity = text.color.a;
        } else {
            textOpacity = -1f;
        }
        image = GetComponent<Image>();
        if (image != null) {
            imageOpacity = image.color.a;
        } else {
            imageOpacity = -1f;
        }
        button = GetComponent<Button>();
        //StartCoroutine (Lerp_Tick ());
    }

    IEnumerator Lerp_Tick () {

        yield return new WaitForSeconds (0.1f);
        StartCoroutine (Lerp_Tick ());
        yield return null;
    }

    void Update () {
        Lerp ();
    }

    void Lerp () {
        if (targetFactor == true) {
            factor = Mathf.Lerp (factor, 1f, 1f * Time.deltaTime);
            if (Mathf.Abs (1f - factor) < 0.2f) {
                factor = 1f;
            }
        } else if (targetFactor == false) {
            factor = Mathf.Lerp (factor, 0f, 2f * Time.deltaTime);
            if (Mathf.Abs (0f - factor) < 0.1f) {
                factor = 0f;
            }
        }

        if (text != null) {
            text.color = new Color (text.color.r, text.color.g, text.color.b, textOpacity * factor);
        }

        if (updateRaycastStatus == true && text != null && factor == 1f) {
            text.raycastTarget = true;
        } else if (updateRaycastStatus == true && text != null && factor < 1f) {
            text.raycastTarget = false;
        }

        if (image != null) {
            image.color = new Color (image.color.r, image.color.g, image.color.b, imageOpacity * factor);
        }

        if (updateRaycastStatus == true && image != null && factor == 1f) {
            image.raycastTarget = true;
        } else if (updateRaycastStatus == true && image != null && factor < 1f) {
            image.raycastTarget = false;
        }

        if (updateButtonInteractability == true && button != null && factor == 1f) {
            button.interactable = true;
        } else if (updateButtonInteractability == true && button != null && factor < 1f) {
            button.interactable = false;
        }
    }

}
