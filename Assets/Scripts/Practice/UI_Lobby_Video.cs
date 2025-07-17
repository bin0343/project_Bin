using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public partial class UI_Lobby : MonoBehaviour
{
    public VideoPlayer VIDEOPLAYER;

    public RawImage RAWIMAGE;
    public Button SkipButton;

    bool VideoSkip = false;

    void SetVideo()
    {
        VideoSkip = false;

        StartCoroutine(IUpdateVideo());
    }

    void SkipVideo()
    {
        VideoSkip = true;
        /*VIDEOPLAYER.Stop();
        VIDEOPLAYER.gameObject.SetActive(false);*/
    }

    IEnumerator IUpdateVideo()
    {
        VideoClip Clip = Resources.Load<VideoClip>("Video/EF_Normal");
        VIDEOPLAYER.clip = Clip;

        VIDEOPLAYER.Play();

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            RAWIMAGE.texture = VIDEOPLAYER.texture;

            if (VideoSkip)
            {
                //VIDEOPLAYER.Stop();
                break;
            }
        }
        VIDEOPLAYER.gameObject.SetActive(false);
    }

    /*private void Update()
    {
        RAWIMAGE.texture = VIDEOPLAYER.texture;
    }*/
}
