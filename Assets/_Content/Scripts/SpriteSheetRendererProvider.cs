using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using vegeo;

public class SpriteSheetRendererProvider : MonoBehaviour
{
    private static SpriteSheetRendererProvider _instance;
    public static SpriteSheetRendererProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SpriteSheetRendererProvider>();
            }
            return _instance;
        }
    }

    public Mesh QuadMesh;
    public Material CreatureMaterial;

    public void Awake()
    {
        _instance = this;
    }
}
