using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBattleShipLogic : EnemyShipLogic
{
    
    void Start()
    {
        game = GameBehaviour.Instance;
        ship_body = GetComponent<Rigidbody2D>();
        ship_collider = GetComponent<PolygonCollider2D>();
        projectilePrefab = game.getProjectileType(projectileType);
        setTeam(2);
        setHealth(1000);
        setMaxHealth(1000);
        barrage_delay = 2;
        barrage_num_shots = 8;
        fireDelay = barrage_dur / barrage_num_shots;
        smokeThreshold = 0.4f;
    }


    protected override void Update()
    {
        var player = game.PlayerEntity;

        if (player.activeSelf && !destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;
            var playerPos_ahead = playerPos + new Vector3( player_body.velocity.normalized.x, 0, 0 ).normalized * 35;

            float dist_x_abs = Mathf.Abs(playerPos.x - ship_body.transform.position.x);

            if (dist_x_abs > 100)
            {
                if (playerPos.x > ship_body.transform.position.x)
                    move_dir = 1;
                else
                    move_dir = -1;
            }
            else
                move_dir = 0;

            if (move_dir == 0)
                fireProjectile();

            if (next_cannon_turn <= Time.time)
            {
                foreach ( GameObject cannon in cannons )
                {
                    var cannon_pos = cannon.transform.position;
                    Vector3 dir = (playerPos_ahead - cannon_pos).normalized;
                    dir.x = Mathf.Clamp(dir.x, -0.8f, 0.8f);
                    dir.y = Mathf.Clamp(dir.y, 0f, 1);

                    cannon.transform.up = Vector3.RotateTowards(cannon.transform.up, dir, 3 * Time.deltaTime, 0.0f);
                    cannon.transform.eulerAngles = new Vector3(0, 0, cannon.transform.eulerAngles.z);
                }
            }
        }

        doSmokeEffect();
    }

    protected override void fireProjectile()
    {
        if (nextBarrage <= Time.time)
        {
            next_cannon_turn = fireDelay * barrage_num_shots + Time.time;
            fire_barrage = next_cannon_turn * 1;
            nextBarrage = barrage_delay + Time.time + UnityEngine.Random.Range(0.4f, 0.9f);
        }

        if (nextFire <= Time.time && fire_barrage > Time.time)
        {
            foreach (GameObject cannon in cannons)
            {
                var spawn_pos = cannon.transform.position + cannon.transform.up * 5;

                var dir = cannon.transform.up;
                dir = Quaternion.Euler(0, 0, Random.Range(-1, 1)) * dir;

                game.createProjectile(projectilePrefab, spawn_pos, dir, 80, 20, getTeam(), Vector3.zero);
                playSound(0, 0.7f);
            }

            nextFire = fireDelay + Time.time;
        }

    }
}
