using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTile : MonoBehaviour
{
    public string nextScene;
    public Vector3 spawnPositionInNextScene;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("포탈 진입!");
            //PortalManager.Instance.spawnPosition = spawnPositionInNextScene;
            //SceneManager.LoadScene(nextScene);
            StartCoroutine(TeleportPlayer());
        }
    }

    private IEnumerator TeleportPlayer()
    {
        //FadeManager.Instance.FadeToScene(nextScene);

        //yield return new WaitForSeconds(FadeManager.Instance.fadeDuration); // 씬 로드 기다리기

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(nextScene);

        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //player.transform.position = spawnPositionInNextScene;
    }
}
