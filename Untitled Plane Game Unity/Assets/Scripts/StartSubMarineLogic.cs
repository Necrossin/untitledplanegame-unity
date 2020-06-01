using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSubMarineLogic : MonoBehaviour
{
    private GameBehaviour game;

    private float moveTime = 0;
    private float moveDur = 1;
    //private float moveY = 0;
    private bool moveUp = false;

    private bool shouldHide = false;

    [SerializeField]
    private AudioSource launchSound;

    void Start()
    {
        game = GameBehaviour.Instance;
        moveSubMarine( true, 1);
    }

    
    void Update()
    {
        if (game.PlayerEntity.activeSelf && !shouldHide )
        {
            shouldHide = true;
            moveSubMarine(false, 3);
            Invoke("doDestroy", 5);
            launchSound.Play();
        }
        
        float cur_y = Mathf.Clamp( 1 - (moveTime - Time.time) / moveDur, 0, 1);

        if (!moveUp)
            cur_y = 1 - cur_y;
        
        gameObject.transform.position = new Vector3(0, game.getWaterLevel() + cur_y * 4 - 4, 0);
    }


    public void moveSubMarine( bool up, float dur )
    {
        moveDur = dur;
        //moveY = up ? 1 : 0;
        moveUp = up;
        moveTime = Time.time + dur;
    }

    private void doDestroy()
    {
        Destroy(gameObject);
    }
}
