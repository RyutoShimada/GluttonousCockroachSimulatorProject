using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockroach : MonoBehaviour
{
    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxHp = 100;
    /// <summary>満腹ゲージ</summary>
    [SerializeField] int m_satietyGauge = 100;
    /// <summary>満腹ゲージが1秒間に減少する値</summary>
    [SerializeField] int m_decreaseValueIn1second = 1;
    /// <summary>現在の体力値</summary>
    int m_currentHp;
    /// <summary>死んだかどうか</summary>
    bool m_isDed = false;
    float m_timer = 0;

    private void Start()
    {
        m_currentHp = m_maxHp;
    }

    private void Update()
    {
        DecreaseHitPoint(m_decreaseValueIn1second);
    }

    /// <summary>
    /// 1秒おきにHitPointを減らす
    /// </summary>
    /// /// <param name="decreaseValue">減少させる量</param>
    void DecreaseHitPoint(int decreaseValue)
    {
        m_timer += Time.deltaTime;

        if (m_timer >= 1f)
        {
            m_timer = 0;
            m_currentHp -= decreaseValue;
            Debug.Log("HP : " + m_currentHp);
        }
    }

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    void Eat(int heelValue)
    {
        m_currentHp += heelValue;

        // 現在のHPがHPの最大値を超えないようにする
        if (m_currentHp > m_maxHp)
        {
            m_currentHp = m_maxHp;
        }

        Debug.Log("Heel");
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        m_currentHp -= damageValue;
        Debug.Log($"Hit!Damage! -{damageValue}, HP : {m_currentHp}");
        if (m_currentHp < 0)
        {
            m_currentHp = 0;
            m_isDed = true;
            Debug.Log("Ded!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food")
        {
            Eat(other.gameObject.GetComponent<Food>().m_heelValue);
            Debug.Log($"Eat! +{other.gameObject.GetComponent<Food>().m_heelValue}, HP : {m_currentHp}");
            other.gameObject.GetComponent<Food>().UnActive();
        }
    }
}
