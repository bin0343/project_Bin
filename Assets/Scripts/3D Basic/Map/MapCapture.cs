using UnityEngine;
using System.IO;

public class MapCapture : MonoBehaviour
{
    public Camera captureCamera;
    public RenderTexture rt;

    [ContextMenu("Capture Map")]
    public void Capture()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        captureCamera.Render();

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = currentRT;

        byte[] bytes = tex.EncodeToPNG();
        string path = Application.dataPath + "/MapCapture.png";
        File.WriteAllBytes(path, bytes);

        Debug.Log("Map saved to: " + path);
    }
}
