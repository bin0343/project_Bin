using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class TableBase : MonoBehaviour
{
    string GetTablePath()
    {
#if UNITY_EDITOR
        return Application.dataPath;
#else
        return Application.persistentDataPath + "Assets";
#endif
    }

    protected void Load_Binary<T>(string _Name, ref T _Obj)
    {
        var b = new BinaryFormatter();

        b.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

        TextAsset asset = Resources.Load("Table_" + _Name) as TextAsset;

        Stream stream = new MemoryStream(asset.bytes);

        _Obj = (T)b.Deserialize(stream);

        stream.Close();
    }

    protected void Save_Binary(string _Name, object _Obj)
    {
        string path = GetTablePath() + "/Table/Resources/" + "Table_" + _Name + ".txt";

        var b = new BinaryFormatter();

        Stream stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);

        b.Serialize(stream, _Obj);

        stream.Close();
    }

    protected CsvReader GetCsvReader(string _Name)
    {
        string ext = ".csv";

        FileStream file = new FileStream("./Document/" + _Name + ext, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        StreamReader stream = new StreamReader(file, System.Text.Encoding.UTF8);

        CsvReader reader = new CsvReader();

        reader.Parse(stream.ReadToEnd(), false, 1);

        stream.Close();

        return reader;
    }
}
