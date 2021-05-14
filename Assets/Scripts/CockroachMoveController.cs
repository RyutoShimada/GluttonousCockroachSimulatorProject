using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CockroachMoveController : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    [SerializeField] float m_gravityPower = 1f;
    [SerializeField] float m_jumpPower = 1f;
    //[SerializeField] float m_turnSpeed = 1f;
    [SerializeField] CinemachineVirtualCameraBase m_vcam = null;
    Rigidbody m_rb;
    Vector3 m_gravityDir;
    Vector3 m_velocity;
    Vector3 m_direction;
    /// <summary>Rayを飛ばす距離</summary>
    const float m_rayDis = 0.19f;

    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        m_rb = this.gameObject.GetComponent<Rigidbody>();
        m_gravityDir = Vector3.down;
    }

    void Update()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        IsGround();
        Jump(m_jumpPower);
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <param name="h">Horizontal</param>
    /// <param name="v">Vertical</param>
    void Move(float h, float v)
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);//重力

        ChangeDirection(h, v, m_gravityDir, ref m_direction, ref m_rb, ref m_velocity);//方向を変換

        //落下速度を早くする
        if (!IsGround())
        {
            m_rb.AddForce(Vector3.down * m_jumpPower, ForceMode.Force);
        }

        transform.forward = Camera.main.transform.forward;
    }


    void ChangeDirection(float h, float v, Vector3 gravity, ref Vector3 direction, ref Rigidbody rb, ref Vector3 velo)
    {
        if (gravity.x != 0)
        {
            direction = new Vector3(0, v, h);

            if (h != 0 || v != 0) //入力されている時
            {
                //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
                direction = Camera.main.transform.TransformDirection(direction);
                direction.x = 0;//y軸方向はゼロにして水平方向のベクトルにする

                velo = direction.normalized * m_speed;
                velo.x = rb.velocity.x;
                rb.velocity = velo;
            }
            else
            {
                velo = new Vector3(rb.velocity.x, 0, 0);//ピタッと止まるようにする
                rb.velocity = velo;
            }
        }
        else if (gravity.y != 0)
        {
            direction = new Vector3(h, 0, v);

            if (h != 0 || v != 0) //入力されている時
            {
                //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
                direction = Camera.main.transform.TransformDirection(direction);
                direction.y = 0;//y軸方向はゼロにして水平方向のベクトルにする

                velo = direction.normalized * m_speed;
                velo.y = rb.velocity.y;
                rb.velocity = velo;
            }
            else
            {
                velo = new Vector3(0, rb.velocity.y, 0);//ピタッと止まるようにする
                rb.velocity = velo;
            }
        }
        else if (gravity.z != 0)
        {
            direction = new Vector3(h, v, 0);

            if (h != 0 || v != 0) //入力されている時
            {
                //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
                direction = Camera.main.transform.TransformDirection(direction);
                direction.z = 0;//y軸方向はゼロにして水平方向のベクトルにする

                velo = direction.normalized * m_speed;
                velo.z = rb.velocity.z;
                rb.velocity = velo;
            }
            else
            {
                velo = new Vector3(0, 0, rb.velocity.z);//ピタッと止まるようにする
                rb.velocity = velo;
            }
        }
        //Debug.Log(m_gravityDir);
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && IsGround())
        {
            m_rb.AddForce(-m_gravityDir * jumpPower, ForceMode.Impulse);
        }
    }

    /// <summary>接地判定</summary>
    /// <returns>判定結果</returns>
    bool IsGround()
    {
        if (Physics.Raycast(this.transform.position, m_gravityDir, m_rayDis))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        RaycastHit hit;
        Vector3 dir = collision.transform.position - transform.position;//Rayを飛ばす方向
        //自分のColliderにRayが当たらないようにInspctorのLayerをlgnore Raycastに設定しておくこと
        if (Physics.Raycast(transform.position, dir, out hit, 10f))
        {
            m_gravityDir = -hit.normal;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        RaycastHit hit;
        Vector3 dir = collision.transform.position - transform.position;//Rayを飛ばす方向
        //自分のColliderにRayが当たらないようにInspctorのLayerをlgnore Raycastに設定しておくこと
        if (Physics.Raycast(transform.position, dir, out hit, 10f))
        {
            m_gravityDir = -hit.normal;
        }
    }
}
