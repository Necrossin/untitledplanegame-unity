using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlaneLogic : PlayerLogic
{

    //private GameObject player;
    protected float closeup_range;
    protected float acceleration_range;
    protected float desired_goal_ang = 30;
    protected float attack_range = 30;

    public int score;

    void Start()
    {
        game = GameBehaviour.Instance;
        plane_body = GetComponent<Rigidbody2D>();
        plane_collider = GetComponent<PolygonCollider2D>();
        projectilePrefab = game.getProjectileType(projectileType);
        setTeam(2);
        setMaxHealth(maxHealth);
        setHealth(getMaxHealth());
        maxSpeed = Random.Range(30f, 50f);
        rotSpeed = Random.Range(100, 200);
        fireDelay = Random.Range( 0.4f, 1.2f );
        closeup_range = Random.Range(3, 10);
        acceleration_range = Random.Range(20, 35);
    }

    protected virtual void Update()
    {

        var player = game.PlayerEntity;

        if ( player.activeSelf )
        {
            var player_body = player.GetComponent<Rigidbody2D>();
            var playerPos = player_body.transform.position;

            playerPos.y = Mathf.Max(playerPos.y, game.getWaterLevel());

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

            if ( (distSqr > acceleration_range * acceleration_range && diff_abs < 40 ) || ( plane_body.transform.position.y < (game.getWaterLevel() + 3) && real_rot_abs < 70 ))
            {
                thrustInput = 1;
            }
            else
            {
                if ( distSqr < closeup_range * closeup_range)
                    thrustInput = 1;
                else
                    thrustInput = 0;
            }


            if (diff_abs < 40 && distSqr < attack_range * attack_range)
            {
                fireProjectile();
            }                

        }

        handleEffects();

    }

    protected override void handleEffects()
    {
        doWaterEffect();
    }

    private void FixedUpdate()
    {
        handleMotion();

        /*var update_pos = plane_body.transform.position;
        float old_x = plane_body.transform.position.x;
        update_pos.x = game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            onTeleported(old_x, update_pos.x);
        }
            

        if (plane_body.transform.position.y < game.levelMins().y)
            update_pos.y = game.levelMins().y;
        if (plane_body.transform.position.y > game.levelMaxs().y)
            update_pos.y = game.levelMaxs().y;

        plane_body.transform.position = update_pos;*/
    }

    protected virtual void LateUpdate()
    {
        var update_pos = plane_body.transform.position;
        float old_x = plane_body.transform.position.x;
        update_pos.x = game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            onTeleported(old_x, update_pos.x);
        }


        if (plane_body.transform.position.y < game.levelMins().y)
            update_pos.y = game.levelMins().y;
        if (plane_body.transform.position.y > game.levelMaxs().y)
            update_pos.y = game.levelMaxs().y;

        plane_body.transform.position = update_pos;
    }


    protected override void doDestroy()
    {
        game.doScreenshake(plane_body.transform.position, 2, 0.5f);
        Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);
        game.addScore(score, plane_body.transform.position);
        game.decrementEnemyCount();
        gameObject.SetActive(false);
        Destroy(gameObject,1f);
    }

    protected virtual void onTeleported( float old_x, float new_x)
    {

    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {

    }

    protected override void OnTriggerStay2D(Collider2D other)
    {

    }

    protected override void doMuzzleflash()
    {
        
    }

}
