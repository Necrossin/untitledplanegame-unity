using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionFXLogic : MonoBehaviour
{

    [SerializeField]
    protected GameObject waterExplosion_fx;
    private GameBehaviour game;

    void Start()
    {
        game = GameBehaviour.Instance;
        
        if ( transform.position.y < (game.getWaterLevel() + 10) )
        {
            var exp_pos = transform.position;
            exp_pos.z = 0;
            exp_pos.y = game.getWaterLevel();

            Instantiate(waterExplosion_fx, exp_pos, Quaternion.identity);
        }   
    }
}
