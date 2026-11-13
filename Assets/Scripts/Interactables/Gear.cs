using System;
using UnityEngine;

public class Gear : Interactable
{
    public enum symbols 
    {
        Skull,
        Ship,
        OldCompass,
        NewCompass
    }

    [SerializeField] private symbols correctSymbol;
    [SerializeField] private symbols currentSymbol;
    [SerializeField] private SpriteRenderer symbolRenderer;
    [SerializeField] private Sprite[] sprites;

    [SerializeField] private bool islocked;

    public symbols CorrectSymbol => correctSymbol;
    public bool IsCorrect => currentSymbol == correctSymbol;
    public void Locked() => islocked = true;
    public void Unlocked() => islocked = false;

    protected override void Awake()
    {
        base.Awake();
        Refresh();
    }
    public override void DoInteract() 
    {
        if (islocked) return;
        int count = Enum.GetValues(typeof(symbols)).Length;
        currentSymbol = (symbols)(((int)currentSymbol + 1) % count);

        Refresh();
    }
    private void Refresh()
    {
        int i = (int)currentSymbol;
        if (symbolRenderer == null || sprites == null || i >= sprites.Length) return;
        symbolRenderer.sprite = sprites[i];
    }

    public void Reset()
    {
        currentSymbol = symbols.Skull;
    }
}
