using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{

    [SerializeField]
    private GameObject player;
    private Rigidbody2D player_body;
    private PlayerLogic player_logic;
    private Camera camera_obj;

    private GameBehaviour game;

    private Vector2 offsetPosition = new Vector2(0, 0);
    private Vector3 screenshake_pos = new Vector3(0, 0, 0);

    private float screenshake_amount = 0;
    private float screenshake_duration = 1;
    private float screenshake_time = 0;

    void Start()
    {
        game = GameBehaviour.Instance;
        camera_obj = GetComponent<Camera>();
        player_body = player.GetComponent<Rigidbody2D>();
        player_logic = player.GetComponent<PlayerLogic>();
    }


    void Update()
    {

        var goal_direction = player_logic.getVelDir();

        if (player_logic.IsAccelerating())
            goal_direction = player_logic.getAimDir();

        approachPosition(player_body.transform.position, goal_direction, player_logic.getThrustDelta() * 10, player_logic.getAimDir(), 10);
        applyScreenshake();
    }

    private void approachPosition(Vector2 pos, Vector2 offset, float offsetSize, Vector2 constantOffset, float constantOffsetSize)
    {
        var goal = new Vector2(offset.x * offsetSize + constantOffset.x * constantOffsetSize, offset.y * offsetSize + constantOffset.y * constantOffsetSize);

        offsetPosition = Vector2.Lerp(offsetPosition, goal, 3f * Time.deltaTime);

        camera_obj.transform.position = new Vector3(pos.x + offsetPosition.x, Mathf.Clamp(pos.y + offsetPosition.y, game.levelMins().y + camera_obj.orthographicSize, game.levelMaxs().y - camera_obj.orthographicSize), camera_obj.transform.position.z);
    }

    public void doScreenshake(Vector3 pos, float am, float dur)
    {
        var cam_pos = camera_obj.transform.position;
        cam_pos.z = 0;

        pos.z = 0;
        
        // make sure that bigger screenshake has priority over the small ones
        if ( screenshake_amount <= am )
        {
            screenshake_pos = pos;
            screenshake_amount = am;
            screenshake_time = Time.time + dur;
        }
    }

    private void applyScreenshake()
    {

        if (screenshake_amount != 0 && screenshake_time < Time.time)
        {
            screenshake_amount = 0;
        }
        else
        {
            float delta = Mathf.Clamp( ( screenshake_time - Time.time ) / screenshake_duration, 0, 1 );

            var cam_pos = camera_obj.transform.position;
            cam_pos.z = 0;

            var shake_dir = (screenshake_pos - cam_pos).normalized;

            var add_shake = Vector3RandDir(screenshake_amount / 2 * delta, screenshake_amount * 1.1f * delta, shake_dir);
            add_shake += Vector3Rand(-screenshake_amount / 3 * delta, screenshake_amount / 3 * delta);
            add_shake.z = 0;

            camera_obj.transform.position += add_shake * Time.deltaTime * 143;
        }

    }

    private Vector3 Vector3RandDir(float min, float max, Vector3 dir)
    {
        return new Vector3(Random.Range(min, max) * dir.x, Random.Range(min, max) * dir.y, Random.Range(min, max) * dir.z);
    }

    private Vector3 Vector3Rand( float min, float max )
    {
        return new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max));
    }
}
