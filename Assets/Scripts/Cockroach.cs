using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockroach : MonoBehaviour
{
    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxSatietyGauge = 100;
    /// <summary>満腹ゲージ</summary>
    [SerializeField] int m_satietyGauge = 100;
    /// <summary>満腹ゲージが1秒間に減少する値</summary>
    [SerializeField] int m_decreaseValueIn1second = 1;
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
    /// <summary>CockroachMoveController</summary>
    CockroachMoveController m_CMC;
    /// <summary>1秒間を測るためのタイマー</summary>
    float m_oneSecondTimer = 0f;
    /// <summary>無敵時間を測るためのタイマー</summary>
    float m_invincibleTimer = 0f;
    /// <summary>死んだかどうか</summary>
    bool m_isDed = false;

    private void Start()
    {
        m_satietyGauge = m_maxSatietyGauge;
        m_CMC = GetComponent<CockroachMoveController>();
    }

    private void Update()
    {
        if (m_isDed) return;
        DecreaseHitPoint(m_decreaseValueIn1second);
        CheckInvincibleMode();
    }

    /// <summary>
    /// 無敵モードの判定
    /// </summary>
    void CheckInvincibleMode()
    {
        if (!m_invincibleMode) return;

        m_invincibleTimer += Time.deltaTime;

        if (m_invincibleTimer <= m_invincibleModeTime) return;

        m_invincibleTimer = 0;
        m_invincibleMode = false;
        // 無敵モード停止
        m_CMC.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
    }

    /// <summary>
    /// 1秒おきにHitPointを減らす
    /// </summary>
    /// /// <param name="decreaseValue">減少させる量</param>
    void DecreaseHitPoint(int decreaseValue)
    {
        m_oneSecondTimer += Time.deltaTime;

        if (m_oneSecondTimer < 1f) return;

        m_oneSecondTimer = 0;
        m_satietyGauge -= decreaseValue; //満腹ゲージを減らす
        Debug.Log("満腹ゲージ : " + m_satietyGauge);
    }

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    void Eat(int heelValue)
    {
        m_satietyGauge += heelValue;

        // 現在のHPがHPの最大値を超えないようにする
        if (m_satietyGauge > m_maxSatietyGauge)
        {
            m_satietyGauge = m_maxSatietyGauge;
        }

        Debug.Log("Heel");
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        if (m_hp < 0)
        {
            m_hp = 0;
            m_isDed = true;
            Debug.Log("Ded!");
        }
        else
        {
            m_hp -= damageValue;
            m_invincibleMode = true;
            m_CMC.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        }

        Debug.Log($"Hit!Damage! -{damageValue}, HP : {m_hp}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food")
        {
            Eat(other.gameObject.GetComponent<Food>().m_heelValue);
            Debug.Log($"Eat! +{other.gameObject.GetComponent<Food>().m_heelValue}, 満腹ゲージ : {m_satietyGauge}");
            other.gameObject.GetComponent<Food>().UnActive();
        }
    }
}
