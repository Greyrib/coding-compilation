using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyHandler : MonoBehaviour
{
    public static GalaxyHandler instance;

    [Header ("References")]
    public Gradient universeColorGradient;
    public SpaceGraphicsToolkit.Starfield.SgtStarfieldBox[] starfields;
    public Material ags;
    public Transform planetsGameObject;
    public Light primaryLight;
    public Light sideLight;
    public AudioSource ambience;
    public AudioClip[] ambienceClips;
    public Transform sunScaleObject;
    public Parallax sunParallax;
    public Material sunMaterial;
    public Material sunCoronaMaterial;
    public SpaceGraphicsToolkit.Starfield.SgtStarfieldBox nebulaAlpha;
    public SpaceGraphicsToolkit.Starfield.SgtStarfieldBox nebulaBravo;
    public SpaceGraphicsToolkit.Belt.SgtBeltSimple beltSimple;
    public SpaceGraphicsToolkit.Belt.SgtBeltSimple beltSimple2;
    public SpaceGraphicsToolkit.Flare.SgtFlareMainTex flare;
    public SpaceGraphicsToolkit.Flare.SgtFlareMesh flareMesh;
    public Parallax flareParallax;
    [Space (5)]
    public GameObject asteroidPrefab;
    [Space (5)]
    public VolumetricFogAndMist.VolumetricFog volmist;

    [Header ("Background Spawnables")]
    public GameObject rubberduckyPrefab;

    [Header ("Dynamics")]
    public float unipoint;
    public float invertedUnipoint;

    [Header ("Debug")]
    public bool generateAsteroids;
    public bool generateSkyboxStars;
    public bool fullLighting;
    [Space (5)]
    public Color pointMinResult;
    public Color pointResult;
    public Color pointMaxResult;
    [Space (3)]
    public Color invertMinResult;
    public Color invertResult;
    public Color invertMaxResult;

    void Awake () {
        if (instance == null) instance = this; else Destroy (this);
        if (fullLighting) {
            Debug.Log ("NOTICE: fullLightning is ENABLED on GalaxyHandler - remember to disable this, if you aren't specifically testing something related to it.");
        }
    }

    public void Generate_Galaxy () {
        unipoint = Random.Range (0, 1f);
        invertedUnipoint = 1f - unipoint;

        Randomnize_AGS ();

        Randomnize_Planet ();
        
        Randomnize_Lighting ();

        Configure_Volmist ();

        Configure_Asteroids ();

        Generate_BackgroundObject ();

        if (PlatformHandler.instance != null) {
            PlatformHandler.instance.Generate_Sector_Platforms ();
        }

        if (ambience && ambienceClips.Length > 0) {
            StopAllCoroutines ();
            StartCoroutine (Fade_Ambience ());
        }

        Set_Debug_Results ();
    }

    IEnumerator Fade_Ambience () {
        while (ambience.volume > 0f) {
            ambience.volume -= Time.deltaTime;
            yield return new WaitForEndOfFrame ();
        }
        ambience.Stop ();
        ambience.clip = ambienceClips [Random.Range (0, ambienceClips.Length)];
        ambience.Play ();
        while (ambience.volume < 1f) {
            ambience.volume += Time.deltaTime;
            yield return new WaitForEndOfFrame ();
        }
        yield return null;
    }

    void Randomnize_AGS () {
        // Randomnize AGS material parameters
        foreach (SpaceGraphicsToolkit.Starfield.SgtStarfieldBox field in starfields) {
            float starfieldPoint = unipoint;
            if (ReferenceHandler.instance.CalculateChance (0.4f)) {
                starfieldPoint = invertedUnipoint;
            }

            Gradient gradient = new Gradient();

            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            GradientColorKey[] colorKey = new GradientColorKey[2];
            colorKey[0].color = universeColorGradient.Evaluate (starfieldPoint);
            colorKey[0].time = 0.0f;
            colorKey[1].color = universeColorGradient.Evaluate (starfieldPoint + Random.Range (-0.1f, 0.1f));
            colorKey[1].time = 1.0f;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
            alphaKey[0].alpha = 1.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f;
            alphaKey[1].time = 1.0f;

            gradient.SetKeys(colorKey, alphaKey);
            field.starColors = gradient;

            field.Brightness = Random.Range (0.025f, 0.075f);
            field.Seed = Random.Range (0, 1000);
        }

        // Midground Nebulas -------------------------------------------------------
        float nebulaBrightnessLowerLimit = 0.05f;
        float nebulaBrightnessUpperLimit = 0.1f;
        if (nebulaAlpha) {
            nebulaAlpha.Color = universeColorGradient.Evaluate (unipoint + Random.Range (-0.1f, 0.1f));
            nebulaAlpha.Seed = Random.Range (0, 1000);
            nebulaAlpha.Brightness = Random.Range (nebulaBrightnessLowerLimit, nebulaBrightnessUpperLimit);
        }

        if (nebulaBravo) {
            nebulaBravo.Color = universeColorGradient.Evaluate (unipoint + Random.Range (-0.1f, 0.1f));
            nebulaBravo.Seed = Random.Range (0, 1000);
            nebulaBravo.Brightness = Random.Range (nebulaBrightnessLowerLimit, nebulaBrightnessUpperLimit);
        }
        // Nebulas end ------------------------------------------------------------

        if (ags) {
            float varianceRandomFloat = 0.1f;

            ags.SetFloat ("_NebulaDensity", Random.Range (0.3f, 0.5f)); //ags.SetFloat ("_NebulaDensity", Random.Range (0f, 1f));
            ags.SetInt ("_NebulaOffset", Random.Range (0, 1000)); // This is actually the "SEED" in the inspector menu for nebula settings
            ags.SetFloat ("_NebulaAnimSpeed", Random.Range (0f, 0.1f)); //ags.SetFloat ("_NebulaAnimSpeed", Random.Range (0f, 0.5f));
            ags.SetColor ("_NebulaColor", universeColorGradient.Evaluate (unipoint + Random.Range (-varianceRandomFloat, varianceRandomFloat)));

            ags.SetFloat ("_CloudDensity", Random.Range (0f, 5f));
            ags.SetInt ("_CloudVisibility", Random.Range (1, 10));
            ags.SetFloat ("_CloudSharpness", Random.Range (0.5f, 1.5f));
            ags.SetInt ("_CloudOffset", Random.Range (0, 1000));
            ags.SetFloat ("_CloudAnimSpeed", 0.1f); //ags.SetFloat ("_CloudAnimSpeed", Random.Range (0f, 0.5f));
            ags.SetColor ("_CloudColor", universeColorGradient.Evaluate (unipoint + Random.Range (-varianceRandomFloat, varianceRandomFloat)));

            ags.SetFloat ("_TrailComplexity", Random.Range (0.5f, 1f)); //ags.SetFloat ("_TrailComplexity", Random.Range (0.5f, 3f));
            ags.SetFloat ("_TrailVisibility", Random.Range (0f, 0.9f));
            ags.SetFloat ("_TrailSize", Random.Range (0.8f, 1f)); //ags.SetFloat ("_TrailSize", Random.Range (0f, 1f));
            ags.SetInt ("_TrailSeed", Random.Range (0, 1000));
            ags.SetFloat ("_TrailAnimSpeed", Random.Range (0f, 2f));
            ags.SetColor ("_TrailColor", universeColorGradient.Evaluate (unipoint + Random.Range (-varianceRandomFloat, varianceRandomFloat)));

            ags.SetFloat ("_StarsAmount", Random.Range (0.3f, 1f)); // 0-1f
            if (generateSkyboxStars == false) {
                ags.SetFloat ("_StarsAmount", 0f);
            }
            ags.SetFloat ("_StarsLightRandomness", Random.Range (0.2f, 1f));
            ags.SetFloat ("_StarsSize", Random.Range (0.2f, 0.4f));
            ags.SetFloat ("_StarsColorVariation", Random.Range (0.5f, 0.5f)); //ags.SetFloat ("_StarsColorVariation", Random.Range (0f, 1f));
            ags.SetFloat ("_StarsBlinkingSpeed", Random.Range (0f, 1f));
            ags.SetColor ("_StarsColorOne", Color.white); //ags.SetColor ("_StarsColorOne", universeColorGradient.Evaluate (unipoint));
            ags.SetColor ("_StarsColorTwo", universeColorGradient.Evaluate (unipoint + Random.Range (-0.1f, 0.1f)));
        }
        
        // --- Flare ---
        if (flare && flareMesh && flareParallax) {
            flare.Color = universeColorGradient.Evaluate (unipoint + Random.Range (-0.05f, 0.05f));
            flareMesh.Radius = Random.Range (13f, 23f);
            flareMesh.WaveStrength = Random.Range (7f, 14f);
            flareMesh.WavePoints = Random.Range (2, 4);
            flareMesh.WavePower = Random.Range (1f, 4f);

            /*
            if (flareMesh.WavePower < 1f) {
                flareMesh.WavePoints = 0;
            }
            */

            flareMesh.WavePhase = Random.Range (0f, 170f);
            if (ReferenceHandler.instance.CalculateChance (0.5f)) {
                flareMesh.Noise = true;
                flareMesh.NoiseSeed = Random.Range (0, 100);
            } else {
                flareMesh.Noise = false;
            }
            
            flare.Attempt_Update_Texture ();
            //flareMesh.UpdateMesh ();
            flareParallax.Change_Origin (new Vector3 (Random.Range (-60f, 60f), flareParallax.origin.y, Random.Range (-35f, 35f)));
        }
        // -------------

        // --- Customize sun ---
        if (sunScaleObject) {
            if (ReferenceHandler.instance.CalculateChance (0.33f)) { // Chance whether this sector contains an 'up-close' sun
                sunScaleObject.gameObject.SetActive (true);
            } else {
                sunScaleObject.gameObject.SetActive (false);
            }
            float newSunScale = Random.Range (3f, 9f);
            sunScaleObject.localScale = new Vector3 (newSunScale, newSunScale, newSunScale);
        }
        if (sunParallax) {
            sunParallax.Change_Origin (new Vector3 (Random.Range (-100f, 100f), sunParallax.origin.y, Random.Range (-45f, 45f)));
        }
        if (sunMaterial) {
            Color allColor = universeColorGradient.Evaluate (unipoint + Random.Range (-0.1f, 0.1f));
            sunMaterial.SetColor ("_Cool", allColor);
            sunMaterial.SetColor ("_Warm", allColor);
            sunMaterial.SetColor ("_Hot", allColor);
            sunCoronaMaterial.SetColor ("_CoronaColor", allColor);
            sunCoronaMaterial.SetFloat ("_CoronaBoost", Random.Range (13f, 23f));
        }
        // ---------------------
    }

    void Configure_Asteroids () {
        if (IntroHandler.instance && IntroHandler.instance.spawnAsteroids == false) {
            if (beltSimple) {
                beltSimple.enabled = false;
            }
            if (beltSimple2) {
                beltSimple2.enabled = false;
            }
            return;
        }

        // Determine whether new sector is asteroid-rich or not
        bool isRich = ReferenceHandler.instance.CalculateChance (0.3f);
        //Debug.Log ("Sector_isRich: " + isRich + " @ " + Time.time.ToString ("F2"));
        // Background (parallaxed) asteroid field
        if (beltSimple) {
            beltSimple.enabled = isRich;
            if (beltSimple.enabled) {
                beltSimple.Seed = Random.Range (1, 1000);
            }
        }
        if (beltSimple2) {
            beltSimple2.enabled = isRich;
            if (beltSimple2.enabled) {
                beltSimple2.Seed = Random.Range (1, 1000);
            }
        }
        // ---------------------

        // Disperse asteroids in new sector
        int amount = 25;
        if (isRich) {
            amount = amount * 3;
            //Debug.Log ("Sector isRich, tripling asteroid-attempts.");
        }

        if (generateAsteroids) {
            List<Vector3> astPoses = new List<Vector3>();
            float minDist = 75f;
            int astCount = 0;
            for (int a = 0; a < amount; a++) {
                bool placeable = true;
                Vector3 newPos = ReferenceHandler.instance.RandomCircle (Vector3.zero, Random.Range (50f, 350f));
                for (int p = 0; p < astPoses.Count; p++) {
                    if (Vector3.Distance (newPos, astPoses[p]) <= minDist) {
                        placeable = false;
                    }
                }
                if (placeable) {
                    GameObject newAst = Instantiate (asteroidPrefab, newPos, Quaternion.Euler (Random.Range (-180f, 180f), Random.Range (-180f, 180f), Random.Range (-180f, 180f))) as GameObject;
                    newAst.name = asteroidPrefab.name + "_" + a;
                    astPoses.Add (newPos);
                    astCount += 1;
                }
            }
            //Debug.Log ("Asteroids placed, final count: " + astCount);
        }
        // ---------------------
    }

    void Randomnize_Planet () {
        if (planetsGameObject == null) {
            return;
        }

        foreach (Transform child in planetsGameObject.transform) {
            child.gameObject.SetActive (false);
        }

        int planetChoice = Random.Range (0, planetsGameObject.transform.childCount);
        planetsGameObject.transform.GetChild (planetChoice).gameObject.SetActive (true);
        planetsGameObject.transform.GetChild (planetChoice).rotation = Quaternion.Euler (Random.Range (-180f, 180f), Random.Range (-180f, 180f), Random.Range (-180f, 180f));

        float newPlanetScale = Random.Range (5f, 15f);
        planetsGameObject.transform.GetChild (planetChoice).localScale = new Vector3 (newPlanetScale, newPlanetScale, newPlanetScale);

        Parallax planetParallax = planetsGameObject.GetComponent<Parallax>();
        if (planetParallax) {
            float xOffset = 75f;
            float xValue = Random.Range (-xOffset, -xOffset);
            float zOffset = 75f;
            float zValue = Random.Range (-zOffset, zOffset);
            planetParallax.Change_Origin (new Vector3 (xValue, planetParallax.Current_Origin ().y, zValue));
        }
    }

    void Randomnize_Lighting () {
        if (primaryLight) {
            primaryLight.intensity = Random.Range (0f, 0.3f);
            if (fullLighting) {
                primaryLight.intensity = 1f;
            }
        }

        if (sideLight) {
            sideLight.color = universeColorGradient.Evaluate (unipoint);
            if (fullLighting) {
                sideLight.intensity = 1f;
            }
        }
    }

    void Configure_Volmist () {
        if (volmist) {
            volmist.color = universeColorGradient.Evaluate (unipoint);
            volmist.alpha = Random.Range (0.1f, 0.35f);
        }
    }

    void Set_Debug_Results () {
        pointMinResult = universeColorGradient.Evaluate (unipoint - 0.1f);
        pointResult = universeColorGradient.Evaluate (unipoint);
        pointMaxResult = universeColorGradient.Evaluate (unipoint + 0.1f);

        invertMinResult = universeColorGradient.Evaluate (invertedUnipoint - 0.1f);
        invertResult = universeColorGradient.Evaluate (invertedUnipoint);
        invertMaxResult = universeColorGradient.Evaluate (invertedUnipoint + 0.1f);
    }
 
    void Generate_BackgroundObject () {
        float[] chances = new float[] {0.9f, 0.1f};
        float choice = ReferenceHandler.instance.Choose (chances);
        //Debug.Log ("Background_Choice = " + choice);

        if (choice == 1f && rubberduckyPrefab != null) {
            // Rubberducky
            Vector3 spawnOrigin = new Vector3 (CameraControl.instance.transform.position.x, -100f, CameraControl.instance.transform.position.z);
            Quaternion spawnRot = Quaternion.Euler (Random.Range (-180f, 180f), Random.Range (-180f, 180f), Random.Range (-180f, 180f));
            GameObject ducky = Instantiate (rubberduckyPrefab, ReferenceHandler.instance.RandomCircle (spawnOrigin, CameraControl.instance.targetZoom * 2f), spawnRot) as GameObject;
            Rigidbody drb = ducky.GetComponent<Rigidbody>();
            if (drb != null) {
                var dir = new Vector3 (CameraControl.instance.transform.position.x, 0f, CameraControl.instance.transform.position.z) - new Vector3 (ducky.transform.position.x, 0f, ducky.transform.position.z);
                float pushForce = Random.Range (0.2f, 0.6f);
                drb.AddForce ((dir * pushForce) * Time.deltaTime, ForceMode.VelocityChange);
                float torque = Random.Range (200f, 400f);
                drb.AddTorque (new Vector3 (Random.Range (-torque, torque), Random.Range (-torque, torque), Random.Range (-torque, torque)) * Time.deltaTime);
            }
        }
    }

}
