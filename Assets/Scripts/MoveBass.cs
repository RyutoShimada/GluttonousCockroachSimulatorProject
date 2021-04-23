using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Rigidbody必須</summary>
[RequireComponent(typeof(Rigidbody))]

/// <summary>動くものはこのクラスを継承する</summary>
public class MoveBass : MonoBehaviour
{
    /// <summary>速度</summary>
    [SerializeField] float m_speed = 5f;
    Rigidbody m_rb;
    Vector3 m_velocity;
    Vector3 m_direction;
    Vector2 m_inputDirection;

    private void Awake()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    /// <summary>移動の処理</summary>
    public virtual void Move()
    {
        m_inputDirection.x = Input.GetAxisRaw("Horizontal");
        m_inputDirection.y = Input.GetAxisRaw("Vertical");
        m_direction = new Vector3(m_inputDirection.x, 0, m_inputDirection.y);//方向を変換

        if (m_inputDirection != Vector2.zero) //入力されている時
        {
            transform.forward = m_direction; //向きを保存
            if (m_inputDirection == Vector2.one || m_inputDirection == new Vector2(-1, -1) || m_inputDirection == new Vector2(1, -1) || m_inputDirection == new Vector2(-1, 1))
            {
                m_velocity = new Vector3(m_inputDirection.x * (m_speed * 0.7f), m_rb.velocity.y, m_inputDirection.y * (m_speed * 0.7f));
            }
            else
            {
                m_velocity = new Vector3(m_inputDirection.x * m_speed, m_rb.velocity.y, m_inputDirection.y * m_speed);
            }
            m_rb.velocity = m_velocity;
        }
        else
        {
            m_velocity.x = 0;
            m_velocity.y = m_rb.velocity.y;
            m_velocity.z = 0;
            m_rb.velocity = m_velocity; //ピタッと止まるようにする
        }
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    public void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && IsGround())
        {
            m_rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        if (IsFall(jumpPower))
        {
            m_rb.AddForce(Vector3.down * jumpPower, ForceMode.Force);
        }
    }

    /// <summary>接地判定</summary>
    /// <returns>判定結果</returns>
    bool IsGround()
    {
        if (Physics.Raycast(this.transform.position, Vector3.down, 0.1f))
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
        if (m_rb.velocity.y < jumpPower)//最大値に達したら落ちはじめる
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
