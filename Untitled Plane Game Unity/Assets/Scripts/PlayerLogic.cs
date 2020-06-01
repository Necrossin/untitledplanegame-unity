using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour, IDamagable
{
    protected GameBehaviour game;

    [SerializeField]
    protected int projectileType;
    protected GameObject projectilePrefab;

    [SerializeField]
    protected GameObject explosion_fx;

    protected Rigidbody2D plane_body;
    protected PolygonCollider2D plane_collider;

    [SerializeField]
    private ParticleSystem muzzleflash_fx;
    [SerializeField]
    protected ParticleSystem damagesmoke_fx;
    [SerializeField]
    protected ParticleSystem jetmain_fx;
    [SerializeField]
    protected ParticleSystem repair_fx;
    [SerializeField]
    protected GameObject impact_fx;
    [SerializeField]
    protected ParticleSystem water_fx;

    [SerializeField]
    private List<AudioClip> soundList;

    [SerializeField]
    private AudioSource audioSourceParent;


    protected float curThrust = 0;
    private float maxThrust = 1;

    protected float health;
    [SerializeField]
    protected float maxHealth = 100;

    protected float rotInput = 0;
    protected float thrustInput = 0;

    public float rotSpeed = 320;

    public float maxSpeed = 40;
    public float acceleration = 10;

    private bool isRepairing = false;

    protected Vector2 vector_down = new Vector2(0, -1);
    protected Vector2 vector_up = new Vector2(0, 1);
    protected Vector3 vector_up3 = new Vector3(0, 0, 1);

    protected int team = 1;

    protected float fireDelay = 0.11f;

    protected float nextFire = 0;
    protected float nextRegen = 0;
    protected float nextEnvDamage = 0;

    protected float smokeThreshold = 1;

    protected float firePitch = 0;

    void Start()
    {
        game = GameBehaviour.Instance;
        plane_body = GetComponent<Rigidbody2D>();
        plane_collider = GetComponent<PolygonCollider2D>();
        projectilePrefab = game.getProjectileType(projectileType);

        setMaxHealth(maxHealth);
        setHealth(getMaxHealth());
    }


    void Update()
    {
        rotInput = Input.GetAxisRaw("Horizontal");
        thrustInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
            fireProjectile();

        handleEnvDamage( true );
        handleHealthRegen();
        handleEffects();
    }

    private void FixedUpdate()
    {
        handleMotion();
        handleLevelBounds();
    }

    protected virtual void handleEffects()
    {
        doSmokeEffect();
        doJetEffect();
        doRepairEffect();
        doWaterEffect();
    }

    protected void handleMotion()
    {
        if (rotInput != 0)
        {
            var new_rotation = plane_body.rotation - rotSpeed * GetRotPower() * Time.fixedDeltaTime * rotInput;
            new_rotation = normalizeAngle(new_rotation);
            plane_body.MoveRotation(new_rotation);
        }

        plane_body.transform.GetChild(0).transform.localEulerAngles = new Vector3(0, plane_body.rotation * -1, 0);

        if (IsAccelerating())
        {
            curThrust = Mathf.Min(curThrust + 2.3f * Time.fixedDeltaTime, maxThrust);
            plane_body.gravityScale = 0;
            plane_body.AddForce(getAimDir() * acceleration * curThrust, ForceMode2D.Impulse);
        }
        else
        {
            curThrust = 0;
            plane_body.gravityScale = 3f;
        }


        if (plane_body.transform.position.y > game.getSkyLevel())
            plane_body.AddForce(vector_down * (1 - (game.levelMaxs().y - plane_body.transform.position.y) / game.getSkyLevelHeight()) * 120, ForceMode2D.Force);

        if (plane_body.transform.position.y < game.getWaterLevel())
        {
            float force_delta = 1 - (game.levelMins().y - plane_body.transform.position.y) / game.getWaterLevelHeight();
            force_delta = Mathf.Pow(force_delta, 3f);
            plane_body.AddForce(vector_up * force_delta * 30, ForceMode2D.Force);
        }

        var clamped_vel = plane_body.velocity.magnitude;

        clamped_vel = Mathf.Clamp(clamped_vel, 0, maxSpeed);

        plane_body.velocity = getVelDir() * clamped_vel;
    }

    private void handleLevelBounds()
    {
        var update_pos = plane_body.transform.position;

        if (plane_body.transform.position.x <= game.levelMins().x)
            update_pos.x = game.levelMaxs().x;
        if (plane_body.transform.position.x >= game.levelMaxs().x)
            update_pos.x = game.levelMins().x;

        if (plane_body.transform.position.y < game.levelMins().y)
            update_pos.y = game.levelMins().y;
        if (plane_body.transform.position.y > game.levelMaxs().y)
            update_pos.y = game.levelMaxs().y;

        plane_body.transform.position = update_pos;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if ( other_class != null && nextEnvDamage <= Time.time )
        {
            game.doScreenshake(other.attachedRigidbody.transform.position, 1.5f, 0.6f);
            other_class.proceedDamage( 25 );
            proceedDamage(15);
            nextEnvDamage = Time.time + 0.1f;
        }
       
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        var other_class = other.attachedRigidbody.GetComponentInParent<IDamagable>();
        if (other_class != null && nextEnvDamage <= Time.time)
        {
            game.doScreenshake(other.attachedRigidbody.transform.position, 1.5f, 0.6f);
            other_class.proceedDamage(13);
            proceedDamage(7);
            nextEnvDamage = Time.time + 0.05f;

            var exp_pos = plane_body.transform.position;
            exp_pos.z = -2;

            Instantiate(impact_fx, exp_pos, Quaternion.identity);

        }

    }


    private void handleHealthRegen()
    {
        if (getHealth() < getMaxHealth())
        {
            if (nextRegen < Time.time)
            {
                setHealth(Mathf.Clamp(getHealth() + 1, 0, getMaxHealth()));
                nextRegen = 0.016f + Time.time;
                playSound(3, 0.1f);
                isRepairing = true;
            }
        }
        else
        {
            if (isRepairing)
            {
                playSound(4, 0.2f);
                isRepairing = false;
            }
        }    
            
    }

    protected void handleEnvDamage( bool noSnd = false )
    {
        if (nextEnvDamage < Time.time && plane_body.transform.position.y < game.getWaterLevel())
        {
            var exp_pos = plane_body.transform.position;
            exp_pos.z = -2;

            Instantiate(impact_fx, exp_pos, Quaternion.identity);
            nextEnvDamage = 0.03f + Time.time;
            proceedDamage(1, noSnd);
        }
    }

    protected virtual float GetRotPower()
    {
        if (IsAccelerating())
            return 0.3f;
        else
            return 1;
    }

    public bool IsAccelerating()
    {
        return thrustInput > 0;
    }

    public Vector2 getVelDir()
    {
        return plane_body.velocity.normalized;
    }

    public Vector2 getAimDir()
    {
        return plane_body.transform.up;
    }

    // current speed / max speed
    public float getVelDelta()
    {
        return plane_body.velocity.sqrMagnitude / (maxSpeed * maxSpeed);
    }

    public float getThrustDelta()
    {
        return curThrust;
    }

    public void setTeam( int t = 1 )
    {
        team = t;
    }

    public int getTeam()
    {
        return team;
    }

    public void setHealth( float am )
    {
        health = am;
    }

    public float getHealth()
    {
        return health;
    }

    public void setMaxHealth(float am)
    {
        maxHealth = am;
    }

    public float getMaxHealth()
    {
        return maxHealth;
    }

    protected float normalizeAngle(float ang)
    {
        ang = (ang + 180) % 360;
        if (ang < 0)
            ang += 360;
        return ang - 180;
    }


    protected virtual void fireProjectile()
    {
        if (game.isGamePaused())
            return;

        if ( nextFire < Time.time )
        {
            var spawn_pos = plane_body.transform.position + plane_body.transform.up * 2.5f;

            var dir = plane_body.transform.up;
            dir = Quaternion.Euler(0, 0, Random.Range( -4, 4 ) ) * dir;

            game.createProjectile(projectilePrefab, spawn_pos, dir, maxSpeed * 2, 25, getTeam(), plane_body.velocity);

            doMuzzleflash();

            playSound(0, 0.5f);

            nextFire = fireDelay + Time.time;
            nextRegen = 0.2f + Time.time;
            isRepairing = false;
        }
    }

    protected virtual void doMuzzleflash()
    {
        muzzleflash_fx.Emit(1);
    }

    protected virtual void doSmokeEffect()
    {

        float hp_delta = Mathf.Clamp(getHealth() / getMaxHealth(), 0, 1);
        float interval = 0.08f - 0.07f * (1 - hp_delta);
        int new_min, new_max;

        var new_burst = damagesmoke_fx.emission.GetBurst(0);

        new_min = 0 + Mathf.RoundToInt( 2 * (1 - hp_delta) );
        new_max = 1 + Mathf.RoundToInt( 3 * (1 - hp_delta) );


        new_burst.repeatInterval = interval;
        new_burst.minCount = (short) new_min;
        new_burst.maxCount = (short) new_max;

        damagesmoke_fx.emission.SetBurst(0, new_burst);

        if ( getHealth() < getMaxHealth() * smokeThreshold)
        {
            if (!damagesmoke_fx.isEmitting)
                damagesmoke_fx.Play();
        }
        else
        {
            if (damagesmoke_fx.isEmitting)
                damagesmoke_fx.Stop();
        }
    }

    protected virtual void doJetEffect()
    {
        if (IsAccelerating())
        {
            if (!jetmain_fx.isEmitting)
            {
                jetmain_fx.Play(true);
                playSound(5, 0.1f);
            }
                
        }
        else
        {
            if (jetmain_fx.isEmitting)
                jetmain_fx.Stop();
        }
    }

    protected virtual void doRepairEffect()
    {
        if (isRepairing)
        {
            if (!repair_fx.isEmitting)
                repair_fx.Play();
        }
        else
        {
            if (repair_fx.isEmitting)
                repair_fx.Stop();
        }
    }

    protected virtual void doWaterEffect()
    {
        float pow_dist = 15;
        float power = Mathf.Clamp01(1 - (plane_body.transform.position.y - game.getWaterLevel()) / pow_dist);
        
        if (plane_body.transform.position.y < ( game.getWaterLevel() + pow_dist) )
        {
            if (!water_fx.isEmitting)
                water_fx.Play(true);

            var waterLogic = water_fx.GetComponent<WaterSplashEngineLogic>();
            waterLogic.setPower(1 + power * ( 5 + ( IsAccelerating() ? 5 : 0 ) ));
            waterLogic.setDirection(plane_body.transform.up);
        }
        else
        {
            if (water_fx.isEmitting)
                water_fx.Stop();
        }

    }

    public void proceedDamage( float dmg, bool noSnd = false )
    {
        nextRegen = 0.5f + Time.time;
        isRepairing = false;
        setHealth(Mathf.Clamp(getHealth() - dmg, 0, getMaxHealth()));

        if (getHealth() <= 0)
            doDestroy();
        else
        {
            if (!noSnd)
                playSound(1, 0.5f);
        }
           
    }

    protected virtual void doDestroy()
    {
        game.doScreenshake(plane_body.transform.position, 9, 2);
        Instantiate(explosion_fx, plane_body.transform.position, Quaternion.identity);
        game.setPlayerDead(true);
        gameObject.SetActive(false);
    }

    public void playSound(int index, float volume = 1)
    {
        if (game.isGamePaused())
            return;

        if (soundList.Count < ( index - 1 ) )
        {
            return;
        }
        
        if (audioSourceParent == null || soundList[index] == null)
        {
            return;
        }

        if (firePitch == 0)
            firePitch = audioSourceParent.pitch * 1;

        if ( index == 0 )
        {
            audioSourceParent.pitch = firePitch + Random.Range(-0.15f, 0.15f);
            audioSourceParent.volume = volume;
            audioSourceParent.Play();
        }
        else
        {
            audioSourceParent.pitch = firePitch * 1;
            audioSourceParent.volume = 1;
            audioSourceParent.PlayOneShot(soundList[index], volume);
        }
    }

    public void playSoundAtPoint(int index, Vector3 pos, float volume = 1)
    {
        if (soundList[index] == null)
        {
            return;
        }
        AudioSource.PlayClipAtPoint(soundList[index], pos, volume);
    }



}
