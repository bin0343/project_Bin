using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Table_Reload : MonoBehaviour
{
    [MenuItem("CS_Util/Table/CSV &F1", false, 1)]   //CSV &F1 띄워야함. f1은 단축키
    static public void ParserTableCSV()
    {
        Shared.TableManager = new TableManager();
        Shared.TableManager.Init();
        Shared.TableManager.Save();
    }
}
