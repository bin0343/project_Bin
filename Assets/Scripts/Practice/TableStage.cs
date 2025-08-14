using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableStage : TableBase
{
    [Serializable]
    public class Info       //csv파일 정보들
    {
        public int Id;
        public byte Type;
        public int Stat;
        public string Icon;
        public string Name;
        public string Dec;
        public int[] Monster = new int[5];
        public int RewardId;
        public string Prefab;
    }

    public Dictionary<int, Info> Dictionary = new Dictionary<int, Info>();

    public Info Get(int _Id)
    {
        if(Dictionary.ContainsKey(_Id))
            return Dictionary[_Id];

        return null;
    }

    public void Init_Binary(string _Name)
    {
        Load_Binary<Dictionary<int, Info>>(_Name, ref Dictionary);
    }

    public void Save_Binary(string _Name)
    {
        Save_Binary(_Name, Dictionary);
    }

    public void Init_Csv(string _name, int _StartRow, int _StartCol)
    {
        CsvReader reader = GetCsvReader(_name);

        for (int row = _StartRow; row < reader.Row; ++row)
        {
            Info info = new Info();

            if (Read(reader, info, row, _StartCol) == false)
                break;

            Dictionary.Add(info.Id, info);
        }
    }

    protected bool Read(CsvReader _Reader, Info _Info, int _Row, int _StartCol)
    {
        if (_Reader.ResetRow(_Row, _StartCol) == false)
            return false;

        _Reader.Get(_Row, ref _Info.Id);

        for (int i = 0; i < 5; ++i)
            _Reader.Get(_Row, ref _Info.Monster[i]);

        _Reader.Get(_Row, ref _Info.RewardId);
        _Reader.Get(_Row, ref _Info.Prefab);

        return true;
    }
}
