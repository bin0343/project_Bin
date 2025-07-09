using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public partial class SceneMANAGER : MonoBehaviour
{
    string FilePath = "Test.txt";
    string StatFile = "Stat.txt";

    private Stat stat;

    public void Init(Stat _stat)
    {
        stat = _stat;
        SaveStat();
    }

    public void SaveFile()
    {
        if (!File.Exists(Application.dataPath + FilePath))
        {
            StreamWriter sw = File.CreateText(FilePath);

            sw.WriteLine("data 100");
            sw.WriteLine("atk 200");

            sw.Close();     //닫아주지 않으면 열고있는 상태. 에러가 생길 수 있음.
        }
        else
        {
            StreamReader sr = File.OpenText(Application.dataPath + FilePath);

            string str1 = sr.ReadLine();

            sr.Close();
        }
    }

    public void SaveStat()
    {
        if (!File.Exists(Application.dataPath + StatFile))
        {
            StreamWriter sw = File.CreateText(StatFile);

            sw.WriteLine($"Hp : {stat.Hp}\nAtk : {stat.Atk}");

            sw.Close();
        }
        else
        {
            StreamReader sr = File.OpenText(Application.dataPath + StatFile);

            string str1 = sr.ReadLine();

            sr.Close();
        }
        Debug.Log($"SaveStat 실행됨. HP: {stat.Hp}, Atk: {stat.Atk}");
    }

    /*public void save(int _Hp, int _Atk)
    {
        if (!File.Exists(Application.dataPath + FilePath))
        {
            StreamWriter sw = File.CreateText(FilePath);

            sw.WriteLine(_Hp);
            sw.WriteLine(_Atk);

            sw.Close();     //닫아주지 않으면 열고있는 상태. 에러가 생길 수 있음.
        }
        else
        {
            StreamReader sr = File.OpenText(Application.dataPath + FilePath);

            string str1 = sr.ReadLine();

            sr.Close();
        }
    }*/

    public void ReadFile(ref int _Hp, ref int _Atk)     //읽기전용(로드)
    {
        StreamReader sr = File.OpenText(Application.dataPath + FilePath);

        string hp = sr.ReadLine();
        string atk = sr.ReadLine();

        sr.Close();

        _Hp = int.Parse(hp);
        _Atk = int.Parse(atk);  
    }
}
