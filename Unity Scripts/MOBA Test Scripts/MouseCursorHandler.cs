using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MouseCursorHandler : MonoBehaviour
{
    public static MouseCursorHandler instance;

    // Class for handling the Mouse Cursor's behaviour

    [Header ("Behaviour Parameters")]
    public bool updateOnAwake = false;
    public bool updateOnStart = true;
    [Space (5)]
    public bool hideHardwareMouseInGame;

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy (this);
        }
        
        if (updateOnAwake == true) {
            Update_MouseCursor_Status ();
        }
    }

    void Start () {
        if (updateOnStart == true) {
            Update_MouseCursor_Status ();
        }
        EditorApplication.pauseStateChanged += LogPauseState;
    }
    
    // Called when we focus the game/app again, e.g. tab'ing/clicking back in
    void OnApplicationFocus (bool hasFocus) {
        Update_MouseCursor_Status ();
    }

    // Consolidate update elements here
    void Update_MouseCursor_Status (bool isEditorPaused = false) {
        if (Cursor.visible == true && hideHardwareMouseInGame == true && EditorApplication.isPaused == false) {
            Cursor.visible = false;
        } else if (Cursor.visible == false && hideHardwareMouseInGame == true && EditorApplication.isPaused == true) {
            Cursor.visible = true;
        }
    }

    private void LogPauseState(PauseState state)
    {
        Debug.Log(state);
        if (state == PauseState.Paused) {
            Update_MouseCursor_Status (true);
        } else if (state == PauseState.Unpaused) {
            Update_MouseCursor_Status (false);
        }
        
    }

}
