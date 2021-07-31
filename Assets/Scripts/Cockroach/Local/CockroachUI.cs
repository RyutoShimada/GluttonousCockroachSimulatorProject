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
    /// <summary>満腹ゲージのUI</summary>
    Image m_satietyGaugeImage = null;
    /// <summary>HPバーのUI</summary>
    Slider m_hpSlider = null;
    /// <summary>ダメージを受けた時のUI</summary>
    Image m_damageImage = null;

    [SerializeField] GameObject m_cockroachUiPrefab = null;

    /// <summary>UIの動きを何秒かけて行うか</summary>
    [SerializeField, Range(0.1f, 1.0f)] float m_afterSeconds = 0.2f;
    /// <summary>m_damageImageの初期colorを保存しておく変数</summary>
    Color m_originDamageColor;
    /// <summary>Alfa値が0のm_damageImageを保存しておく変数</summary>
    Color m_color;

    private void Start()
    {
        if (!m_cockroachUiPrefab)
        {
            Debug.LogError("m_cockroachUiPrefab が NULL です。CockroachUI スクリプトに cockroachUiPrefab がアサインされているか確認して下さい。", this);
        }
        else
        {
            Transform t = GameObject.Find("Canvas").transform;
            GameObject go = Instantiate(m_cockroachUiPrefab, t);

            if (go)
            {
                m_satietyGaugeImage = m_cockroachUiPrefab.transform.Find("Gauge").GetComponentInChildren<Image>();
                m_hpSlider = m_cockroachUiPrefab.transform.Find("HPSlider").GetComponent<Slider>();
                m_damageImage = m_cockroachUiPrefab.transform.Find("DamageImage").GetComponent<Image>();
            }
            else
            {
                Debug.LogError("m_cockroachUiPrefab がインスタンス化されていません。", this);
            }
            

            if (m_damageImage)
            {
                m_damageImage.gameObject.SetActive(true);
                m_originDamageColor = m_damageImage.color;

                // アルファ値を0にする
                m_color = m_damageImage.color;
                m_color.a = 0;
                m_damageImage.color = m_color;
            }
            else
            {
                Debug.LogError("m_damageImage を GetComponent 出来ませんでした。", this);
            }
        }
    }

    /// <summary>
    /// ダメージを受けた時に画面を赤くする
    /// </summary>
    /// <returns></returns>
    public IEnumerator DamageColor()
    {
        m_damageImage.DOColor(m_originDamageColor, m_afterSeconds);
        yield return new WaitForSeconds(0.25f);
        m_damageImage.DOColor(m_color, m_afterSeconds);
    }

    /// <summary>
    /// Cockroachから満腹ゲージが減った時に呼ぶ
    /// </summary>
    /// <param name="satietyGauge">満腹ゲージ</param>
    /// <param name="maxSatietyGauge">満腹ゲージの最大値</param>
    public void ReflectGauge(float satietyGauge, float maxSatietyGauge)
    {
        m_satietyGaugeImage.DOFillAmount(satietyGauge / maxSatietyGauge, m_afterSeconds);
    }

    /// <summary>
    /// Cockroachから体力が減った時に呼ぶ
    /// </summary>
    /// <param name="hp">体力</param>
    /// <param name="maxHp">体力の最大値</param>
    public void ReflectHPSlider(float hp, float maxHp)
    {
        m_hpSlider.DOValue(hp / maxHp, m_afterSeconds);
    }
}
