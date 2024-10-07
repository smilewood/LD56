using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private InfoPopup _currentInfoPopup;

    public void Highlight(Transform target)
    {
        if (target.GetComponent<InfoPopup>() != null)
        {
            _currentInfoPopup = target.GetComponent<InfoPopup>();
            _currentInfoPopup.Highlight();
        }
    }

    public void Unhighlight()
    {
        if (_currentInfoPopup != null)
        {
            _currentInfoPopup.Unhighlight();
        }
    }
}
