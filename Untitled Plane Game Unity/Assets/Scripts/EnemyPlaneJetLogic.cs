using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneJetLogic : EnemyPlaneLogic
{

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
        maxSpeed = Random.Range(90f, 100f);
        rotSpeed = Random.Range(220, 230);
        fireDelay = 0.2f;
        acceleration_range = Random.Range(60, 65);
    }


    protected override void Update()
    {

        var player = game.PlayerEntity;

        if (player.activeSelf && !destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            /*float dist = playerPos.x - plane_body.transform.position.x;

            if (Mathf.Abs(dist) > game.level_width / 2)
            {
                if (playerPos.x < plane_body.transform.position.x)
                    playerPos.x += game.level_width;
                else
                    playerPos.x -= game.level_width;
            }*/

            // dont get too close to the water surface
            playerPos.y = Mathf.Max(playerPos.y, game.getWaterLevel() + game.getWaterLevelHeight());

            Vector3 dir = (playerPos - plane_body.transform.position).normalized;

            float goal_ang = Mathf.Atan2(dir.y, dir.x);
            goal_ang = (goal_ang / Mathf.PI) * 180;

            float real_rot = normalizeAngle(plane_body.rotation);
            float real_rot_abs = Mathf.Abs(real_rot);

            float diff = normalizeAngle(goal_ang - plane_body.rotation + 90);
            float diff_abs = Mathf.Abs(diff - 180);

            if (diff_abs > desired_goal_ang)
            {
                if (diff > 0)
                    rotInput = 1;
                else
                    rotInput = -1;
            }
            else
                rotInput = 0;


            float distSqr = (playerPos - plane_body.transform.position).sqrMagnitude;

            // zip past player, only do quick turns when far away
            if ((distSqr < acceleration_range * acceleration_range) || (distSqr > acceleration_range * acceleration_range && diff_abs < 30) || (plane_body.transform.position.y < (game.getWaterLevel() + 3) && real_rot_abs < 80) || (plane_body.transform.position.y > (game.getSkyLevel() - 3) && real_rot_abs > 140))
            {
                thrustInput = 1;
            }
            else
            {
                thrustInput = 0;
            }

            var ahead_vec = plane_body.transform.position + plane_body.transform.up * game.getWaterLevelHeight();

            if (ahead_vec.y < game.getWaterLevel() && plane_body.transform.position.y > game.getWaterLevel() || ahead_vec.y > game.getSkyLevel() && plane_body.transform.position.y > game.getSkyLevel())
                thrustInput = 0;

            if (diff_abs < 40 && distSqr < 50 * 50)
            {
                fireProjectile();
            }
        }

        if (destroyed)
        {
            thrustInput = 0;
            rotInput = 1;
            rotSpeed = 1300;
            if (plane_body.transform.position.y < game.getWaterLevel())
                realDoDestroy();
        }
        else
            handleEnvDamage( true );

        
        handleEffects();
    }

    protected override void handleEffects()
    {
        doSmokeEffect();
        doJetEffect();
        doWaterEffect();
    }

    protected override float GetRotPower()
    {
        if (IsAccelerating())
            return 0.15f;
        else
            return 1;
    }

    protected override void onTeleported(float old_x, float new_x)
    {
        var trail = GetComponentInChildren<TrailRenderer>();

        Vector3 new_offset = new Vector3(game.level_width, 0, 0);

        if (new_x < old_x)
            new_offset *= -1;


        for ( int i = 0; i < trail.positionCount; i++ )
        {
            trail.SetPosition(i, trail.GetPosition(i) + new_offset);
        }

    }

    protected override void doDestroy()
    {
        if (destroyed)
            return;

        destroyed = true;

        plane_collider.enabled = false;

        game.doScreenshake(plane_body.transform.position, 4, 0.5f);
        Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);

        Invoke("realDoDestroy", 3);

        if (nextEnvDamage < Time.time)
            game.addScore(score, plane_body.transform.position);

        game.decrementEnemyCount();

    }
    private void realDoDestroy()
    {
        if ( game.isOnScreen(plane_body.transform.position, 1.2f ) )
            game.doScreenshake(plane_body.transform.position, 8, 1);

        Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);
        gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {

    }

    protected override void OnTriggerStay2D(Collider2D other)
    {

    }
}
