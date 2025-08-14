using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CsvReader
{
    private System.String[,] Arr_Grid;      //엑셀 테이블의 행과 열 나타냄.[,] 이부분.

    public CsvReader()
    {

    }

    public CsvReader(System.String[,] _Grid)
    {
        Arr_Grid = _Grid;
    }

    public System.String[,] Grid
    {
        get { return Arr_Grid; }        //테이블의 정보를 담는다.
    }

    public CsvReader Parse(UnityEngine.TextAsset _TextAsset, bool _Debug)
    {
        Parse(_TextAsset, _Debug);

        return this;
    }

    public CsvReader Parse(string _Text, bool _Debug, int _Encode = 0)
    {
        Arr_Grid = SplitCsvGrid(_Text, _Encode);        //grid = csv에 있는 공간들(각 셀들) - arr_grid에 담음.

        if (_Debug)
            DebugOutPutGrid();

        return this;
    }

    public void DebugOutPutGrid()
    {
        System.String textoutput = "";

        for (int i=0; i < Arr_Grid.GetUpperBound(1); i++)
        {
            for (int j = 0; j < Arr_Grid.GetUpperBound(0); j++)
            {
                textoutput += Arr_Grid[j, i];
                textoutput += "|";

                textoutput = "\n";
            }    
        }

        Debug.Log(textoutput);
    }

    public int Column       //열과 행
    {
        get { return Arr_Grid.GetUpperBound(0); }
    }

    public int Row
    {
        get { return Arr_Grid.GetUpperBound(1); }
    }

    public System.String[] GetRowArray(int _Row)        //열에 있는  정보들 읽어올때.
    {
        System.String[] arr = new System.String[Column];

        for (int i = 0; i< Column; ++i)
        {
            arr[i] = Arr_Grid[i, _Row];
        }

        return arr;
    }

    public bool IsData(int _Row, int _Col)      //데이터 유무 처리. 공백인지, 정보가 아예 없는지.
    {
        string s = Arr_Grid[_Col, _Row];

        if ((s == null) || (s == ""))
            return false;

        return true;
    }

    public int Getint(int _Row, int _Col)       //읽어오는것들이 모두 string이라서 int로 형변환.
    {
        string s = Arr_Grid[_Col, _Row];

        if ((s == null) || (s == ""))
            return 0;

        return (int)System.Convert.ToInt32(s);
    }

    private int CurCol = 0;         //한줄한줄 읽어올떄 읽어온 줄들을 저장.

    public bool ResetRow(int _Row, int _StartCol)
    {
        CurCol = _StartCol; 
        
        string s = Arr_Grid[_StartCol, _Row];
        
        if (s == null)
            return false;

        if (Arr_Grid[_StartCol, _Row] == "")
            return false;

        return true;
    }

    public void Get(int _Row, ref bool _Val)        //bool형 갖고 오기.
    {
        string s = Arr_Grid[CurCol, _Row];

        ++CurCol;

        if ((s == null) || (s == ""))
        {
            _Val = false;

            return;
        }

        _Val = ((int)System.Convert.ToInt32(s) != 0);
    }

    public void Get(int _Row, ref int _Val)
    {
        string s = Arr_Grid[CurCol, _Row];

        ++CurCol;

        if ((s == null) || (s == ""))
        {
            _Val = 0;

            return;
        }

        _Val = ((int)System.Convert.ToInt32(s));
    }

    public void Get(int _Row, ref long _Val)
    {
        string s = Arr_Grid[CurCol, _Row];

        ++CurCol;

        if ((s == null) || (s == ""))
        {
            _Val = 0;
            return;
        }

        _Val = (long)System.Convert.ToInt64(s);
    }

    public void Get(int _Row, ref float _Val)   //확인 필요
    {
        string s = Arr_Grid[CurCol, _Row];

        ++CurCol;

        if ((s == null) || (s == ""))
        {
            _Val = 0;
            return;
        }

        _Val = (float)System.Convert.ToInt32(s);
    }

    public void Get(int _Row, ref string _Val)      //확인 필요
    {
        string s = Arr_Grid[CurCol, _Row];

        ++CurCol;

        if ((s == null) || (s == ""))
        {
            _Val = "";
            return;
        }

        _Val = s;
    }

    public void Get(int _Row, ref int[] _Val, float _Cnt)
    {
        for (int i = 0; i < _Cnt; ++i)
        {
            string s = Arr_Grid[CurCol, _Row];

            ++CurCol;

            if ((s == null) || (s == ""))
            {
                _Val[i] = 0;
                continue;
            }

            _Val[i] = (int)System.Convert.ToInt32(s);
        }
    }

    public void Get(int _Row, ref string[] _Val, float _Cnt)
    {
        for (int i = 0; i < _Cnt; ++i)
        {
            string s = Arr_Grid[CurCol, _Row];

            ++CurCol;

            if ((s == null) || (s == ""))
            {
                _Val[i] = "";
                continue;
            }

            _Val[i] = s;
        }
    }

    public CsvReader Find(int _FieldIndex, System.String _Value)        //내가 찾고자 하는 정보 갖고올때.
    {
        List<int> listindex = new List<int>();

        for (int i=0; i< Arr_Grid.GetUpperBound(0); ++i)
        {
            if (_Value != Arr_Grid[_FieldIndex, i])
                continue;

            listindex.Add(i);   
        }

        if (0 == listindex.Count)
            return null;

        System.String[,] arrnewgrid = new System.String[Arr_Grid.GetUpperBound(0) + 1, listindex.Count + 1];

        for(int i=0; i<Arr_Grid.GetUpperBound(0); ++i )//j?
        {
            for(int j = 0; j<listindex.Count; ++i)
            {
                arrnewgrid[i,j] = Arr_Grid[i, listindex[j]];
            }
        }   
        
        return new CsvReader(arrnewgrid);
    }

    public System.String FindValue(int _FieldIndex, System.String _Value, System.Object _Field)     //안써도 되긴함. find로 해결가능.
    {
        return Find(_FieldIndex, _Value).Grid[System.Convert.ToInt32(_Field), 0];
    }

    System.String[,] SplitCsvGrid(System.String _CsvText, int _Encode)      //한줄한줄 읽어서 다읽으면 다음줄, 다음줄 
    {
        if (2 == _Encode)
            _CsvText = _CsvText.Replace("\t", ",");

        bool FindNewLine = false;
        int FindStartIndex = 0;
        int FindEndIndex = 0;

        List<string> list = new List<string>();

        for(int i=0; i< _CsvText.Length; ++i)
        {
            if ( _CsvText[i] == '"')
            {
                if (FindNewLine == false)
                {
                    FindStartIndex = i;
                    list.Add(_CsvText.Substring(FindEndIndex, FindStartIndex - FindEndIndex));
                    FindNewLine = true;
                }
                else if(FindNewLine == true)
                {
                    FindEndIndex = i + 1;

                    string parcing = _CsvText.Substring(FindStartIndex, FindEndIndex - FindStartIndex);

                    parcing = parcing.Replace("\n", "");
                    parcing = parcing.Replace("\n", "\\z");
                    list.Add(parcing);
                    FindNewLine = false;
                }
            }
        }

        if(list.Count > 0)
        {
            list.Add(_CsvText.Substring(FindEndIndex, _CsvText.Length - 1 - FindStartIndex));

            _CsvText = "";

            for(int i = 0; i < list.Count; ++i)
            {
                _CsvText += list[i];
            }
        }

        System.String[] lines = _CsvText.Split("\n"[0]);

        int width = 0;

        for(int i = 0;i < lines.Length; ++i)
        {
            System.String[] row = SplitCsvLine(lines[i]);
            width = UnityEngine.Mathf.Max(width, row.Length);
        }

        System.String[,] outputgrid = new System.String[width + 1, lines.Length + 1];

        for (int y = 0; y < lines.Length; y++)
        {
            lines[y] = lines[y].Replace("\n", "asdf!@#$");

            System.String[] row = SplitCsvLine(lines[y]);

            for (int x = 0; x < row.Length; x++)
            {
                row[x] = row[x].Replace("asdf!@#$", ",");

                outputgrid[x, y] = row[x];

                outputgrid[x, y] = outputgrid[x, y].Replace(@"\n", "\n");       //필요없을수도 있음.
                outputgrid[x, y] = outputgrid[x, y].Replace(@"\z", "\n");
                outputgrid[x, y] = outputgrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputgrid;
    }

    System.String[] SplitCsvLine(System.String _line)   //특수문자나 파일에 쓰는 규칙을 피할 수 있음. Csv한줄한줄 파싱(parsing)하는 규칙. 엑셀 2016까지 유효.
    {
        return (from System.Text.RegularExpressions.Match m in 
          System.Text.RegularExpressions.Regex.Matches(_line, @"(((?<x>(?=[,\r\n]
          +))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
          System.Text.RegularExpressions.RegexOptions.ExplicitCapture) select
          m.Groups[1].Value).ToArray();
    }
}
