using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private Light _light;

    private void Awake()
    {
        _light = GetComponentInChildren<Light>();
    }

    public void Highlight(Transform target)
    {
        if (target.GetComponent<BuildingManager>() != null)
        {
            _light.spotAngle = 10f;
            _light.innerSpotAngle = 10f;
        }
        else
        {
            _light.innerSpotAngle = 35f;
            _light.spotAngle = 35f;
        }

        transform.position = target.position;

        _light.enabled = true;
    }

    public void Unhighlight()
    {
        _light.enabled = false;
    }
}
