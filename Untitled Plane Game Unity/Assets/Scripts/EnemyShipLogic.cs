using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class EnemyShipLogic : EnemyPlaneLogic
{

    protected Rigidbody2D ship_body;
    protected PolygonCollider2D ship_collider;

    [SerializeField]
    protected List<GameObject> cannons;

    [SerializeField]
    protected List<MeshRenderer> renderers;

    [SerializeField]
    protected List<MeshRenderer> renderers_debris;

    protected int move_dir = 0;
    protected float barrage_dur = 1f;
    protected int barrage_num_shots = 3;
    protected float barrage_delay;
    protected float nextBarrage = 0;
    protected float next_cannon_turn = 0;
    protected float fire_barrage = 0;
    protected bool destroyed = false;
    protected float sinkTime = 0;
    protected float sinkDuration = 6;
    protected float sink_height = -2;

    void Start()
    {
        game = GameBehaviour.Instance;
        ship_body = GetComponent<Rigidbody2D>();
        ship_collider = GetComponent<PolygonCollider2D>();
        projectilePrefab = game.getProjectileType(projectileType);
        setTeam(2);
        setHealth(200);
        setMaxHealth(200);
        barrage_delay = 2;
        fireDelay = barrage_dur/barrage_num_shots;
        smokeThreshold = 0.5f;
    }


    protected override void Update()
    {

        var player = game.PlayerEntity;

        if (player.activeSelf && !destroyed)
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            float dist_x_abs = Mathf.Abs(playerPos.x - ship_body.transform.position.x);

            if (dist_x_abs > 60)
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

            if ( next_cannon_turn <= Time.time )
            { 
                var cannon_pos = cannons[0].transform.position;
                Vector3 dir = (playerPos - cannon_pos).normalized;
                dir.x = Mathf.Clamp(dir.x, -0.8f, 0.8f);
                dir.y = Mathf.Clamp(dir.y, 0f, 1);

                cannons[0].transform.up = Vector3.RotateTowards(cannons[0].transform.up, dir, 5 * Time.deltaTime, 0.0f);
                cannons[0].transform.eulerAngles = new Vector3( 0, 0, cannons[0].transform.eulerAngles.z );
            }
        }

        doSmokeEffect();

    }

    private void FixedUpdate()
    {

        if ( move_dir != 0 && !destroyed )
        {
            var move_pos = new Vector2(move_dir * maxSpeed * Time.fixedDeltaTime, 0);
            ship_body.MovePosition(ship_body.position + move_pos);
        }

        /*var update_pos = ship_body.transform.position;
        update_pos.x = game.calculateLoopingX(update_pos.x);
        if ( !destroyed )
            update_pos.y = game.getWaterLevel();

        ship_body.transform.position = update_pos;*/

        if ( destroyed && sinkTime >= Time.time )
        {
            float sink = sink_height * Mathf.Clamp(1 - (sinkTime - Time.time) / sinkDuration, 0, 1);
            var move_pos = new Vector2( 0, sink * Time.fixedDeltaTime);
            ship_body.MovePosition(ship_body.position + move_pos);
        }

    }

    protected override void LateUpdate()
    {
        var update_pos = ship_body.transform.position;
        update_pos.x = game.calculateLoopingX(update_pos.x);
        if (!destroyed)
            update_pos.y = game.getWaterLevel();

        ship_body.transform.position = update_pos;
    }

    protected override void fireProjectile()
    {
        if (nextBarrage <= Time.time)
        {
            next_cannon_turn = fireDelay * barrage_num_shots + Time.time;
            fire_barrage = next_cannon_turn * 1;
            nextBarrage = barrage_delay + Time.time + UnityEngine.Random.Range( 0.4f, 0.9f) ;
        }

        if (nextFire <= Time.time && fire_barrage > Time.time)
        {
            var spawn_pos = cannons[0].transform.position + cannons[0].transform.up * 2.2f;

            var dir = cannons[0].transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range(-1, 1)) * dir;

            game.createProjectile(projectilePrefab, spawn_pos, dir, 80, 25, getTeam(), Vector3.zero);

            playSound(0, 0.5f);

            nextFire = fireDelay + Time.time;   
        }
       
    }

    protected override void doDestroy()
    {
        if (destroyed)
            return;

        destroyed = true;
        move_dir = 0;

        var exp_pos = ship_body.transform.position;
        exp_pos.z -= 2;

        ship_collider.enabled = false;

        game.doScreenshake(ship_body.transform.position, 8, 1);

        GameObject explosionObj = Instantiate(explosion_fx, exp_pos, Quaternion.identity);

        foreach ( MeshRenderer renderer in renderers )
        {
            renderer.enabled = false;
        }

        foreach (MeshRenderer renderer in renderers_debris)
        {
            renderer.enabled = true;
        }

        sinkTime = Time.time + sinkDuration;

        Invoke("realDoDestroy", sinkDuration);

        playSound(2);

        game.addScore(score, ship_body.transform.position);
        game.decrementEnemyCount();
       
    }

    private void realDoDestroy()
    {
        Destroy(gameObject);
    }
}
