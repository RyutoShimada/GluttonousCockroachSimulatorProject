using System.Collections;
using UnityEngine;
using Photon.Pun;
using System;

[RequireComponent(typeof(CockroachMoveController))]

/// <summary>
/// ゴキブリのスクリプト
/// </summary>
public class Cockroach : MonoBehaviourPunCallbacks, IPunObservable
{
    public static Action IsDed;
    public static Action<int, Vector3, Vector3> Generate;
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

    [SerializeField] GameObject m_miniMapPrefab = null;

    [SerializeField] GameObject m_dedEffect = null;

    [SerializeField] AudioClip m_eatSE = null;

    [SerializeField] AudioClip m_dedSE = null;

    CockroachMoveController m_moveController = null;
    CockroachUI m_cockroachUINetWork = null;

    Food m_food = null;
    AudioSource m_audio;
    Animator m_anim;

    bool m_eating = false;

    /// <summary>死んだかどうか</summary>
    [HideInInspector] public bool m_isDed = false;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
        m_moveController = GetComponent<CockroachMoveController>();
        EventSystem.Instance.Subscribe((EventSystem.Reset)ResetPosition);
        m_isDed = false;
        m_hp = m_data.MaxHP;

        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            HumanAttackController.HitDamege += BeAttacked;
            m_camera?.SetActive(false);
            return;
        }

        m_audio = GetComponent<AudioSource>();
        m_cockroachUINetWork = GetComponent<CockroachUI>();
        m_camera?.SetActive(true);
        GameObject go = Instantiate(m_miniMapPrefab);
        go.transform.GetComponentInChildren<MiniMap>().m_player = this.gameObject;
    }

    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe((EventSystem.Reset)ResetPosition);
        HumanAttackController.HitDamege -= BeAttacked;
    }

    public void ResetPosition(Vector3 v, Quaternion q)
    {
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            this.transform.position = v;
            this.transform.rotation = q;
        }
    }

    void CheckAlive()
    {
        if (m_hp > 0) return;
        m_hp = 0;
        m_isDed = true;
        m_moveController.IsDed = true;
        Instantiate(m_dedEffect, transform.position + transform.up * 0.2f, transform.rotation);
        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            m_audio.PlayOneShot(m_dedSE);
        }
        else if (!PhotonNetwork.IsConnected)
        {
            m_audio.PlayOneShot(m_dedSE);
        }

        IsDed?.Invoke();
        Invoke(nameof(UnActive), 0.2f);
    }

    void UnActive() => this.gameObject.SetActive(false);

    /// <summary>
    /// 無敵モード
    /// </summary>
    IEnumerator InvincibleMode()
    {
        // 無敵モード開始
        m_moveController.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", true);

        yield return new WaitForSeconds(m_invincibleModeTime);

        // 無敵モード停止
        m_invincibleMode = false;
        m_moveController.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        m_anim.SetBool("Damage", false);
    }

    void Eat()
    {
        // 子供の生成
        int random = UnityEngine.Random.Range(30, 36);
        CallGenerate(random);
    }

    public void CallGenerate(int count)
    {
        Generate.Invoke(count, transform.position, transform.up);
        m_eating = false;
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
            // 無敵モード開始
            photonView.RPC(nameof(StartCoroutineInvicibleMode), RpcTarget.All);
            // HPの同期
            photonView.RPC(nameof(RefleshHp), RpcTarget.Others, damageValue);
        }
        else
        {
            StartCoroutineInvicibleMode();
            RefleshHp(damageValue);
            StartCoroutine(m_cockroachUINetWork?.DamageColor());
            m_cockroachUINetWork?.ReflectHPSlider(m_hp, m_data.MaxHP);
        }
    }

    [PunRPC]
    void StartCoroutineInvicibleMode()
    {
        StartCoroutine(InvincibleMode());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Food") return;

        if (!m_food)
        {
            m_food = other.gameObject.GetComponent<Food>();
        }
        else
        {
            m_food.UnActive();
            if (PhotonNetwork.IsConnected && !photonView.IsMine || m_eating) return;
            m_eating = true;
            Eat();
        }

        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_audio.PlayOneShot(m_eatSE);
    }

    [PunRPC]
    void RefleshHp(int damage)
    {
        m_hp -= damage;
        // HPバーを減少させる
        m_cockroachUINetWork.ReflectHPSlider(m_hp, m_data.MaxHP);
        // ダメージを受けたUI表示
        StartCoroutine(m_cockroachUINetWork.DamageColor());
        CheckAlive();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 実装しないと Observed scripts have to implement IPunObservable.
        //このエラーが出る。インターフェイスの継承をつけていなくてもでる.
    }
}