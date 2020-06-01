using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupLogic : MonoBehaviour
{
    private GameBehaviour game;
    private Vector3 save_pos;

    void Start()
    {
        game = GameBehaviour.Instance;
        save_pos = transform.position;

        if (game.getCombo() >= 20)
            GetComponent<AudioSource>().Play();

        Destroy(gameObject, 3);
        //transform.localPosition += new Vector3(0, 1f, 0);
    }

    void Update()
    {
        save_pos.x = game.calculateLoopingX(save_pos.x);
        transform.position = game.toScreenView(save_pos);
    }

}
