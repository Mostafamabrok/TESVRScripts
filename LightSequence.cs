using System.Collections;
using UnityEngine;

public class HierarchicalLightActivator : MonoBehaviour
{
    public float delayBetweenLights = 0.2f;

    private void Start()
    {
        // Turn off all child lights at startup
        Light[] lights = GetComponentsInChildren<Light>(includeInactive: true);
        foreach (var light in lights)
        {
            light.enabled = false;
        }
    }

    public IEnumerator ActivateLightsInOrder()
    {
        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (!child.name.StartsWith("Light")) continue;

            Light light = child.GetComponentInChildren<Light>();
            AudioSource audio = child.GetComponentInChildren<AudioSource>(); // changed to search in children

            if (light != null)
                light.enabled = true;

            if (audio != null)
                audio.Play();

            yield return new WaitForSeconds(delayBetweenLights);
        }
    }

}

