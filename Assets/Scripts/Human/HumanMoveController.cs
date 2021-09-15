using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class HumanMoveController : MonoBehaviourPunCallbacks, IIsCanMove
{
    [HideInInspector] public bool m_canMove = true;

    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 1f;
    /// <summary>体を回転する速度</summary>
    [SerializeField] float m_turnSpeed = 1f;

    Vector2 m_input;
    Vector3 m_direction;
    Vector3 m_velocity;
    Rigidbody m_rb;
    Animator m_anim;

    void Start()
    {
        m_anim = GetComponent<Animator>();

        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_rb = GetComponent<Rigidbody>();
        MenuController.IsMove += IsMove;
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;

        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");

        DoRotate();
    }

    void LateUpdate()
    {
        DoAnimation();
    }

    public void IsMove(bool isMove) => m_canMove = isMove;

    void Move()
    {
        m_direction = Vector3.forward * m_input.y + Vector3.right * m_input.x;

        if (m_input != Vector2.zero)
        {
            //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
            m_direction = Camera.main.transform.TransformDirection(m_direction);
            m_direction.y = 0;

            m_velocity = m_direction.normalized * m_moveSpeed;
            m_velocity.y = m_rb.velocity.y;
            m_rb.velocity = m_velocity;
        }
        else
        {
            m_rb.velocity = Vector3.zero;
        }
    }

    void DoRotate()
    {
        //Playerの向きをカメラの向いている方向にする
        this.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, this.transform.rotation, m_turnSpeed * Time.deltaTime);
        //Playerが倒れないようにする
        this.transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
    }

    void DoAnimation()
    {
        // とりあえずのアニメーション
        if (m_input.x != 0)
        {
            m_anim.SetFloat("Speed", Mathf.Abs(m_input.x));
        }
        else
        {
            m_anim.SetFloat("Speed", Mathf.Abs(m_input.y));
        }

    }
}