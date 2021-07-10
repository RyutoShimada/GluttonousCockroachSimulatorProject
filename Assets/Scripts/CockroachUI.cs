using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// ゴキブリのUI
/// </summary>
public class CockroachUI : MonoBehaviour
{
    [SerializeField] Image m_satietyGaugeImage = null;
    [SerializeField] Slider m_hpSlider = null;
    [SerializeField, Range(0.1f, 1.0f)] float m_secondsToDecrease = 0.5f;

    /// <summary>
    /// Cockroachから満腹ゲージが減った時に呼ぶ
    /// </summary>
    /// <param name="satietyGauge">満腹ゲージ</param>
    /// <param name="maxSatietyGauge">満腹ゲージの最大値</param>
    public void ReflectGauge(float satietyGauge, float maxSatietyGauge)
    {
        m_satietyGaugeImage.DOFillAmount(satietyGauge / maxSatietyGauge, 0.5f);
        //m_satietyGaugeImage.fillAmount = satietyGauge / maxSatietyGauge;
    }

    /// <summary>
    /// Cockroachから体力が減った時に呼ぶ
    /// </summary>
    /// <param name="hp">体力</param>
    /// <param name="maxHp">体力の最大値</param>
    public void ReflectHPSlider(float hp, float maxHp)
    {
        m_hpSlider.DOValue(hp / maxHp, 0.5f);
        //m_hpSlider.value = hp / maxHp;
    }
}
