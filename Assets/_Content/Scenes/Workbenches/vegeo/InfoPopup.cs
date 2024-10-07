using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InfoPopup : MonoBehaviour
{
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInChildren<Canvas>();

        LookAtConstraint constraint = _canvas.transform.GetComponent<LookAtConstraint>();
        constraint.AddSource(new ConstraintSource
        {
            sourceTransform = Camera.main.transform,
            weight = 1
        });
        constraint.constraintActive = true;
    }

    public void Highlight()
    {

    }

    public void Unhighlight()
    {

    }
}
