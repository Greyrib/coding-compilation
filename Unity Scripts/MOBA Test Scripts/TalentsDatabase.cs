using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentsDatabase : MonoBehaviour
{
    public static TalentsDatabase instance;

    

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        Load_All_Talents ();
    }

    void Load_All_Talents () {

    }

}
