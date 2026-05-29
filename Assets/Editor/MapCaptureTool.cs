using UnityEngine;
using UnityEditor;

public class MapCaptureTool : MonoBehaviour
{
    // 유니티 상단 메뉴바에 [Tools] -> [맵 고해상도 캡처하기] 버튼을 만들어줍니다.
    [MenuItem("Tools/맵 고해상도 캡처하기")]
    public static void CaptureMap()
    {
        // 파일 이름을 캡처한 시간으로 설정
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"MapCapture_{timestamp}.png";

        // 화면 캡처 실행 (해상도를 2배(슈퍼 사이즈)로 뻥튀기해서 찍고 싶으면 뒤에 숫자 2나 3을 넣어도 됩니다)
        ScreenCapture.CaptureScreenshot(fileName, 1);

        Debug.Log($"[캡처 완료] 유니티 프로젝트 최상단 폴더에 {fileName} 파일이 저장되었습니다!");

        // 에디터 폴더를 새로고침해서 파일이 바로 보이게 함
        AssetDatabase.Refresh();
    }
}