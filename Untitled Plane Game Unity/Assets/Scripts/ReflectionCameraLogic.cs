using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionCameraLogic : MonoBehaviour
{
    private GameBehaviour game;
    private Camera camera_obj;

    void Start()
    {
        game = GameBehaviour.Instance;
        camera_obj = GetComponent<Camera>();
    }

    
    void LateUpdate()
    {
        var update_pos = new Vector3(game.getCameraX(), game.getWaterLevel() + camera_obj.orthographicSize, camera_obj.transform.position.z);
        camera_obj.transform.position = update_pos;
    }
}
