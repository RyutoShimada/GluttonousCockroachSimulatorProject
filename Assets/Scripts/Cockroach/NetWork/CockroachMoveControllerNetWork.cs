using System.Collections;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

/// <summary>
/// ゴキブリの動きに関するスクリプト
/// </summary>
public class CockroachMoveControllerNetWork : MonoBehaviourPunCallbacks, IIsCanMove
{
    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 7f;
    /// <summary>現在の移動速度</summary>
    [SerializeField] float m_currentMoveSpeed = 0;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float m_jumpPower = 4f;
    /// <summary>現在のジャンプ力</summary>
    [SerializeField] float m_currentJumpPower = 0;
    /// <summary>重力</summary>
    [SerializeField] float m_gravityPower = 10f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float m_maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform m_rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform m_rotateRayPos = null;
    /// <summary>マウスの感度</summary>
    [SerializeField] float m_mouseSensitivity = 5f;
    /// <summary>Rigidbody</summary>
    Rigidbody m_rb;
    /// <summary>Velocity</summary>
    Vector3 m_velo;
    /// <summary>Direction</summary>
    Vector3 m_dir;
    /// <summary>重力方向</summary>
    Vector3 m_gravityDir = Vector3.down;
    /// <summary>ジャンプする方向(床以外でのジャンプ時のみ)</summary>
    Vector3 m_jumpDir;
    /// <summary>回転する時に必要な法線ベクトルを取得するためのRay</summary>
    RaycastHit m_rotateHit;
    Animator m_anim;
    /// <summary>Vertical</summary>
    float m_v;
    /// <summary>マウスのX軸の動いた量</summary>
    float m_mouseMoveX;
    /// <summary>回転中かどうか</summary>
    bool m_isRotate = false;
    /// <summary>接地しているかどうか</summary>
    bool m_isGrounded = true;
    /// <summary>ジャンプ中かどうか</summary>
    bool m_isJumping = false;
    /// <summary>死んでいるかどうか</summary>
    bool m_isDed = false;

    [HideInInspector]
    public bool m_canMove = true;

    [SerializeField] bool m_debugMode = false;

    /// <summary>死んでいるかどうか</summary>
    public bool IsDed
    {
        get => m_isDed;

        set
        {
            m_isDed = value;
        }
    }

    void Start()
    {
        if (!photonView.IsMine) return;
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        m_rb = GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();
        m_currentMoveSpeed = m_moveSpeed;
        m_currentJumpPower = m_jumpPower;
        //EventSystem.Instance.Subscribe((EventSystem.CanMove)CanMove);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (!m_debugMode)
        {
            if (!m_canMove) return;
            if (m_isDed) return;
        }
        Gravity();
        Move();
        FallForce();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (!m_canMove) return;
        if (m_isDed) return;

        m_v = Input.GetAxisRaw("Vertical");
        Ray();
        Jump(m_currentJumpPower);
        MouseMove();
    }

    private void OnDestroy()
    {
        //EventSystem.Instance.Unsubscribe((EventSystem.CanMove)CanMove);
    }

