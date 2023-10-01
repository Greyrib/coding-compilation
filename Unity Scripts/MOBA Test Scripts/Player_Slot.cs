using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Slot : MonoBehaviour
{
    // A 'Player' in a match ; handles respawn of its chosen hero with its chosen kit

    public string playerName = "Generic Player";
    public IFF.Faction playerFaction;
    public bool isComputer;

    [Header ("Dynamics")]
    public string heroChoice;
    public List<string> abilitiesChosen;
    public GameObject playerUnit;

    #region player_setup
    public void Setup_PlayerSlot_HeroChoiceAndAbilities () {
        // If we're player, grab from PlayerProfile
        if (isComputer == false && PlayerProfile.instance != null) {
            heroChoice = PlayerProfile.instance.heroChosen;
            abilitiesChosen = PlayerProfile.instance.abilitiesChosen;
        } else if (isComputer == true && SkillsDatabase.instance != null) {
            // Else, generate randoms (nón-duplicated between AI playerslots)
            if (HeroDatabase.instance.heroPrefabs != null && HeroDatabase.instance.heroPrefabs.Count > 0) {
                // NOTE for now, duplication-issue not considered (still need to make enough heroes for variety)
                heroChoice = HeroDatabase.instance.heroPrefabs[Random.Range (0, HeroDatabase.instance.heroPrefabs.Count)].HeroName;
            }
            // TODO Request a set of randomnized abilities (DIFFERENT FROM ONES ALREADY CHOSEN BY OTHER AI PLAYERS)

        }
    }
    #endregion player_setup

    #region respawning
    public void Commence_Respawning (float respawnTime) {
        StartCoroutine (Respawn_Timer (respawnTime));
    }

    IEnumerator Respawn_Timer (float respawnTime) {
        float respawnTimeLeft = respawnTime;

        while (respawnTimeLeft > 0f) {
            // TODO Update player's HUD icon on top of match to respawnTimeLeft

            respawnTimeLeft -= 0.1f;
            yield return new WaitForSecondsRealtime (0.1f);
        }

        Respawn_Player_Unit ();

        yield return null;
    }

    // Public to also be called by MatchHandler, when setting up players first time around before match official start
    public void Respawn_Player_Unit () {
        // TODO Spawn player hero with chosen skills & talents
        if (MatchHandler.instance.baseHeroPrefab != null && PlayerProfile.instance != null) {
            int ourPosInPlayerArray = PlayersHandler.instance.Get_PlayerArrayIndex (this);
            Vector3 spawnPos = ReferenceHandler.instance.RandomCircle (Vector3.zero, 6f);
            Quaternion spawnRot = Quaternion.identity;
            Debug.Log (ourPosInPlayerArray + " for " + playerName + " | " + MatchHandler.instance.playerSpawnPointsBlue.Count + " / " + MatchHandler.instance.playerSpawnPointsRed.Count);
            if (ourPosInPlayerArray != -1 && playerFaction == IFF.Faction.Blue &&  MatchHandler.instance != null && MatchHandler.instance.playerSpawnPointsBlue != null && MatchHandler.instance.playerSpawnPointsBlue.Count > ourPosInPlayerArray) {
                spawnPos = MatchHandler.instance.playerSpawnPointsBlue[ourPosInPlayerArray].position;
                spawnRot = Quaternion.Euler (0f, 90f, 0f);
            } else if (ourPosInPlayerArray != -1 && playerFaction == IFF.Faction.Red &&  MatchHandler.instance != null && MatchHandler.instance.playerSpawnPointsRed != null && MatchHandler.instance.playerSpawnPointsRed.Count > ourPosInPlayerArray) {
                spawnPos = MatchHandler.instance.playerSpawnPointsRed[ourPosInPlayerArray].position;
                spawnRot = Quaternion.Euler (0f, -90f, 0f);
            }

            // Visual spawn effect
            if (Objectpooler.instance != null && Objectpooler.instance.warpinEffect != null) {
                GameObject warpFX = Instantiate (Objectpooler.instance.warpinEffect, spawnPos, spawnRot) as GameObject;
                warpFX.name = Objectpooler.instance.warpinEffect + "_instantiated_for_" + playerName;
            }

            GameObject newPlayer = Instantiate (MatchHandler.instance.baseHeroPrefab, spawnPos, spawnRot) as GameObject;
            newPlayer.name = MatchHandler.instance.baseHeroPrefab.name + playerName;

            // If we're the player's slot, set PlayerProfile's playersUnit to us [so crosshair cursor commands can order us around]
            if (isComputer == false && PlayerProfile.instance != null) {
                PlayerProfile.instance.playersUnit = newPlayer.GetComponent<Unit_Control>();
            }

            IFF niff = newPlayer.GetComponent<IFF>();
            if (niff != null) {
                niff.unitOwner = this;
                niff.faction = playerFaction;
            }

            // In both player & non-player case, create appropriate hero chosen vehicule
            if (heroChoice != "") {
                
            }

            // We are a PLAYER, so do PLAYER-related stuffs to our respawned hero-unit
            if (isComputer == false) {
                newPlayer.tag = "Player";
                if (SkillsDatabase.instance != null) {
                    SkillsDatabase.instance.Assign_Abilities_To_PlayerObject (newPlayer);
                }
                MatchHandler.instance.playerHeroRoot = newPlayer.transform; 
            } else if (isComputer == true) {
                // If we ARE a computer-player(-slot), grab appropriate abilities

            }
        } else {
            Debug.LogError ("Spawn_Players() Error! |>>| " + MatchHandler.instance.baseHeroPrefab + " || " + PlayerProfile.instance);
        }
    }
    #endregion respawning

    #region fountain_timer
    public void Use_Fountain () {
        // When player tries to use a 'fountain'/repairpad, this will trigger

        // If succesful, will apply heal-HoT & energy-regen-HoT
        // Will also start the fountain-cooldown-timer

        // If not, BONK BONK error sfx

    }

    IEnumerator Fountain_Cooldown () {
        yield return new WaitForSeconds (120f); // HOTS is 120 sec (?)
        // fountainUseable = true;
        yield return null;
    }
    #endregion fountain_timer

}
