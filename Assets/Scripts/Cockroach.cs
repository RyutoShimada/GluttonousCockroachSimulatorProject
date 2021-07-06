using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockroach : MonoBehaviour
{
    /// <summary>体力</summary>
    [SerializeField] int m_hp = 100;
    /// <summary>満腹ゲージ</summary>
    [SerializeField] int m_satietyGauge = 100;
    /// <summary>満腹ゲージが1秒間に減少する値</summary>
    [SerializeField] int m_decreaseValue = 1;

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    void Eat(int heelValue)
    {
        m_hp += heelValue;
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        m_hp -= damageValue;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food")
        {
            // Eat(other.gameObject.GetComponent<Food>().m_heelValue);
        }
    }
}
