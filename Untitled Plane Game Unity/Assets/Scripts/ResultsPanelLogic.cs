using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultsPanelLogic : MonoBehaviour
{
    private GameBehaviour game;

    [SerializeField]
    private TextMeshProUGUI bestText;
    [SerializeField]
    private TextMeshProUGUI lastText;
    [SerializeField]
    private GameObject highScoreText;
    [SerializeField]
    private Animation fadeInAnim;
    [SerializeField]
    private Animation highScoreAnim;

    private float clickTime = 0;

    void Start()
    {
        game = GameBehaviour.Instance;
    }

    public void updateScoreResults( int best, int last, bool newHighscore = false )
    {
        fadeInAnim.Play();
        bestText.text = "BEST: " + string.Format("{0:D8}", best);
        lastText.text = "LAST: " + string.Format("{0:D8}", last);
        clickTime = Time.time + 1.5f;

        if (newHighscore)
        {
            highScoreText.SetActive(true);
            highScoreAnim.Play();
        }
        
    }

    void Update()
    {
        if (Input.anyKey && clickTime <= Time.time)
            game.restartGame();
    }
}
