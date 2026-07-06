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

    [Header("극한회피 데미지 표시")]
    [SerializeField] private Color perfectEvadeColor = new Color(1f, 0.82f, 0.12f, 1f);
    [SerializeField] private float perfectEvadeSizeMulitplier = 1.35f;
    [SerializeField] private bool useBoldItalic = true;

    private int defaultFontSize;
    private FontStyle defaultFontStyle;
    private Color defaultColor;
    private bool initialized = false;

    private void Awake()
    {
        Initialize();
    }

    void Start()
    {
        Initialize();
        alpha = text.color;
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));

        alpha.a = Mathf.Lerp(alpha.a, 0, Time.deltaTime * alphaSpeed);
        text.color = alpha;
    }

    private void Initialize()
    {
        if (initialized) return;

        text = GetComponent <Text>();

        if (text != null)
        {
            defaultFontSize = text.fontSize;
            defaultFontStyle = text.fontStyle;
            defaultColor = text.color;
        }

        initialized = true;
    }

    public void SetDamage(int damage)
    {
        SetDamage(damage, false);
    }

    public void SetDamage(int damage, bool isPerfectEvadeBonus)
    {
        Initialize();

        if (text == null) return;

        text.text = damage.ToString();

        if (isPerfectEvadeBonus)
        {
            text.color = perfectEvadeColor;
            text.fontSize = Mathf.RoundToInt(defaultFontSize * perfectEvadeSizeMulitplier);

            if (useBoldItalic)
            {
                text.fontStyle = FontStyle.BoldAndItalic;
            }
        }
        else
        {
            text.color = defaultColor;
            text.fontSize = defaultFontSize;
            text.fontStyle = defaultFontStyle;
        }

        alpha = text.color;
    }
}