    /// <summary>
    /// 移動
    /// </summary>
    void Move()
    {
        m_dir = transform.forward;

        if (m_isJumping) return;

        if (m_v > 0) // 進む処理
        {
            m_velo = m_dir.normalized * m_currentMoveSpeed;

            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度を維持
                m_velo.y = m_rb.velocity.y;
            }
            else if (m_gravityDir == Vector3.left || m_gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度を維持
                m_velo.x = m_rb.velocity.x;
            }
            else if (m_gravityDir == Vector3.forward || m_gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度を維持
                m_velo.z = m_rb.velocity.z;
            }
        }
        else // 止まる処理
        {
            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度以外を0
                m_velo = new Vector3(0f, m_rb.velocity.y, 0f);
            }
            else if (m_gravityDir == Vector3.left || m_gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度以外を0
                m_velo = new Vector3(m_rb.velocity.x, 0f, 0f);
            }
            else if (m_gravityDir == Vector3.forward || m_gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度以外を0
                m_velo = new Vector3(0f, 0f, m_rb.velocity.z);
            }
        }

        if (!m_isJumping && m_isRotate)
        {
            m_velo *= 0.5f;
        }

        m_rb.velocity = m_velo;

        if (m_isGrounded)
        {
            m_anim.SetFloat("Velocity", m_v);
        }
        else
        {
            m_anim.SetFloat("Velocity", 0);
        }
    }

    private void MouseMove()
    {
        m_mouseMoveX = Input.GetAxis("Mouse X") * m_mouseSensitivity;
        transform.Rotate(new Vector3(0f, m_mouseMoveX, 0f));
    }

    void Gravity()
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && m_isGrounded)
        {
            m_isJumping = true;

            m_isGrounded = false;

            m_rb.AddForce(-m_gravityDir.normalized * jumpPower, ForceMode.Impulse);

            if (m_gravityDir != Vector3.down)
            {
                m_jumpDir = -m_gravityDir;
                ChangeGravity(Vector3.down);
                StartCoroutine(ChangeRotate(Vector3.up, 0.1f));
            }
        }
    }

    void FallForce()
    {
        if (m_isJumping && m_gravityDir != Vector3.down)
        {
            m_rb.AddForce(m_jumpDir);
        }
    }

    /// <summary>
    /// 子オブジェクトのTriggerから呼ぶ
    /// </summary>
    /// <param name="isGround"></param>
    /// <returns></returns>
    public void IsGround(bool isGround)
    {
        //if (_isJump) return;

        if (isGround)
        {
            m_isGrounded = true;
        }
        else
        {
            m_isGrounded = false;
        }
    }

    /// <summary>
    /// 動けるかどうか（イベントから呼ばれる）
    /// </summary>
    /// <returns></returns>
    //void CanMove(bool isMove)
    //{
    //    m_canMove = isMove;
    //}

    void Ray()
    {
        Physics.Raycast(m_rayOriginPos.position, m_rotateRayPos.position - m_rayOriginPos.position, out m_rotateHit, m_maxRayDistance);
        Debug.DrawRay(m_rayOriginPos.position, (m_rotateRayPos.position - m_rayOriginPos.position).normalized * m_maxRayDistance, Color.green);
    }

    void ChangeGravity(Vector3 nomal)
    {
        m_gravityDir = nomal;
    }

    IEnumerator ChangeRotate(Vector3 nomal, float waitTime)
    {
        if (m_isRotate) yield return null;

        m_isRotate = true;

        if (!m_isJumping) // 壁に上ったりする時だけ使う
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f).OnComplete(() =>
            {
                // 回転した時にできた隙間を強制的に埋める
                m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Impulse);
            });
        }
        else
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f);
        }

        yield return new WaitForSeconds(waitTime); // 回転する時間を稼ぐ

        m_isRotate = false;
    }

    [PunRPC]
    /// <summary>
    /// cockroachから呼ばれる
    /// </summary>
    /// <param name="addSpeed">加算する速度</param>
    /// <param name="addJump">加算するジャンプ力</param>
    public void InvincibleMode(bool isMode, float addSpeed, float addJump)
    {
        if (isMode)
        {
            m_currentMoveSpeed += addSpeed;
            m_currentJumpPower += addJump;
        }
        else
        {
            m_currentMoveSpeed = m_moveSpeed;
            m_currentJumpPower = m_jumpPower;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_isJumping = false;

        m_jumpDir = Vector3.zero; // 着地したらジャンプする方向をリセット

        foreach (ContactPoint point in collision.contacts)
        {
            //Debug.Log(point.normal);
            if (m_gravityDir == Vector3.down && point.normal == Vector3.up)
            {
                return;
            }
            else
            {
                ChangeGravity(-point.normal);
                StartCoroutine(ChangeRotate(point.normal, 0.5f));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_isJumping || m_isRotate) return;

        if (m_rotateHit.collider)
        {
            if (m_rotateHit.normal != -m_gravityDir)
            {
                ChangeGravity(-m_rotateHit.normal);
                StartCoroutine(ChangeRotate(m_rotateHit.normal, 0.5f));
            }
        }
    }

    public void IsMove(bool isMove)
    {
        m_canMove = isMove;
    }
}
