using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CockroachMoveController))]

/// <summary>
/// ゴキブリのスクリプト
/// </summary>
public class Cockroach : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] CockroachScriptableObject m_data = null;
    /// <summary>現在の体力値</summary>
    [SerializeField] int m_hp = 100;
    /// <summary>無敵モード時間</summary>
    [SerializeField] float m_invincibleModeTime = 3f;
    /// <summary>加算する速度</summary>
    [SerializeField] float m_addSpeedValue = 5f;
    /// <summary>加算するジャンプ力</summary>
    [SerializeField] float m_addJumpValue = 5f;
    /// <summary>無敵モード</summary>
    [SerializeField] bool m_invincibleMode = false;

    [SerializeField] GameObject m_camera = null;

    CockroachMoveController m_moveController = null;
    CockroachUI m_cockroachUINetWork = null;

    Food m_food = null;
    AudioSource m_audio;
    Animator m_anim;

    /// <summary>死んだかどうか</summary>
    public bool m_isDed = false;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_moveController = GetComponent<CockroachMoveController>();
        EventSystem.Instance.Subscribe((EventSystem.Reset)ResetPosition);
        m_isDed = false;
        m_hp = m_data.MaxHP;

        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            m_camera?.SetActive(false);
            return;
        }

        m_audio = GetComponent<AudioSource>();
        m_cockroachUINetWork = GetComponent<CockroachUI>();
        m_camera?.SetActive(true);
        HumanAttackController.HitDamege += BeAttacked;
    }

    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe((EventSystem.Reset)ResetPosition);
    }

    public void ResetPosition(Vector3 v, Quaternion q)
    {
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            this.transform.position = v;
            this.transform.rotation = q;
        }
    }

    [PunRPC]
    void CheckAlive()
    {
        if (m_hp > 0) return;
        m_hp = 0;
        m_isDed = true;
        m_moveController.IsDed = true;
        EventSystem.Instance.IsDed(m_isDed);
        Debug.Log("Ded!");
    }

    /// <summary>
    /// 無敵モード
    /// </summary>
    IEnumerator InvincibleMode()
    {
        Debug.Log("無敵モード開始");
        // 無敵モード開始
        m_moveController.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", true);

        yield return new WaitForSeconds(m_invincibleModeTime);

        Debug.Log("無敵モード停止");
        // 無敵モード停止
        m_invincibleMode = false;
        m_moveController.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", false);
    }

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    [PunRPC]
    void Eat()
    {
        // 子供の生成
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        if (m_isDed) return;
        if (m_invincibleMode) return;

        m_invincibleMode = true;

        if (PhotonNetwork.IsConnected)
        {
            // 生存確認
            photonView.RPC(nameof(CheckAlive), RpcTarget.All);
            // 無敵モード開始
            photonView.RPC(nameof(StartCoroutineInvicibleMode), RpcTarget.All);
            // HPの同期（IsMine ではないオブジェクトからの同期なので OnPhotonSerializeView は使えない）
            photonView.RPC(nameof(RefleshHp), RpcTarget.All, m_hp, damageValue);
        }
        else
        {
            CheckAlive();
            StartCoroutineInvicibleMode();
            RefleshHp(m_hp, damageValue);
        }

        // ダメージを受けたUI表示
        StartCoroutine(m_cockroachUINetWork?.DamageColor());
        // HPバーを減少させる
        m_cockroachUINetWork?.ReflectHPSlider(m_hp, m_data.MaxHP);
    }

    [PunRPC]
    void StartCoroutineInvicibleMode()
    {
        StartCoroutine(InvincibleMode());
    }

    [PunRPC]
    void StartCoroutineDamegeImageChangeColor()
    {
        StartCoroutine(m_cockroachUINetWork.DamageColor());
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!photonView.IsMine) return;

        if (other.tag == "Food")
        {
            m_audio?.Play();

            if (!m_food)
            {
                m_food = other.gameObject.GetComponent<Food>();
            }
            else
            {
                photonView.RPC(nameof(Eat), RpcTarget.All, m_food.m_heelValue);
                //Eat(m_food.m_heelValue);
                m_food.UnActive();
            }
        }
    }

    [PunRPC]
    void RefleshHp(int hp, int damage)
    {
        hp -= damage;
        this.m_hp = hp;
        Debug.Log("HP : " + m_hp);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_isDed);

        }
        else
        {
            m_isDed = (bool)stream.ReceiveNext();
        }
    }
}