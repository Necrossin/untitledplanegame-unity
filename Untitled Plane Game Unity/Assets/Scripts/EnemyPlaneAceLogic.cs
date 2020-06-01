using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneAceLogic : EnemyPlaneLogic
{

    protected float barrage_dur = 0.5f;
    protected int barrage_num_shots = 5;
    protected float barrage_delay;
    protected float nextBarrage = 0;
    protected float fire_barrage = 0;
    private bool destroyed = false;

    void Start()
    {
        game = GameBehaviour.Instance;
        plane_body = GetComponent<Rigidbody2D>();
        plane_collider = GetComponent<PolygonCollider2D>();
        projectilePrefab = game.getProjectileType(projectileType);
        setTeam(2);
        setMaxHealth(maxHealth);
        setHealth(getMaxHealth());
        maxSpeed = Random.Range(70f, 80f);
        rotSpeed = Random.Range(300, 310);
        closeup_range = 3;
        acceleration_range = 10;
        barrage_delay = 1;
        fireDelay = barrage_dur / barrage_num_shots;
        smokeThreshold = 0.5f;
        desired_goal_ang = 20;
        attack_range = 50;
    }

    protected override void Update()
    {
        if ( !destroyed )
            base.Update();
        else
        {
            thrustInput = 0;
            rotInput = -1;
            rotSpeed = 700;
            handleEffects();

            if (plane_body.transform.position.y < game.getWaterLevel())
                realDoDestroy();
        }   
    }

    protected override void handleEffects()
    {
        doSmokeEffect();
        doJetEffect();
        doWaterEffect();
    }

    protected override void fireProjectile()
    {
        if (nextBarrage <= Time.time)
        {
            fire_barrage =  fireDelay * barrage_num_shots + Time.time;
            nextBarrage = barrage_delay + Time.time + Random.Range(0.4f, 0.9f);
        }

        if (nextFire <= Time.time && fire_barrage > Time.time)
        {
            var spawn_pos = plane_body.transform.position + plane_body.transform.up * 5.5f;

            var dir = plane_body.transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range(-5, 5)) * dir;

            game.createProjectile(projectilePrefab, spawn_pos, dir, maxSpeed * 2, 25, getTeam(), plane_body.velocity);

            playSound(0);

            nextFire = fireDelay + Time.time;
        }

    }

    protected override float GetRotPower()
    {
        if (IsAccelerating())
            return 0.7f;
        else
            return 1;
    }

    protected override void doDestroy()
    {
        if (destroyed)
            return;

        destroyed = true;

        plane_collider.enabled = false;

        game.doScreenshake(plane_body.transform.position, 5, 0.5f);
        GameObject explosionObj = Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);

        Invoke("realDoDestroy", 3);

        game.addScore(score, plane_body.transform.position);
        game.decrementEnemyCount();

    }
    private void realDoDestroy()
    {
        if (game.isOnScreen(plane_body.transform.position, 1.4f))
            game.doScreenshake(plane_body.transform.position, 8, 1);

        Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }
}
