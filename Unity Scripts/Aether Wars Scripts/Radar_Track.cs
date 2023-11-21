using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Radar_Track : MonoBehaviour
{
    [Header ("References")]
    public Image corners;
    public Image diamond;
    public TextMeshProUGUI typeTxt;
    public Image backgroundbar;
    public Image healthbar;
    public Image shieldbar;

    [Header ("Parameters")]
    public bool showClampedCorners;
    public bool showClampedDiamond;
    public bool showClampedText;
    public bool showClampedBackground;
    public bool showClampedHealthbar;
    public bool showClampedShieldbar;
    [Space (5)]
    public bool showUnclampedCorners;
    public bool showUnclampedDiamond;
    public bool showUnclampedText;
    public bool showUnclampedBackground;
    public bool showUnclampedHealthbar;
    public bool showUnclampedShieldbar;

    [Header ("Dynamics")]
    public IFF target;

    public void Assign_Target (IFF newTarget) {
        target = newTarget;

        gameObject.name = "RadarTrack__" + target.gameObject.name;

        Update_HUD_Position_To_Target ();

        // Colorize according to target's faction
        if (newTarget != null && newTarget.faction == IFF.Faction.Alliance && RadarHandler.instance && newTarget.transform.root.tag != "Boss") {
            Colorize_Elements (RadarHandler.instance.friendlyColor);
        } else if (newTarget != null && newTarget.faction == IFF.Faction.Federation && RadarHandler.instance && newTarget.transform.root.tag != "Boss") {
            Colorize_Elements (RadarHandler.instance.hostileColor);
        } else if (newTarget != null && (newTarget.faction == IFF.Faction.Civilian || newTarget.faction == IFF.Faction.Galaxy) && newTarget.transform.root.tag != "Boss") {
            Colorize_Elements (Color.white);
        } else if (newTarget.transform.root.tag == "Boss") {
            Colorize_Elements (RadarHandler.instance.bossColor);
        } else {
            Colorize_Elements (RadarHandler.instance.hostileColor);
        }

        if (typeTxt && newTarget != null) {
            typeTxt.text = newTarget.unitType;
        }

        Update_HUD_Position_To_Target ();
    }

    public void Clear_Target () {
        target = null;
        gameObject.name = "RadarTrack__CLEARED";
        if (typeTxt) {
            typeTxt.text = "";
        }
    }

    void Colorize_Elements (Color newColor) {
        if (corners) {
            corners.color = newColor;
        }

        if (diamond) {
            diamond.color = newColor;
        }

        if (typeTxt) {
            typeTxt.color = newColor;
        }
    }

    void OnEnable () {
        // We get disabled when entering hangarmode, so reset-&-reboot our update_tick when we get re-enabled again
        StopAllCoroutines ();
        StartCoroutine (Update_Tick ());
    }

    IEnumerator Update_Tick () {
        if (target != null && target.gameObject.activeInHierarchy == false) {
            Clear_Target ();
        }
        if (target != null) {
            Update_HUD_Position_To_Target ();
        } else if (target == null) {
            target = null;

            if (corners.color.a > 0f || typeTxt.color.a > 0f) {
                Colorize_Elements (new Color (corners.color.r, corners.color.g, corners.color.b, 0f));
            }
            if (diamond.color.a > 0f) {
                diamond.color = new Color (diamond.color.r, diamond.color.g, diamond.color.b, 0f);
            }
        }
        Update_Bars ();

        yield return new WaitForSeconds (0.03f);
        StartCoroutine (Update_Tick ());
        yield return null;
    }

    void Update_HUD_Position_To_Target () {
        if (target != null) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint (target.transform.position);
            screenPos = new Vector3 (Mathf.Clamp (screenPos.x, 0f, Screen.width), Mathf.Clamp (screenPos.y, 0f, Screen.height), screenPos.z);
            if (screenPos.x == 0f || screenPos.x == Screen.width || screenPos.y == 0f || screenPos.y == Screen.height) {
                Update_Clamped_Status (true);
            } else {
                Update_Clamped_Status (false);
            }
            transform.position = screenPos;
        }
    }

    void Update_Clamped_Status (bool amIClamped) {
        if (amIClamped) {
            if (corners.color.a > 0f) {
                //Set_Alpha_Value (corners, 0f);
                Set_Alpha_Value (corners, Parsed_Clamp_Bool (showClampedCorners));
            }
            if (typeTxt && typeTxt.color.a > 0f) {
                //Set_Alpha_Value (typeTxt, 0f);
                Set_Alpha_Value (typeTxt, Parsed_Clamp_Bool (showClampedText));
            }
            if (backgroundbar && backgroundbar.color.a > 0f) {
                //Set_Alpha_Value (backgroundbar, 0f);
                Set_Alpha_Value (backgroundbar, Parsed_Clamp_Bool (showClampedBackground) * 0.5f);
            }
            if (healthbar && healthbar.color.a > 0f) {
                //Set_Alpha_Value (healthbar, 0f);
                Set_Alpha_Value (healthbar, Parsed_Clamp_Bool (showClampedHealthbar));
            }
            if (shieldbar && shieldbar.color.a > 0f) {
                //Set_Alpha_Value (shieldbar, 0f);
                Set_Alpha_Value (shieldbar, Parsed_Clamp_Bool (showClampedShieldbar));
            }
            if (diamond.color.a < 1f) {
                //Set_Alpha_Value (diamond, 1f);
                Set_Alpha_Value (diamond, Parsed_Clamp_Bool (showClampedDiamond));
            }
        } else if (!amIClamped) {
            if (corners.color.a < 1f) {
                //Set_Alpha_Value (corners, 1f);
                Set_Alpha_Value (corners, Parsed_Clamp_Bool (showUnclampedCorners));
            }
            if (typeTxt && typeTxt.color.a < 1f) {
                //Set_Alpha_Value (typeTxt, 1f);
                Set_Alpha_Value (typeTxt, Parsed_Clamp_Bool (showUnclampedText));
            }
            if (backgroundbar && backgroundbar.color.a < 0.5f) {
                //Set_Alpha_Value (backgroundbar, 0.5f);
                Set_Alpha_Value (backgroundbar, Parsed_Clamp_Bool (showUnclampedBackground) * 0.5f);
            }
            if (healthbar && healthbar.color.a < 1f) {
                //Set_Alpha_Value (healthbar, 1f);
                Set_Alpha_Value (healthbar, Parsed_Clamp_Bool (showUnclampedHealthbar));
            }
            if (shieldbar && shieldbar.color.a < 1f) {
                //Set_Alpha_Value (shieldbar, 1f);
                Set_Alpha_Value (shieldbar, Parsed_Clamp_Bool (showUnclampedShieldbar));
            }
            if (diamond.color.a > 0f) {
                //Set_Alpha_Value (diamond, 0f);
                Set_Alpha_Value (diamond, Parsed_Clamp_Bool (showUnclampedDiamond));
            }
        }
    }

    float Parsed_Clamp_Bool (bool inputBool) {
        if (inputBool) {
            return 1f;
        }

        return 0f;
    }

    void Update_Bars () {
        if (target != null) {
            if (backgroundbar) {
                //Set_Alpha_Value (backgroundbar, 0.5f);
            }

            if (healthbar) {
                if (healthbar.color.a < 1f) {
                    //Set_Alpha_Value (healthbar, 1f);
                }
                
                if (target.HP_Ref () != null && target.HP_Ref ().healthMax > 0f) {
                    healthbar.fillAmount = target.HP_Ref ().health / target.HP_Ref ().healthMax;
                }
            }

            if (shieldbar) {
                if (healthbar.color.a < 1f) {
                    //Set_Alpha_Value (shieldbar, 1f);
                }

                if (target.HP_Ref () != null && target.HP_Ref ().shield != null && target.HP_Ref ().shield.shieldMax > 0f) {
                    shieldbar.fillAmount = target.HP_Ref ().shield.shieldValue / target.HP_Ref ().shield.shieldMax;
                } else if ((target.HP_Ref () != null && target.HP_Ref ().shield == null) || (target.HP_Ref () == null)) {
                    shieldbar.fillAmount = 0f;
                }
            }
        } else if (target == null) {
            if (backgroundbar && backgroundbar.color.a > 0f) {
                Set_Alpha_Value (backgroundbar, 0f);
            }

            if (healthbar && healthbar.color.a > 0f) {
                Set_Alpha_Value (healthbar, 0f);
            }

            if (shieldbar && shieldbar.color.a > 0f) {
                Set_Alpha_Value (shieldbar, 0f);
            }
        }
    }

    void Set_Alpha_Value (Image img, float newAlpha) {
        img.color = new Color (img.color.r, img.color.g, img.color.b, newAlpha);
    }

    void Set_Alpha_Value (TextMeshProUGUI txt, float newAlpha) {
        txt.color = new Color (txt.color.r, txt.color.g, txt.color.b, newAlpha);
    }

}
