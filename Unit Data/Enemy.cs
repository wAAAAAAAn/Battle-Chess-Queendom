using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
// 인스펙터 창을 보기위한 직렬화
public class Enemy
{
    public int enemy_index;     //적 고유번호
    public string enemy_name;   //적 이름
    public float eneny_hp;    //적 체력
    public float enemy_damage;    //적 공격력
    public float enemy_AtkRange;  //적 공격범위
    public int enemy_VaryMP;    // 적 마나변동치

    //캐릭터 생성자 1
    public Enemy()
    {
        enemy_index = -1;
        enemy_name = "empty";
        eneny_hp = -1;
        enemy_damage = -1;
        enemy_AtkRange = -1;
        enemy_VaryMP = -1;
    }

    //캐릭터 생성자 2
    public Enemy(int _index, string _name, int _hp, int _damage, float _atkrange, int _varymp)
    {
        enemy_index = _index;
        enemy_name = _name;
        eneny_hp = _hp;
        enemy_damage = _damage;
        enemy_AtkRange = _atkrange;
        enemy_VaryMP = _varymp;
    }

    public void enemy_initial(Enemy ch_)
    {
        enemy_index = ch_.enemy_index;
        enemy_name = ch_.enemy_name;
        eneny_hp = ch_.eneny_hp;
        enemy_damage = ch_.enemy_damage;
        enemy_AtkRange = ch_.enemy_AtkRange;
        enemy_VaryMP = ch_.enemy_VaryMP;
    }

    public void Enemy_delete()
    {
        enemy_index = -1;
        enemy_name = "empty";
        eneny_hp = -1;
        enemy_damage = -1;
        enemy_AtkRange = -1;
        enemy_VaryMP = -1;
    }

}

