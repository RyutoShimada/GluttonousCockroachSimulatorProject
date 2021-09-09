using System.Collections;
using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(CockroachMoveControllerNetWork))]
[RequireComponent(typeof(CockroachUINetWork))]

/// <summary>
/// ゴキブリのスクリプト
/// </summary>
public class CockroachNetWork : MonoBehaviourPunCallbacks, IPunObservable
{
    static public GameObject m_Instance;

    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxSatietyGauge = 100;
    /// <summary>満腹ゲージ</summary>
    [SerializeField] int m_satietyGauge = 100;
    /// <summary>満腹ゲージが1秒間に減少する値</summary>
    [SerializeField] int m_decreaseValueIn1second = 1;
    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxHp = 100;
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

    [SerializeField] bool m_godMode = false;

    CockroachMoveControllerNetWork m_cockroachMoveControllerNetWork = null;
    CockroachUINetWork m_cockroachUINetWork = null;

    Food m_food = null;
    AudioSource m_audio;
    Animator m_anim;

    /// <summary>1秒間を測るためのタイマー</summary>
    float m_oneSecondTimer = 0f;
    /// <summary>死んだかどうか</summary>
    public bool m_isDed = false;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_cockroachMoveControllerNetWork = GetComponent<CockroachMoveControllerNetWork>();
        m_cockroachUINetWork = GetComponent<CockroachUINetWork>();
        EventSystem.Instance.Subscribe((EventSystem.ResetTransform)ResetPosition);

        if (photonView.IsMine)
        {
            m_audio = GetComponent<AudioSource>();
            m_camera.SetActive(true);

            m_isDed = false;
            m_satietyGauge = m_maxSatietyGauge;
            m_hp = m_maxHp;
        }
        else
        {
            m_camera.SetActive(false);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (!NetWorkGameManager.m_Instance.IsGame) return;
            if (m_isDed) return;
            DecreaseHitPoint(m_decreaseValueIn1second);
        }
    }

    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe((EventSystem.ResetTransform)ResetPosition);
    }

    public void ResetPosition(Vector3 v, Quaternion q)
    {
        if (photonView.IsMine)
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
        m_cockroachMoveControllerNetWork.IsDed = true;
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
        m_cockroachMoveControllerNetWork.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", true);

        yield return new WaitForSeconds(m_invincibleModeTime);

        Debug.Log("無敵モード停止");
        // 無敵モード停止
        m_invincibleMode = false;
        m_cockroachMoveControllerNetWork.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", false);
    }

    /// <summary>
    /// 1秒おきにHitPointを減らす
    /// </summary>
    /// /// <param name="decreaseValue">減少させる量</param>
    void DecreaseHitPoint(int decreaseValue)
    {
        m_oneSecondTimer += Time.deltaTime;

        if (m_oneSecondTimer < 1f) return;

        if (m_satietyGauge > 0)
        {
            //満腹ゲージを減らす
            m_satietyGauge -= decreaseValue;
        }
        else
        {
            // 体力を減らす
            m_hp -= decreaseValue;
            if (m_cockroachUINetWork)
            {
                StartCoroutine(m_cockroachUINetWork.DamageColor());
            }
        }

        m_cockroachUINetWork.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
        m_cockroachUINetWork.ReflectHPSlider(m_hp, m_maxHp);
        photonView.RPC(nameof(CheckAlive), RpcTarget.All);

        m_oneSecondTimer = 0;
    }

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    [PunRPC]
    void Eat(int heelValue)
    {
        m_satietyGauge += heelValue;

        // 現在のHPがHPの最大値を超えないようにする
        if (m_satietyGauge > m_maxSatietyGauge)
        {
            m_satietyGauge = m_maxSatietyGauge;
        }

        m_cockroachUINetWork.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
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

        // 生存確認
        photonView.RPC(nameof(CheckAlive), RpcTarget.All);
        // 無敵モード開始
        photonView.RPC(nameof(StartCoroutineInvicibleMode), RpcTarget.All);
        // ダメージを受けたUI表示
        photonView.RPC(nameof(StartCoroutineDamegeImageChangeColor), RpcTarget.Others);
        // HPの同期（IsMine ではないオブジェクトからの同期なので OnPhotonSerializeView は使えない）
        photonView.RPC(nameof(RefleshHp), RpcTarget.All, m_hp, damageValue);
        // HPバーを減少させる
        photonView.RPC(nameof(m_cockroachUINetWork.ReflectHPSlider), RpcTarget.Others, m_hp, m_maxHp);
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