using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class InfoPopup : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    private void Awake()
    {
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
        _canvas.gameObject.SetActive(true);
    }

    public void Unhighlight()
    {
        _canvas.gameObject.SetActive(false);
    }
}
