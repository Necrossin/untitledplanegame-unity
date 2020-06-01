using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBackroundLogic : MonoBehaviour
{

    private GameBehaviour game;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Camera camera_obj;
    private PlayerLogic player_logic;

    [SerializeField]
    private SpriteRenderer sprite_renderer;
    [SerializeField]
    private SpriteRenderer sprite_renderer_circle;

    private float max_scale = 40;
    

    void Start()
    {
        player_logic = player.GetComponent<PlayerLogic>();
        game = GameBehaviour.Instance;
    }

    
    void LateUpdate()
    {
        if (player.activeSelf)
        {

            // update position
            var cam_pos = camera_obj.transform.position;
            cam_pos.z = 8;
            gameObject.transform.position = cam_pos;

            var player_pos = player.transform.position;
            player_pos.z = 7;
            sprite_renderer_circle.transform.position = player_pos;

            float health_mul = Mathf.Clamp(player_logic.getHealth() / player_logic.getMaxHealth(), 0, 1);
            float thresold = 0.98f;

            var new_col_bg = new Color(1, 1, 1, 0);
            var new_col_circle = sprite_renderer_circle.color;
            float circle_scale = 1;


            if (health_mul < thresold)
            {
                new_col_bg.a = ((1 - health_mul) * 230 + 10) / 255;
                new_col_circle.a = 1;
                circle_scale = health_mul * max_scale;
            }
            else
                new_col_circle.a = 0;


            sprite_renderer.color = new_col_bg;
            sprite_renderer_circle.color = new_col_circle;
            sprite_renderer_circle.transform.localScale = new Vector3(circle_scale, circle_scale, 1);
        }
        else
            if (game.isGameActive())
                gameObject.SetActive(false);

    }
}
