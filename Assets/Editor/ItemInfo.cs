using System;
using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Portion,
    Food,
}

[CreateAssetMenu(fileName = "ItemInfo", menuName = "ScriptableObject/ItemInfo",order = 1)]
public class ItemInfo : ScriptableObject
{
    public string id = Guid.NewGuid().ToString().ToUpper();
    //고유 아이디
    public string name;
    //이름
    public Sprite icon;
    //아이콘
    public ItemType itemType;
    //타입
    public string info;
    //정보
    public int gold;
    //돈
}