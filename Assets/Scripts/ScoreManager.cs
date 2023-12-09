using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : Singleton<ScoreManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private AudioMixer audiomixer;


    [SerializeField]
    private Text scoreText, comboText;

    [SerializeField]
    private Image comboSlider;

    [SerializeField]
    private Image bottleFillImage;

    private int score, comboMultiplier;
    public int Score
    {
        get
        {
            return score;
        }
    }
    [SerializeField]
    private float ComboLimitTime = 5;
    private float currentComboTime;
    private float timeSinceLastScore;

    private int maxCombo;
    public int MaxCombo
    {
        get
        {
            return maxCombo;
        }
    }

    private bool timerIsActive;

    [SerializeField]
    private Transform BottlePosition;

    [SerializeField]
    private ParticleSystem splat;


    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        audiomixer = AudioMixer.Instance;

        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount * IncreaseCombo();
        scoreText.text = score.ToString();

        bottleFillImage.fillAmount += amount / 100.0f;
        splat.Play();
        if (bottleFillImage.fillAmount >= 1f)
        {
            score += 1000;
            bottleFillImage.fillAmount = 0f;
        }

        timeSinceLastScore = 0;

        if (!timerIsActive)
            StartCoroutine(ComboTimer());

        audiomixer.PlaySound(SoundEffects.score);
    }

    private IEnumerator ComboTimer()
    {
        timerIsActive = true;
        comboText.enabled = true;
        comboSlider.gameObject.SetActive(true);

        do
        {
            timeSinceLastScore += Time.deltaTime;
            comboSlider.fillAmount = 1 - timeSinceLastScore / currentComboTime;
            yield return null;
        }
        while (timeSinceLastScore < currentComboTime);

        comboMultiplier = 0;
        comboText.enabled = false;
        comboSlider.gameObject.SetActive(false);
        timerIsActive = false;
    }


    private int IncreaseCombo()
    {
        comboText.text = "x" + ++comboMultiplier;
        maxCombo = Mathf.Max(maxCombo, comboMultiplier);

        currentComboTime = ComboLimitTime - Mathf.Log(comboMultiplier) / 2;

        return comboMultiplier;
    }

    public IEnumerator ResolveMatch(Match toResolve, MatchType powerupUsed = MatchType.invalid)
    {
        Matchable powerupFormed = null;
        Matchable matchable;

        Transform target = BottlePosition;

        if (powerupUsed == MatchType.invalid && toResolve.Count > 3)
        {
            powerupFormed = pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.Type);
            toResolve.RemoveMatchable(powerupFormed);
            target = powerupFormed.transform;
            powerupFormed.SortingOrder = 3;

            audiomixer.PlaySound(SoundEffects.upgrade);
        }
        else
        {
            audiomixer.PlaySound(SoundEffects.resolve);
        }

        for (int i = 0; i != toResolve.Count; ++i)
        {
            matchable = toResolve.Matchables[i];

            if (powerupUsed != MatchType.match4 && matchable.IsBomb)
                continue;

            grid.RemoveItemAt(matchable.position);

            if (i == toResolve.Count - 1)
                yield return StartCoroutine(matchable.Resolve(target));
            else
                StartCoroutine(matchable.Resolve(target));
        }
        AddScore(toResolve.Count * toResolve.Count);

        if (powerupFormed != null)
            powerupFormed.SortingOrder = 0;
    }
}