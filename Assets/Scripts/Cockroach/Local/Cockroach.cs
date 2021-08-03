using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゴキブリのスクリプト
/// </summary>
public class Cockroach : MonoBehaviour
{
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

    CockroachMoveController m_CMC;
    CockroachUI m_CU;
    GameManager m_GM;
    /// <summary>1秒間を測るためのタイマー</summary>
    float m_oneSecondTimer = 0f;
    /// <summary>死んだかどうか</summary>
    public bool m_isDed = false;

    private void Start()
    {
        m_isDed = false;
        m_satietyGauge = m_maxSatietyGauge;
        m_hp = m_maxHp;
        m_CMC = GetComponent<CockroachMoveController>();
        m_CU = GetComponent<CockroachUI>();
        m_CU.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
        m_CU.ReflectHPSlider(m_hp, m_maxHp);
        m_GM = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (m_isDed) return;
        DecreaseHitPoint(m_decreaseValueIn1second);
    }

    void CheckAlive()
    {
        if (m_hp > 0) return;
        m_hp = 0;
        m_isDed = true;
        m_CMC.IsDed = true;
        Debug.Log("Ded!");
    }

    /// <summary>
    /// 無敵モード
    /// </summary>
    IEnumerator InvincibleMode()
    {
        Debug.Log("無敵モード開始");
        // 無敵モード開始
        m_invincibleMode = true;
        m_CMC.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);

        yield return new WaitForSeconds(m_invincibleModeTime);

        Debug.Log("無敵モード停止");
        // 無敵モード停止
        m_invincibleMode = false;
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

        if (m_satietyGauge > 0)
        {
            //満腹ゲージを減らす
            m_satietyGauge -= decreaseValue;
        }
        else
        {
            // 体力を減らす
            m_hp -= decreaseValue;
            StartCoroutine(m_CU.DamageColor());
        }

        m_CU.ReflectGauge(m_satietyGauge , m_maxSatietyGauge);
        m_CU.ReflectHPSlider(m_hp, m_maxHp);

        CheckAlive();
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

        m_CU.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
        m_GM.FoodGenerate();
        Debug.Log("Heel");
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        if (m_isDed) return;
        if (m_invincibleMode) return;

        m_hp -= damageValue;
        CheckAlive();                           // 生存確認
        StartCoroutine(InvincibleMode());       // 無敵モード開始
        StartCoroutine(m_CU.DamageColor());     // ダメージを受けたUI表示
        m_CU.ReflectHPSlider(m_hp, m_maxHp);    // HPバーを減少させる
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food")
        {
            //Eat(other.gameObject.GetComponent<Food>().m_heelValue);
        }
    }
}
