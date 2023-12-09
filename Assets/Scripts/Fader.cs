using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fader : MonoBehaviour
{
    private Image toFade;
    private Color faded;

    [SerializeField] private float fadeSpeed;

    private void Awake()
    {
        toFade = GetComponent<Image>();
        faded = toFade.color;
    }

    public void Hide(bool hidden)
    {
        toFade.enabled = hidden;
    }

    public IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = faded.a;
        float curTime = 0;

        do
        {
            curTime += Time.deltaTime * fadeSpeed;
            faded.a = Mathf.Lerp(startAlpha, targetAlpha, curTime);
            toFade.color = faded;
            yield return null;
        }
        while (curTime < 1);
    }
}
