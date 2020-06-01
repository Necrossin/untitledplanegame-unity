using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDLogic : MonoBehaviour
{

    private GameBehaviour game;

    [SerializeField]
    GameObject startText;

    [SerializeField]
    GameObject scoreText;

    [SerializeField]
    GameObject tempScoreText;

    [SerializeField]
    GameObject comboMeterText;

    private int cur_score = 0;
    private bool played_combo_sound = false;


    void Start()
    {
        game = GameBehaviour.Instance;
    }

    
    void Update()
    {

        startText.SetActive(!game.isGameActive());

        updateScore();
        updateTempScore();
        updateComboMeter();
    }

    private void updateScore()
    {
        scoreText.SetActive(game.isGameActive() && !game.isPlayerDead());

        if (!scoreText.activeSelf)
            return;

        cur_score = Mathf.RoundToInt(Mathf.Clamp(cur_score + 5000 * Time.deltaTime, 0, game.getScore()));

        scoreText.GetComponent<TextMeshProUGUI>().text = string.Format("{0:D6}", cur_score );
    }

    private void updateTempScore()
    {
        tempScoreText.SetActive(game.getTempScore() > 0);

        if (!tempScoreText.activeSelf)
            return;

        tempScoreText.GetComponent<TextMeshProUGUI>().text = string.Format("{0:D6}", game.getTempScore());
    }

    private void updateComboMeter()
    {
        comboMeterText.SetActive(game.getTempScore() > 0);

        if (!comboMeterText.activeSelf)
            return;

        comboMeterText.GetComponent<TextMeshProUGUI>().text = game.getCombo() >= 20 ? "MAX!" : "x" + game.getCombo();

        // adjust gradient texture offset as combo time runs out
        // and yes: this is terrible, but it does the trick
        float offset_delta = game.getComboDelta() * 0.39f;
        comboMeterText.GetComponent<TextMeshProUGUI>().materialForRendering.SetTextureOffset( "_FaceTex", new Vector2( 0, 0.42f - offset_delta ) );

        float white_flash;

        if (game.getComboDelta() >= 0.9)
        {
            if (!played_combo_sound)
            {
                var audio = comboMeterText.GetComponent<AudioSource>();
                audio.Stop();
                audio.Play();
                played_combo_sound = true;
            }
            white_flash = Mathf.PingPong(Time.time * 15, 1);
        }
        else
        {
            played_combo_sound = false;
            white_flash = 1;
        }
            

        var col = comboMeterText.GetComponent<TextMeshProUGUI>().color;
        col.a = white_flash;

        comboMeterText.GetComponent<TextMeshProUGUI>().color = col;
    }

}
