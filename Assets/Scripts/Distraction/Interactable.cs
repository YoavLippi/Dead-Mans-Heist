using System;
using UnityEngine;
using UnityEngine.Events;

//very basic helper script for now, but it should let us categorise later
public abstract class Interactable : MonoBehaviour
{
    public UnityEvent OnInteract;
    [SerializeField] private SpriteRenderer sr;
    private MaterialPropertyBlock materialProperties;
    
    private static readonly int OutlineSizeID = Shader.PropertyToID("_OutlineSize");

    private void Awake()
    {
        materialProperties = new MaterialPropertyBlock();
    }

    public virtual void DoInteract()
    {
        throw new NotImplementedException("Please use a child of the interactable parent, not the parent itself");
    }

    public virtual void SetOutlineWidth(float w)
    {
        sr.GetPropertyBlock(materialProperties);
        materialProperties.SetFloat(OutlineSizeID,w);
        //sr.material.SetFloat(OutlineSizeID, w);
        sr.SetPropertyBlock(materialProperties);
    }
}
