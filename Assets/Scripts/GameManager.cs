using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : Singleton<GameManager>
{
    private MatchablePool pool;
    private MatchableGrid grid;
    private Cursor cursor;
    private ScoreManager score;
    private AudioMixer audiomixer;

    [SerializeField] private Fader loadingScreen, darkener;
    [SerializeField] private Timer timer;

    [SerializeField] private Vector2Int dimensions = Vector2Int.one;

    [SerializeField] private Text ScoreText, ComboText;

    [SerializeField] private GameObject result;

    private bool timerEnd;
    public bool TimerEnd
    {
        set { timerEnd = value; }
    }


    void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        cursor = Cursor.Instance;
        score = ScoreManager.Instance;
        audiomixer = AudioMixer.Instance;

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.5f);

        result.SetActive(false);

        cursor.enabled = false;

        loadingScreen.Hide(true);


        pool.PoolObjects(dimensions.x * dimensions.y * 2);

        grid.InitializeGrid(dimensions);


        StartCoroutine(loadingScreen.Fade(0));

        audiomixer.PlayMusic();


        yield return StartCoroutine(grid.PopulateGrid(false, true));

        cursor.enabled = true;

        yield return StartCoroutine(timer.Countdown(30f));
    }

    public void NoMoreMoves()
    {
        if (!timerEnd)
            grid.MatchEverything();
    }

    public void GameOver()
    {
        ScoreText.text = score.Score.ToString();
        ComboText.text = score.MaxCombo.ToString();

        cursor.enabled = false;

        darkener.Hide(true);
        StartCoroutine(darkener.Fade(0.8f));
        result.SetActive(true);

        audiomixer.PlaySound(SoundEffects.upgrade);
        audiomixer.StopMusic();
    }
}
