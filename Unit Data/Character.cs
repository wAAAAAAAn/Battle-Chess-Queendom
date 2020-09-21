using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
// 인스펙터 창을 보기위한 직렬화
public class Character
{
    public int character_index;     //캐릭터 고유번호
    public string character_name;   //캐릭터 이름
    public int character_rating;    //캐릭터 등급
    public float character_MaxHp;    //캐릭터 최대 체력
    public float character_Damage;    //캐릭터 공격력
    public float character_AtkRange;  //캐릭터 타입
    public float character_AtkType;  //캐릭터 타입
    public int character_VaryMP;    //캐릭터 공격력


    //캐릭터 생성자 1
    public Character()
    {
        //Character_info_update();
    }

    public void Character_info_update()
    {
        character_index = -1;
        character_name = "empty";
        character_rating = -1;
        character_MaxHp = -1;
        character_Damage = -1;
        character_AtkRange = -1;
        character_AtkType = -1;
        character_VaryMP = -1;

    }

    public void Character_info_update(int _index, string _name, int _rating, int _hp, int _damage, float _atkrange, int _atktype, int _varymp)
    {
        character_index = _index;
        character_name = _name;
        character_rating = _rating;
        character_MaxHp = _hp;
        character_Damage = _damage;
        character_AtkRange = _atkrange;
        character_AtkType = _atktype;
        character_VaryMP = _varymp;


    }

    public void character_delete()
    {
        character_index = -1;
        character_name = "empty";
        character_rating = -1;
        character_MaxHp = -1;
        character_Damage = -1;
        character_AtkRange = -1;
        character_AtkType = -1;
        character_VaryMP = -1;

    }

    public void chracter_initial(Character ch_)
    {
        character_index = ch_.character_index;
        character_name = ch_.character_name;
        character_rating = ch_.character_rating;
        character_MaxHp = ch_.character_MaxHp;
        character_Damage = ch_.character_Damage;
        character_AtkRange = ch_.character_AtkRange;
        character_AtkType = ch_.character_AtkType;
        character_VaryMP = ch_.character_VaryMP;
    }
}

