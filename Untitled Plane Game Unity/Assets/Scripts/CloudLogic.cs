using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudLogic : MonoBehaviour
{
    private GameBehaviour game;
    [SerializeField]
    private GameObject cloud;
    private float speed;
    private bool back_layer = false;
    float save_y = 9999;

    void Start()
    {
        game = GameBehaviour.Instance;
        speed = Random.Range(-3f, 3f);
    }

    
    void LateUpdate()
    {
        
        var new_pos = new Vector3( game.calculateLoopingX( cloud.transform.position.x + speed * Time.deltaTime, true ), cloud.transform.position.y, cloud.transform.position.z);

        if ( back_layer )
        {
            if (save_y == 9999)
                save_y = new_pos.y * 1;

            float cam_y = Mathf.Max(game.getCameraY(), game.getWaterLevel() + 50);
            float cam_delta = Mathf.Clamp( cam_y / game.levelMaxs().y, -1, 1);
            new_pos.y = save_y + cam_delta * 30;
        }

        cloud.transform.position = new_pos;  
    }

    public void setBackLayer( bool back )
    {
        back_layer = back;
    }

}
