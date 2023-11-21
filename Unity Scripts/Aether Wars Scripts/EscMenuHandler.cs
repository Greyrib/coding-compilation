using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor;
//using UnityEditor.SceneManagement;

public class EscMenuHandler : MonoBehaviour
{
    public static EscMenuHandler instance;

    [Header ("References")]
    public GameObject escMenuObject;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        if (escMenuObject && escMenuObject.activeInHierarchy) {
            escMenuObject.SetActive (false);
        }
    }

    void Update () {
        if (Input.GetKeyDown (KeyCode.Escape) && ((OptionsHandler.instance && !OptionsHandler.instance.optionsObject.activeInHierarchy) || (OptionsHandler.instance == null))) {
            Toggle_Exit_Menu ();
        }
    }

    public void Toggle_Exit_Menu () {
        if (escMenuObject && !escMenuObject.activeInHierarchy) {
            Time.timeScale = 0f;
            escMenuObject.SetActive (true);
            if (CrosshairControl.instance) {
                CrosshairControl.instance.Set_Crosshair_Mode (1);
            }
        } else if (escMenuObject && escMenuObject.activeInHierarchy) {
            escMenuObject.SetActive (false);
            Time.timeScale = 1f;
            if (CrosshairControl.instance) {
                CrosshairControl.instance.Set_Crosshair_Mode (0);
            }
        }
    }

    public void Exit_From_The_Game () {
        PlayerProfile.instance.Save_Player_Data ();
        
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode ();
        #endif
    }

}
