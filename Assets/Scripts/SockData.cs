using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SockType
{
    public string id;
    public string name;
    public Color color;
    public int basketIndex;
}

[Serializable]
public class LevelData
{
    public int level;
    public string name;
    public string[] types;
    public int sockCount;
    public float speed;
    public int quota;
    public int time;
    public string desc;
}