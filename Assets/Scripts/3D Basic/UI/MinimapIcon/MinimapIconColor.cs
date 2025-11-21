using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MinimapIconColor : MonoBehaviour
{
    public Color iconColor = Color.white;

    void Start()
    {
        var renderer = GetComponent<Renderer>();

        // PropertyBlock 생성
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);

        // 머티리얼의 _Color 속성을 개별로 설정
        block.SetColor("_Color", iconColor);

        renderer.SetPropertyBlock(block);
    }
}