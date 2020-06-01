using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    [SerializeField]
    private GameObject hit_fx;
    [SerializeField]
    private GameObject hitWater_fx;

    private Rigidbody2D proj_body;
    private CircleCollider2D proj_collider;
    private GameBehaviour game;

    private float speed;
    private int team;
    private float damage;
    private float lifeTime;
    private float maxLifeTime = 3.5f;
    private Vector3 direction;
    private Vector3 extra_velocity = new Vector3( 0, 0, 0 );
    private int teleports = 0;
    private bool hitWater = false;

    void Start()
    {
        game = GameBehaviour.Instance;
        proj_body = GetComponent<Rigidbody2D>();
        proj_collider = GetComponent<CircleCollider2D>();

        setLifeTime(maxLifeTime);

        if (getTeam() != 1)
            setLifeTime(maxLifeTime * 2);

        proj_body.transform.right = getDirection();

        proj_body.AddForce(getDirection() * getSpeed() + extra_velocity, ForceMode2D.Impulse);
    }

    
    void Update()
    {
        if ( getLifeTime() <= Time.time || proj_body.transform.position.y > game.levelMaxs().y || proj_body.transform.position.y < game.levelMins().y)
        {
            var exp_pos = proj_body.transform.position;
            exp_pos.z = -2;

            GameObject explosionObj = Instantiate(hit_fx, exp_pos, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        if ( !hitWater && proj_body.transform.position.y < ( game.getWaterLevel() + 1 ) )
        {
            hitWater = true;

            var exp_pos = proj_body.transform.position;
            exp_pos.y = game.getWaterLevel();

            GameObject explosionObj = Instantiate(hitWater_fx, exp_pos, Quaternion.identity);
        }

    }

    private void FixedUpdate()
    {
        /*var update_pos = proj_body.transform.position;
        float old_x = proj_body.transform.position.x;
        update_pos.x = game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            onTeleported(old_x, update_pos.x);
        }

        proj_body.transform.position = update_pos;*/
    }

    private void LateUpdate()
    {
        var update_pos = proj_body.transform.position;
        float old_x = proj_body.transform.position.x;
        update_pos.x = game.calculateLoopingX(update_pos.x);

        float diff_x = old_x - update_pos.x;

        if (Mathf.Abs(diff_x) > 0)
        {
            onTeleported(old_x, update_pos.x);
        }

        proj_body.transform.position = update_pos;
    }

    private void onTeleported(float old_x, float new_x)
    {
        teleports += 1;

        if (teleports >= 2)
            Destroy(gameObject);

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if ( other_class == null )
        {
            Destroy(gameObject);
            return;
        }


        other_class.proceedDamage(getDamage());

        var exp_pos = proj_body.transform.position;
        exp_pos.z = -2;

        GameObject explosionObj = Instantiate(hit_fx, exp_pos, Quaternion.identity);

        Destroy(gameObject);
    }
    public void setupProjectile( Vector3 dir, float sp, float dmg, int tm, Vector3 extra_vel )
    {
        setDirection(dir);
        setSpeed(sp);
        setDamage(dmg);
        setTeam(tm);
        setExtraVelocity(extra_vel);
    }

    public void setLifeTime( float time )
    {
        lifeTime = Time.time + time;
    }

    public float getLifeTime()
    {
        return lifeTime;
    }

    public void setDamage( float dmg )
    {
        damage = dmg;
    }

    public float getDamage()
    {
        return damage;
    }

    public void setDirection( Vector3 dir )
    {
        direction = dir;
    }

    public Vector3 getDirection()
    {
        return direction;
    }

    public void setSpeed( float sp )
    {
        speed = sp;
    }

    public float getSpeed()
    {
        return speed;
    }

    public void setTeam( int tm )
    {
        team = tm;
    }
    public int getTeam()
    {
        return team;
    }

    public void setExtraVelocity( Vector3 extra )
    {
        extra_velocity = extra;
    }

}
