using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile instance;

    [Header ("Pre-Match Setup")]
    [Range (0, 4)] public int allyCount = 1;
    [Range (1, 5)] public int enemyCount = 1;
    public string heroChosen;
    public List<string> abilitiesChosen; // Which abilities we chose for this match ; TODO Choose in menu pre-match | for now, debug-purposes dictate just writing/copypasting abilitynames as testing occurs

    [Header ("Match Configuratorables")]
    public float towerProjectileDamage = 290f; // Average of HOTS outer tower 250 & HOTS inner tower 330 | Reference: https://heroesofthestorm.fandom.com/wiki/Tower

    [Header ("Active Match")]
    public Unit_Control playersUnit;

    void Awake () {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad (this);
        } else {
            Destroy (this);
        }
    }

}
