using UnityEngine;

public class Interactable : MonoBehaviour
{
    private Outline _outline;

    protected void Awake()
    {
        _outline = gameObject.AddComponent<Outline>();
        _outline.OutlineWidth = 0;
        _outline.OutlineMode = Outline.Mode.OutlineVisible;
    }

    public void Highlight()
    {
        _outline.OutlineWidth = 10;
    }

    public void Unhighlight()
    {
        _outline.OutlineWidth = 0;
    }
}
