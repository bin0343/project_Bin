using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneMANAGER : MonoBehaviour
{
    public void SetPlayerPrefsIntKey(string _key, int _Value)
    {
        PlayerPrefs.SetInt(_key, _Value);
        PlayerPrefs.Save();
    }

    public int GetPlayerPrefsIntKey(string _key)
    {
        return PlayerPrefs.GetInt(_key);
    }

    public void SetPlayerPrefsFloatKey(string _key, float _Value)
    {
        PlayerPrefs.SetFloat(_key, _Value);
        PlayerPrefs.Save();
    }

    public float GetPlayerPrefsFloatKEy(string _key)
    {
        return PlayerPrefs.GetFloat(_key);
    }

    public void SetPlayerPrefsStringKey(string _key, string _Value)
    {
        PlayerPrefs.SetString(_key, _Value);
        PlayerPrefs.Save();
    }

    public string GetPlayerPrefsStringKey(string _key)
    {
        return PlayerPrefs.GetString(_key);
    }

    public void DeletePlayerPrefsKey(string _key)
    {
        PlayerPrefs.DeleteKey(_key);
    }

    public void DeletePlayerPrefsAll()
    {
        PlayerPrefs.DeleteAll();
    }
}
