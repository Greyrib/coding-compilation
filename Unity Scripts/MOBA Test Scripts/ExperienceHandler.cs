using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ExperienceHandler : MonoBehaviour
{
    public static ExperienceHandler instance;

    [Header ("References")]
    public Image blueExperienceBar;
    public Image redExperienceBar;
    public TextMeshProUGUI factionLevelTextBlue;
    public TextMeshProUGUI factionLevelTextRed;

    [Header ("Dynamics")]
    public List<int> experiencePoints;
    public List<int> factionLevels;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
    }

    void Start () {
        Prematch_Setup ();
    }

    void Prematch_Setup () {
        experiencePoints = new List<int> () {0, 0};
        factionLevels = new List<int> () {1, 1};
        oppositeFactionStructuresDestroyed = new List<int>() {0, 0};
        Update_HUD_Experiencebars ();
        Check_For_Levelups ();
    }

    void Update () {
        // if (Input.GetKeyDown (KeyCode.Alpha1)) {
        //     Grant_Experience (250, IFF.Faction.Blue);
        // }
        // if (Input.GetKeyDown (KeyCode.Alpha2)) {
        //     Grant_Experience (250, IFF.Faction.Red);
        // }
    }

    public void Grant_Experience (int experienceAmount, IFF.Faction experienceFaction) {
        if (experienceFaction == IFF.Faction.Blue) {
            experiencePoints [0] += experienceAmount;
        } else if (experienceFaction == IFF.Faction.Red) {
            experiencePoints [1] += experienceAmount;
        }

        // TODO Hero-XP-calc & Hots 'Underdog' kill calc-mechanic (see wiki 'Experience' for details)

        Check_For_Levelups ();
        Update_HUD_Experiencebars ();
    }

    void Check_For_Levelups () {
        // Cache the current levels of the factions
        List<int> prelevels = factionLevels;
        //Debug.Log ("Check_For_Levelups - Pre levels: " + prelevels[0] + " & " + prelevels[1]);

        // Loop through both factions experiencepoints, and if they aren't the appropriate level, levelify them after concluding which level they're at
        // NOTE They start at level 1, at beginning of the match
        List<bool> levelIncremented = new List<bool> () {false, false};
        for (int l = 0; l < Level_Exp_Requirements().Count; l++) {
            //Debug.Log ("Checking lvling ;; " + experiencePoints[0] + " VS " + Level_Exp_Requirements () [l] + " | " + factionLevels[0] + " VS " + l);
            //Debug.Log ((experiencePoints[0] >= Level_Exp_Requirements() [l]) + " || " + (factionLevels[0] <= l));
            if (experiencePoints[0] >= Level_Exp_Requirements() [l] && factionLevels[0] <= l) {
                //Debug.Log ("We found a level we've surpassed; blue is leveling up! ||| " + l);
                levelIncremented [0] = true;
            }
            if (experiencePoints[1] >= Level_Exp_Requirements() [l] && factionLevels[1] <= l) {
                //Debug.Log ("We found a level we've surpassed; blue is leveling up! ||| " + l);
                levelIncremented [1] = true;
            }
        }

        //Debug.Log (levelIncremented[0] + " & " + levelIncremented[1]);

        if (levelIncremented [0] == true) {
            // VFX for all BLUE faction heroes
            Debug.Log ("BLUE leveled up! " + Time.time.ToString ("F2"));
            factionLevels[0] += 1;
        }

        if (levelIncremented [1] == true) {
            // VFX for all RED faction heroes
            Debug.Log ("RED leveled up! " + Time.time.ToString ("F2"));
            factionLevels[1] += 1;
        }

        // If able, update factionLevelTexts to respective faction levels
        if (factionLevelTextBlue != null && factionLevels != null && factionLevels.Count >= 1) {
            factionLevelTextBlue.text = factionLevels[0].ToString();
        }
        if (factionLevelTextRed != null && factionLevels != null && factionLevels.Count >= 2) {
            factionLevelTextRed.text = factionLevels[1].ToString();
        }
    }

    #region static_level_array_stuff
    List<int> Level_Exp_Requirements () {
        // Numbers taken from HOTS wiki, lvl 1-30
        // Reference: https://heroesofthestorm.fandom.com/wiki/Experience
        List<int> levelExperienceAmounts = new List<int> {
            0,
            2010,
            4164,
            6318,
            8472,
            10626,
            13929,
            17232,
            20535,
            23838,
            27141,
            31593,
            36045,
            40497,
            44949,
            49401,
            55001,
            60601,
            66201,
            71801,
            80801,
            90801,
            102301,
            115301,
            130301,
            147301,
            166801,
            188801,
            213801,
            241801
        };

        return levelExperienceAmounts;
    }

    int XP_REQ_ForLVL (int askingLevel) {
        // Numbers taken from HOTS wiki, lvl 1-30
        // Reference: https://heroesofthestorm.fandom.com/wiki/Experience
        List<int> levelExperienceAmounts = new List<int> {
            0,
            2010,
            4164,
            6318,
            8472,
            10626,
            13929,
            17232,
            20535,
            23838,
            27141,
            31593,
            36045,
            40497,
            44949,
            49401,
            55001,
            60601,
            66201,
            71801,
            80801,
            90801,
            102301,
            115301,
            130301,
            147301,
            166801,
            188801,
            213801,
            241801
        };
        if (askingLevel <= levelExperienceAmounts.Count) {
            return levelExperienceAmounts [askingLevel];
        }

        return 999999;
    }
    #endregion static_level_array_stuff

    #region hud_experiencebars
    void Update_HUD_Experiencebars () {
        if (blueExperienceBar != null) {
            int curLevelXP_Blue = experiencePoints[0] - XP_REQ_ForLVL (factionLevels[0] - 1);
            int nextLevelXP_Blue = XP_REQ_ForLVL (factionLevels[0]) - XP_REQ_ForLVL (factionLevels[0] - 1);
            //Debug.Log (curLevelXP_Blue + " / " + nextLevelXP_Blue + " || " + (float) ((float) curLevelXP_Blue / (float)nextLevelXP_Blue));
            blueExperienceBar.fillAmount = (float) ((float) curLevelXP_Blue / (float)nextLevelXP_Blue);
        }
        if (redExperienceBar != null) {
            int curLevelXP_Red = experiencePoints[1] - XP_REQ_ForLVL (factionLevels[1] - 1);
            int nextLevelXP_Red = XP_REQ_ForLVL (factionLevels[1]) - XP_REQ_ForLVL (factionLevels[1] - 1);
            //Debug.Log (curLevelXP_Red + " / " + nextLevelXP_Red + " || " + (float) ((float) curLevelXP_Red / (float)nextLevelXP_Red));
            redExperienceBar.fillAmount = (float) ((float) curLevelXP_Red / (float)nextLevelXP_Red);
        }
    }
    #endregion hud_experiencebars

    #region passive_xp
    // NOTE Destroying a fort or keep in HOTS grants +4.6 xp/s increase (stacking from multiple structure kills) bonus
    [Header ("Passive XP")]
    public List<int> oppositeFactionStructuresDestroyed;
    public void Commence_Passive_Experience () {
        StartCoroutine (Passive_Experience ());
    }

    IEnumerator Passive_Experience () {
        Grant_Experience (Passive_XP_Current (IFF.Faction.Blue), IFF.Faction.Blue);
        Grant_Experience (Passive_XP_Current (IFF.Faction.Red), IFF.Faction.Red);

        yield return new WaitForSeconds (1f);

        if (MatchHandler.instance != null && MatchHandler.instance.matchStatus == MatchHandler.MatchStatus.Active) {
            StartCoroutine (Passive_Experience ());
        }

        yield return null;
    }

    int Passive_XP_Current (IFF.Faction passiveFaction) {
        int xp = 23;
        if (passiveFaction == IFF.Faction.Blue) {
            xp += (oppositeFactionStructuresDestroyed[0] * 5);
        } else if (passiveFaction == IFF.Faction.Red) {
            xp += (oppositeFactionStructuresDestroyed[1] * 5);
        }
        return xp;
    }
    #endregion passive_xp

    #region scaling
    // These scale off their level & base stats from Hero_Stats class
    public void Scale_Hero (IFF hero) {

    }

    // These scale off
    public void Scale_Minion (IFF minion) {

    }
    #endregion scaling

}
