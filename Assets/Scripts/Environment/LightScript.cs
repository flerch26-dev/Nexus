using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightScript : MonoBehaviour
{
    Light lightSource;

    float flickerDuration = 2f;  // Duration for the flicker effect
    float minIntensity = 0f;     // Minimum light intensity during flickering
    float maxIntensity = 1f;     // Maximum light intensity during flickering
    float flickerSpeed = 0.1f;   // Speed at which the light flickers

    private float timeElapsed = 0f;

    private void Start()
    {
        lightSource = transform.GetComponent<Light>();
    }

    public void SetLightIntensity(float intensity)
    {
        lightSource.intensity = intensity;
    }

    public IEnumerator FlickerLights()
    {
        lightSource.enabled = true; // Enable the light

        while (timeElapsed < flickerDuration)
        {
            lightSource.intensity = Random.Range(minIntensity, maxIntensity); // Random flicker
            timeElapsed += flickerSpeed;
            yield return new WaitForSeconds(flickerSpeed); // Pause before next flicker
        }

        // Once flickering is done, set light to full intensity
        lightSource.intensity = maxIntensity;
    }
}
