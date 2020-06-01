using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterLogic : MonoBehaviour
{
   
    [SerializeField]
    private GameBehaviour game;
    [SerializeField]
    private GameObject water_obj;


    void Start()
    {
        
    }

    
    void LateUpdate()
    {
        var update_pos = new Vector3(game.getCameraX(), game.getWaterLevel() - water_obj.transform.localScale.y / 2, -3);
        water_obj.transform.position = update_pos;
    }
}
