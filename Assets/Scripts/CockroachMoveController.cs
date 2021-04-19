using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachMoveController : MoveBass
{
    [SerializeField] float m_jumpPower = 1f;

    void Update()
    {
        Move();
        base.Jump(m_jumpPower);
    }

    public override void Move()
    {
        //壁や天井を這う処理を書く
        base.Move();
    }
}
