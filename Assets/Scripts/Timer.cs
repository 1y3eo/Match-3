using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private GameManager gm;

    [SerializeField] private Image timerFillImage;

    private float curTime;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    public IEnumerator Countdown(float timeLimit)
    {
        StopAllCoroutines();
        curTime = timeLimit;

        do
        {
            curTime -= Time.deltaTime;
            timerFillImage.fillAmount = curTime / timeLimit;
            yield return null;
        }
        while (curTime > 0.0f);

        gm.TimerEnd = true;
        gm.GameOver();
    }
}
