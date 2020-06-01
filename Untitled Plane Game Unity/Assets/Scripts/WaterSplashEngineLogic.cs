using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplashEngineLogic : MonoBehaviour
{
    
    [SerializeField]
    private List<ParticleSystem> splashes;

    [SerializeField]
    private List<float> splash_vel;

    private float sizeX = 1;
    private float sizeY = 1;

    private Vector3 direction;

    private GameBehaviour game;

    void Start()
    {
        game = GameBehaviour.Instance;
    }

    
    void LateUpdate()
    {

        float rotation = GetComponentInParent<Rigidbody2D>().rotation;

        int index = 0;

        foreach ( ParticleSystem p in splashes )
        {
            var main = p.main;
            main.startSizeYMultiplier = sizeY;

            var scale = p.transform.localScale;
            scale.x = sizeX;
            p.transform.localScale = scale;

            var vel = p.velocityOverLifetime;
            vel.x = splash_vel[index] + direction.x * -1 * (10 + sizeX);
            index += 1;
        }

        var update_pos = transform.parent.position;
        update_pos.y = game.getWaterLevel();

        transform.position = update_pos;
        transform.rotation = Quaternion.identity;

        var scale2 = transform.localScale;
        scale2.x = sizeX * 3;
        transform.localScale = scale2;

    }

    public void setPower( float pow )
    {
        //sizeX = 1 + (pow - 1) / 3;
        //sizeY = pow * 1.5f;
        sizeX = Mathf.Lerp(sizeX, 1 + (pow - 1) / 3, 2f * Time.deltaTime);
        sizeY = Mathf.Lerp(sizeY, pow * 1.5f, 2f * Time.deltaTime);
    }

    public void setDirection( Vector3 dir )
    {
        direction = dir;
    }
}
