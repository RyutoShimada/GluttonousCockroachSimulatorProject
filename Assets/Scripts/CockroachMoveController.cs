using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachMoveController : MoveBass
{
    [SerializeField] float m_jumpPower = 1f;
    bool m_isWall = false;

    /// <summary>Rigidbodyが無効かどうか</summary>
    Wall wall;

    void Start()
    {
        wall = new Wall();
    }

    void Update()
    {
        Move();
        base.Jump(m_jumpPower);
    }

    public override void Move()
    {
        if (m_isWall)
        {
            WallMove();
        }
        else
        {
            base.Move();
        }
    }

    void WallMove()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall" || other.tag == "Ceiling")
        {
            //壁または天井に接触した時
            m_isWall = true;
            Debug.Log(m_isWall);

            if (other.gameObject.name == "NorthWall")
            {
                wall = Wall.North;
            }
            else if (other.gameObject.name == "SouthWall")
            {
                wall = Wall.South;
            }
            else if (other.gameObject.name == "WestWall")
            {
                wall = Wall.West;
            }
            else if (other.gameObject.name == "EastWall")
            {
                wall = Wall.East;
            }

            switch (wall)
            {
                case Wall.North:
                    //Debug.Log("North");
                    //this.gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
                    this.gameObject.transform.forward = Vector3.up;
                    break;
                case Wall.South:
                    break;
                case Wall.West:
                    break;
                case Wall.East:
                    break;
                default:
                    break;
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "Wall" || other.tag == "Ceiling")
    //    {
    //        m_isWall = false;
    //        Debug.Log(m_isWall);
    //    }
    //}

    enum Wall
    {
        North,
        South,
        West,
        East
    }
}
