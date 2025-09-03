using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour
{
    private Text text;
    private Color alpha;

    public float moveSpeed = 2f; //텍스트 위로 올라가는 속도
    public float alphaSpeed = 1.5f; //텍스트 투명해지는 속도
    public float destroyTime = 1.5f; //텍스트 사라지는 시간

    void Start()
    {
        text = GetComponent<Text>();
        alpha = text.color;
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 텍스트를 위로 이동시킴
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));

        // 텍스트의 알파(투명도) 값을 점차 0으로 변경하여 서서히 사라지게 함
        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        text.color = alpha;
    }

    public void SetDamage(int damage)
    {
        if (text == null)
        {
            text = GetComponent<Text>();
        }
        text.text = damage.ToString();
    }
}
