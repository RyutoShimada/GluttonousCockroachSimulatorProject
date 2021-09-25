using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] Cockroach player;

    void Update()
    {
        var pos = player.transform.position;
        pos.y = transform.position.y;
        transform.position = pos;

        //var rot = player.transform.rotation;
        //rot.y = transform.rotation.y;
        //transform.rotation = rot;
    }
}
