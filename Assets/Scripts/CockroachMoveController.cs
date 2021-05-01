using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CockroachMoveController : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    [SerializeField] float m_gravityPower = 1f;
    [SerializeField] float m_jumpPower = 1f;
    Rigidbody m_rb;
    Vector3 m_gravityDir;
    Vector3 m_velocity;
    Vector3 m_direction;
    /// <summary>落下速度</summary>
    float m_velocityY;
    /// <summary>重力方向を管理</summary>
    GravityDirection m_gravityDirection;

    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        m_rb = this.gameObject.GetComponent<Rigidbody>();
        m_gravityDirection = new GravityDirection();
        m_gravityDirection = GravityDirection.Floor;
        m_gravityDir = Vector3.down;
    }

    void Update()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Jump(m_jumpPower);
    }

    void Move(float h, float v)
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);//重力

        ChangeVector(h, v);

        //落下速度を早くする
        if (IsFall(m_jumpPower))
        {
            m_rb.AddForce(m_gravityDir * m_jumpPower, ForceMode.Force);
        }
    }

    void ChangeVector(float h, float v)
    {
        switch (m_gravityDirection)
        {
            case GravityDirection.Floor:
                m_gravityDir = Vector3.down;//重力方向を変更
                m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            case GravityDirection.Ceiling:
                m_gravityDir = Vector3.up;//重力方向を変更
                //m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    //forwardに方向を代入すると、オブジェクトが逆さまにならないので直接向きを変えている
                    if (h == 1)//右
                    {
                        transform.rotation = Quaternion.Euler(0, 90, 180);
                    }
                    if (h == -1)//左
                    {
                        transform.rotation = Quaternion.Euler(0, -90, 180);
                    }
                    if (v == 1)//上
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 180);
                    }
                    if (v == -1)//下
                    {
                        transform.rotation = Quaternion.Euler(0, 180, 180);
                    }

                    //transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            case GravityDirection.NorthWall:
                m_gravityDir = Vector3.forward;//重力方向を変更
                m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            case GravityDirection.SouthWall:
                m_gravityDir = Vector3.back;//重力方向を変更
                m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            case GravityDirection.WestWall:
                m_gravityDir = Vector3.left;//重力方向を変更
                m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            case GravityDirection.EastWall:
                m_gravityDir = Vector3.right;//重力方向を変更
                m_direction = new Vector3(h, 0, v);//方向を変換

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    m_velocityY = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, m_velocityY, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);
                    m_rb.velocity = m_velocity; //ピタッと止まるようにする
                }
                break;

            default:
                break;
        }
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
        if (Physics.Raycast(this.transform.position, m_gravityDir, 0.1f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>落下判定</summary>
    /// <returns>判定結果</returns>
    bool IsFall(float jumpPower)
    {
        if (m_rb.velocity.y <= jumpPower)//最大値に達したら落ちはじめる
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 重力方向を判定する
    /// </summary>
    /// <param name="tag">タグ</param>
    /// <param name="name">名前</param>
    void SwapGravityDirection(string tag, string name)
    {
        if (tag == "Wall")
        {
            if (name == "NorthWall")
            {
                m_gravityDirection = GravityDirection.NorthWall;
            }
            else if (name == "SouthWall")
            {
                m_gravityDirection = GravityDirection.SouthWall;
            }
            else if (name == "WestWall")
            {
                m_gravityDirection = GravityDirection.WestWall;
            }
            else if (name == "EastWall")
            {
                m_gravityDirection = GravityDirection.EastWall;
            }
        }
        else if (tag == "Floor")
        {
            m_gravityDirection = GravityDirection.Floor;
        }
        else if (tag == "Ceiling")
        {
            m_gravityDirection = GravityDirection.Ceiling;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        SwapGravityDirection(collision.gameObject.tag, collision.gameObject.name);
    }

    private void OnCollisionStay(Collision collision)
    {
        SwapGravityDirection(collision.gameObject.tag, collision.gameObject.name);
    }

    enum GravityDirection
    {
        Floor,
        Ceiling,
        NorthWall,
        SouthWall,
        WestWall,
        EastWall
    }
}
