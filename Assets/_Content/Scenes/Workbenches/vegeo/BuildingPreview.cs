using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    public bool ValidPlacement { get; private set; }

    private int _numOverlaps;

    public void OnEnable()
    {
        _numOverlaps = 0;
        ValidPlacement = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        ValidPlacement = false;
        _numOverlaps++;
    }

    public void OnTriggerExit(Collider other)
    {
        _numOverlaps--;
        if (_numOverlaps <= 0)
        {
            ValidPlacement = true;
        }
    }
}
