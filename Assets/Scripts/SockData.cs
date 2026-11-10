
// ====================================================================================================================================================================
// Autor: Pedro Antonio Ramos
// Email: ing.ramospedro@gmail.com
// Propósito del script: Definir la estructura de datos (SockType y LevelData) que almacena las propiedades de las medias (ID, nombre, color, índice de canasto, sprite)
// y las variables de cada nivel.
// Fecha: 09/11/2026
// ====================================================================================================================================================================
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
    public Sprite customSprite; // Sprite con el patrón de diseño (Plano, Lunares o Rayas)
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