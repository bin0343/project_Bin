using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class UI_Loading : MonoBehaviour
{
    public LOADSTRING LoadString;
    public Text LoadText;
    public string[] LANS;       //public을 써주면 에디터만 활용하면 다 바꿀 수 있음. private는 개발자만 가능. 왜? 코드에서 바꿔줘야 하니까. public은 에디터상에서 가능
    public Image BackGroundsImage;
    public Sprite[] BackGrounds;

    public List<string> LoadingMessages = new List<string>()
    {
        "캐릭터 레벨업을 위해선 경험치가 필요합니다.",
        "스킬은 쿨타임이 있습니다.",
        "골드는 기본 재화입니다.",
        "몬스터를 잡으면 아이템을 획득 할 수 있습니다.",
        "옵션에서 사운드를 조절 할 수 있습니다."
    };

    public void Start()
    {
        StartCoroutine(ChangeString());
        StartCoroutine(ChangeBackGround());
    }

    void SetImg()
    {
        BackGroundsImage.sprite = Shared.SceneMANAGER.GetSpriteAtlas("Item", "Skeleton_0");
        
    }
    
    IEnumerator ChangeString()
    {
        while (true)
        {
            int rand = Random.Range(0, LoadingMessages.Count);
            LoadText.text = LoadingMessages[rand];
            yield return new WaitForSeconds(3f);
        }
        //startcoroutine 가능
    }

    IEnumerator ChangeBackGround()
    {
        while (true)
        {
            int rand = Random.Range(0, BackGrounds.Length);
            BackGroundsImage.sprite = BackGrounds[rand];
            yield return new WaitForSeconds(2f);
        }
    }

    /*public void String(LOADSTRING _s)
    {
        switch (_s)
        {
            case LOADSTRING.STRING1:
                LoadText.text = "캐릭터 레벨업을 위해선 경험치가 필요합니다.";
                break;
            case LOADSTRING.STRING2:
                LoadText.text = "스킬은 쿨타임이 있습니다.";
                break;
            case LOADSTRING.STRING3:
                LoadText.text = "골드는 기본 재화입니다.";
                break;
            case LOADSTRING.STRING4:
                LoadText.text = "몬스터를 잡으면 아이템을 획득 할 수 있습니다.";
                break;
            case LOADSTRING.STRING5:
                LoadText.text = "옵션에서 사운드를 조절 할 수 있습니다.";
                break;
        }
    }

    IEnumerator ChangeString()
    {
        while (true)
        {
            int rand = Random.Range(0, 5);
            String((LOADSTRING)rand);

            yield return new WaitForSeconds(3f);
        }
    }*/
}
