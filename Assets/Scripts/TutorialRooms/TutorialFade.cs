using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFade : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeTime = 0.3f;
    private Coroutine routine;

    public void Show(string text, Sprite sprite)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Swap(text, sprite));
    }
    public void SetPrompt(string text) => Show(text, null);

    public IEnumerator Swap(string text, Sprite sprite) 
    {
        yield return Fade(group.alpha, 0f);
        label.text = text;
        image.sprite = sprite;
        image.gameObject.SetActive(sprite != null);
        yield return Fade(0f, 1f);
        routine = null;
    }

    public IEnumerator Fade(float from, float to) 
    {
        float t = 0f;
        while (t < fadeTime) 
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t/fadeTime);
            yield return null;
        }
        group.alpha = to;
    }

}
