using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoadAttribute]
public class DevTimeTracker : MonoBehaviour
{
    // Keeps track of time spent in editor & play mode
    // Saves to file after each session

    public enum LogLevel {None, Full};

    [Header ("Parameters")]
    public LogLevel loglevel;

    [Header ("Dynamics")]
    public float time;
    public DateTime lastTimeStarted;
    public DateTime thisTimeStarted;

    void Awake () {
        thisTimeStarted = DateTime.Now;
        Load_DevTime ();
        if (loglevel == LogLevel.Full)
            Debug.Log ("thisTimeStarted: " + thisTimeStarted + $" | Time loaded to: {time}");
    }

    void Update () {
        time += Time.deltaTime;
    }

    void OnApplicationQuit () {
        Save_DevTime ();
    }

    #region wip
    // Ref: https://docs.unity3d.com/ScriptReference/EditorApplication-playModeStateChanged.html
    // ------
    // 2-Ref: https://docs.unity3d.com/ScriptReference/EditorApplication-playModeStateChanged.html
    // 2-Ref: https://docs.unity3d.com/ScriptReference/EditorApplication-wantsToQuit.html
    // 3-Ref: https://docs.unity3d.com/ScriptReference/EditorApplication.html

    // register an event handler when the class is initialized
    static DevTimeTracker()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        Debug.Log("State changed to: " + state);
        if (state == PlayModeStateChange.ExitingPlayMode) {
            
        }
    }
    #endregion wip

    void Load_DevTime () {
        time = PlayerPrefs.GetFloat ("DevTime", 0f);
    }

    void Save_DevTime () {
        PlayerPrefs.SetFloat ("DevTime", time);
        if (loglevel == LogLevel.Full)
            Debug.Log ("DevTime saved to: " + PlayerPrefs.GetFloat ("DevTime", 0f));
    }

}
