using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class HUD_AbilityButton : MonoBehaviour
{
    // Updates UI button, based on linked Ability-derived-class (e.g. " ShooterHero's_RaynorCopycatShot ")

    [Header ("References")]
    public Image abilityImage;
    public TextMeshProUGUI cooldowntimer;
    public Image cooldownOverlayFilledImage;

    [Header ("Dynamics")]
    public Ability linkedAbility;

    // NOTE Grey out abilityImage if not ready

    public void Assign_Ability (Ability newAbility) {
        linkedAbility = newAbility;
        if (abilityImage != null && newAbility != null && newAbility.abilitySprite != null) {
            abilityImage.sprite = newAbility.abilitySprite;
        } else if (abilityImage != null) {
            abilityImage.sprite = null;
        }
    }

    void LateUpdate () {
        if (linkedAbility != null) {
            if (linkedAbility.cooldownTime > 0f) {
                // Ability not ready
                if (cooldowntimer != null) {
                    cooldowntimer.text = linkedAbility.cooldownTime.ToString ("F1");
                }
                if (cooldownOverlayFilledImage != null && linkedAbility.cooldown > 0f) {
                    cooldownOverlayFilledImage.fillAmount = linkedAbility.cooldownTime / linkedAbility.cooldown;
                }
            } else if (linkedAbility.cooldownTime <= 0f) {
                // Ability ready
                if (cooldowntimer != null) {
                    cooldowntimer.text = "";
                }
                if (cooldownOverlayFilledImage != null && cooldownOverlayFilledImage.fillAmount != 0f) {
                    cooldownOverlayFilledImage.fillAmount = 0f;
                }
            }
        }
    }

}
