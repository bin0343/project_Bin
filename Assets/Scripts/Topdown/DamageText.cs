using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    //public TextMeshProUGUI damageText;
    public Text damageText;
    public float moveUpAmount = 1f;
    public float fadeTime = 0.5f;

    private float timer;

    public void Setup(int damageAmount)
    {
        damageText.text = damageAmount.ToString();
        transform.localScale = Vector3.one;

        timer = fadeTime;
    }

    void Update()
    {
        if (transform.parent != null && transform.parent.GetComponent<Canvas>()?.renderMode == RenderMode.WorldSpace)
        {
            transform.localPosition += Vector3.up * Time.deltaTime * moveUpAmount;
        }
        else
        {
            transform.position += Vector3.up * Time.deltaTime * moveUpAmount;
        }
        timer -= Time.deltaTime;

        if (damageText != null)
        {
            var color = damageText.color;
            color.a = Mathf.Clamp01(timer / fadeTime);
            damageText.color = color;
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}