using System.Collections;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// ゴキブリの動きに関するスクリプト
/// </summary>
public class CockroachMoveController : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    [SerializeField] CockroachScriptableObject m_data = null;
    /// <summary>現在の移動速度</summary>
    float m_currentMoveSpeed = 0;
    /// <summary>現在のジャンプ力</summary>
    float m_currentJumpPower = 0;
    /// <summary>重力</summary>
    [SerializeField] float m_gravityPower = 10f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float m_maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform m_rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform m_rotateRayPos = null;
    // <summary>マウスの感度</summary>
    [SerializeField, Range(50f, 300f)] float m_mouseSensitivity = 50f;

    [SerializeField] Animator m_anim = null;
    #endregion


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

    bool m_isNPCmode = false;

    /// <summary>死んでいるかどうか</summary>
    public bool IsDed
    {
        get => m_isDed;

        set
        {
            m_isDed = value;
        }
    }

    public void StartSetPlayer()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_rb = GetComponent<Rigidbody>();
        m_currentMoveSpeed = m_data.Speed;
        m_currentJumpPower = m_data.JumpPower;
        m_anim = GetComponent<Animator>();
        m_rb.useGravity = false;
    }

    public void StartSetNpc()
    {
        m_isNPCmode = true;
        m_rb = GetComponent<Rigidbody>();
        m_currentMoveSpeed = m_data.Speed;
        m_currentJumpPower = m_data.JumpPower;
        m_anim = GetComponent<Animator>();
        m_rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (!m_isNPCmode &&PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (m_isDed) return;
        Gravity();
        //Move();
        FallForce();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isNPCmode && PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (m_isDed) return;
        Ray();
    }

    public void Move(float virtical)
    {
        m_dir = transform.forward;
        if (m_isJumping) return;

        if (virtical > 0) // 進む処理
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
            if (m_rb == null) 
            {
                // なぜか最初に NULL になる
                m_rb = GetComponent<Rigidbody>();
            }

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
            m_anim.SetFloat("Velocity", virtical);
        }
        else
        {
            m_anim.SetFloat("Velocity", 0);
        }
    }

    public void MouseMove(float horizontal, float x)
    {
        if (horizontal != 0)
        {
            m_mouseMoveX = (horizontal * m_mouseSensitivity) * Time.deltaTime;
        }
        else if (x != 0)
        {
            m_mouseMoveX = (x * m_mouseSensitivity) * Time.deltaTime;
        }
        else
        {
            m_mouseMoveX = 0;
        }

        transform.Rotate(new Vector3(0f, m_mouseMoveX, 0f));
    }

    void Gravity()
    {
        if (m_rb == null) m_rb = GetComponent<Rigidbody>();
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);
    }

    /// <summary>ジャンプする</summary>
    /// <param name="isJump">ジャンプするかどうか</param>
    public void Jump(bool isJump)
    {
        if (isJump && m_isGrounded)
        {
            m_isJumping = true;
            m_isGrounded = false;
            m_rb.AddForce(-m_gravityDir.normalized * m_currentJumpPower, ForceMode.Impulse);

            if (m_gravityDir == Vector3.down) return;
            m_jumpDir = -m_gravityDir;
            ChangeGravity(Vector3.down);
            StartCoroutine(ChangeRotate(Vector3.up, 0.1f));
        }
    }

    void FallForce()
    {
        if (m_isJumping && m_gravityDir != Vector3.down) { m_rb.AddForce(m_jumpDir); }
    }

    /// <summary>
    /// 子オブジェクトのTriggerから呼ぶ
    /// </summary>
    /// <param name="isGround"></param>
    /// <returns>接地判定</returns>
    public void IsGround(bool isGround)
    {
        if (isGround)
        {
            m_isGrounded = true;
        }
        else
        {
            m_isGrounded = false;
        }
    }

    void Ray()
    {
        Physics.Raycast(m_rayOriginPos.position, m_rotateRayPos.position - m_rayOriginPos.position, out m_rotateHit, m_maxRayDistance);
        Debug.DrawRay(m_rayOriginPos.position, (m_rotateRayPos.position - m_rayOriginPos.position).normalized * m_maxRayDistance, Color.red);

        if (!m_rotateHit.collider)
        {
            m_rotateHit.normal = -Vector3.down;
        }
    }

    void ChangeGravity(Vector3 nomal) => m_gravityDir = nomal;

    IEnumerator ChangeRotate(Vector3 nomal, float waitTime)
    {
        if (m_isRotate) yield return null;

        m_isRotate = true;

        if (!m_isJumping) // 壁に上ったりする時だけ使う
        {
            // https://teratail.com/questions/290578
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
            transform.DORotateQuaternion(toRotate, 0.25f).OnComplete(() =>
            {
                // 回転した時にできた隙間を強制的に埋める
                m_rb.AddForce(m_gravityDir * m_gravityPower * 10, ForceMode.Impulse);
            });
        }
        else
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
            transform.DORotateQuaternion(toRotate, 0.25f);
        }

        yield return new WaitForSeconds(waitTime); // 回転する時間を稼ぐ

        m_isRotate = false;
    }

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
            m_currentMoveSpeed = m_data.Speed;
            m_currentJumpPower = m_data.JumpPower;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_isJumping = false;
        m_jumpDir = Vector3.zero;

        foreach (ContactPoint point in collision.contacts)
        {
            if (m_gravityDir == Vector3.down && point.normal == Vector3.up) return;
            ChangeGravity(-point.normal);

            if (!this.gameObject.activeSelf) return;
            StartCoroutine(ChangeRotate(point.normal, 0.5f));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_isJumping || m_isRotate) return;

        if (!m_rotateHit.collider) { m_rotateHit.normal = -Vector3.down; }

        if (m_rotateHit.normal != -m_gravityDir)
        {
            ChangeGravity(-m_rotateHit.normal);
            StartCoroutine(ChangeRotate(m_rotateHit.normal, 0.5f));
        }
    }
}
