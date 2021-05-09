using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CockroachMoveController : MonoBehaviour
{
    [SerializeField] float m_speed = 5f;
    [SerializeField] float m_gravityPower = 1f;
    [SerializeField] float m_jumpPower = 1f;
    Rigidbody m_rb;
    Vector3 m_gravityDir;
    Vector3 m_velocity;
    Vector3 m_direction;
    /// <summary>重力方向を管理</summary>
    GravityDirection m_gravityDirection;
    /// <summary>Rayを飛ばす距離</summary>
    const float m_rayDis = 0.19f;
    /// <summary>家具の位置</summary>
    Vector3 m_FurniturePos;

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
        GetPorigon();
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
        ChangeGravity(h, v);
        //落下速度を早くする
        if (!IsGround())
        {
            m_rb.AddForce(Vector3.down * m_jumpPower, ForceMode.Force);
        }
    }

    void GetPorigon()
    {
        //RaycastHit hit;
        //if (Physics.Raycast(transform.Find("RayOriginPos").transform.position, transform.forward, out hit, 1f))
        //{
        //    //https://docs.unity3d.com/2019.3/Documentation/Manual/ComputingNormalPerpendicularVector.html
        //    //https://unitylab.wiki.fc2.com/wiki/%E3%83%A1%E3%83%83%E3%82%B7%E3%83%A5%E3%82%B3%E3%83%B3%E3%83%9D%E3%83%BC%E3%83%8D%E3%83%B3%E3%83%88%E3%81%AE%E9%A0%82%E7%82%B9%E4%BD%8D%E7%BD%AE%EF%BC%88Vector3%E9%85%8D%E5%88%97%EF%BC%89%E3%82%92%E5%BE%97%E3%82%8B
        //    //Debug.Log("nom" + hit.point.normalized);
        //    //Vector3 a;
        //    //Vector3 b;
        //    //Vector3 c;
        //    //Vector3[] mf = hit.collider.gameObject.GetComponent<MeshFilter>().mesh.normals;
        //    //a = mf[0];
        //    //b = mf[1];
        //    //c = mf[2];
        //    //Vector3 side1 = b - a;
        //    //Vector3 side2 = c - a;
        //    //Vector3 prep = Vector3.Cross(side1, side2);
        //    //Debug.Log(prep);
        //}
        //Debug.DrawRay(transform.Find("RayOriginPos").transform.position, transform.forward, Color.red, 1f);
    }

    /// <summary>
    /// 重力方向変更後の処理
    /// </summary>
    /// <param name="h">Horizontal</param>
    /// <param name="v">Vertical</param>
    void ChangeGravity(float h, float v)
    {
        switch (m_gravityDirection)
        {
            case GravityDirection.Floor:

                m_direction = new Vector3(h, 0, v);//方向を変換
                float fallSpeed;//速度保存用

                if (h != 0 || v != 0) //入力されている時
                {
                    transform.forward = m_direction; //向きを保存
                    m_velocity = new Vector3(h, 0, v);//方向を入力
                    fallSpeed = m_rb.velocity.y;//Y軸速度を保存
                    m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
                    m_rb.velocity = new Vector3(m_rb.velocity.x, fallSpeed, m_rb.velocity.z);//落下速度を維持
                }
                else
                {
                    m_velocity = new Vector3(0, m_rb.velocity.y, 0);//ピタッと止まるようにする
                    m_rb.velocity = m_velocity;
                }
                break;

            case GravityDirection.Ceiling:

                ChangeDirection(h, v, new Vector3(-h, m_rb.velocity.y, -v), Vector3.up, Vector3.down, new Vector3(-h, 0, -v));
                break;

            case GravityDirection.NorthWall:

                ChangeDirection(h, v, new Vector3(h, v, m_rb.velocity.z), Vector3.forward, Vector3.back, new Vector3(h, v, 0));
                break;

            case GravityDirection.SouthWall:

                ChangeDirection(h, v, new Vector3(h, -v, m_rb.velocity.z), Vector3.back, Vector3.forward, new Vector3(h, -v, 0));
                break;

            case GravityDirection.WestWall:

                ChangeDirection(h, v, new Vector3(m_rb.velocity.x, -h, v), Vector3.left, Vector3.right, new Vector3(0, -h, v));
                break;

            case GravityDirection.EastWall:

                ChangeDirection(h, v, new Vector3(m_rb.velocity.x, h, v), Vector3.right, Vector3.left, new Vector3(0, h, v));
                break;

            case GravityDirection.Fall:

                m_gravityDir = Vector3.down;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 方向変換
    /// </summary>
    /// <param name="h">Horizontal</param>
    /// <param name="v">Vertical</param>
    /// <param name="velocity">変換した速度ベクトル</param>
    /// <param name="stop">止まった時の速度ベクトル</param>
    /// <param name="gravity">重力方向</param>
    /// <param name="transformUp">オブジェクトの上方向</param>
    /// <param name="dir">オブジェクトの正面</param>
    void ChangeDirection(float h, float v, Vector3 velocity, Vector3 gravity, Vector3 transformUp, Vector3 dir)
    {
        Quaternion look;
        m_gravityDir = gravity;//重力方向を変更
        m_direction = dir;//方向を変換
        m_velocity = velocity;//方向を入力

        if (h != 0 || v != 0)//入力されている時
        {
            look = Quaternion.LookRotation(m_direction, transformUp);
            this.transform.localRotation = look;
            m_rb.velocity = m_velocity.normalized * m_speed;//ベクトルを正規化してスピードをかける
        }
        else
        {
            m_rb.velocity = m_velocity;//ピタッと止まるようにする
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
        if (Physics.Raycast(this.transform.position, m_gravityDir, m_rayDis))
        {
            return true;
        }
        else
        {
            m_gravityDirection = GravityDirection.Fall;
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
        EastWall,
        Fall
    }
}
