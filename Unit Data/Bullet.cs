using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    GameObject Archer;
    GameObject WhiteWizard;
    GameObject BlackWizard;
    GameObject EnemyWizard;

    GameObject Now_Bullet;

    public float Bullet_Speed = 15;

    // Start is called before the first frame update
    void Start()
    {
        Archer = transform.GetChild(0).gameObject;
        WhiteWizard = transform.GetChild(1).gameObject;
        BlackWizard = transform.GetChild(2).gameObject;
        EnemyWizard = transform.GetChild(3).gameObject;
    }

    // 캐릭터 
    public void Shoot_Bullet(CharacterM origin ,EnemyM target)
    {
        StartCoroutine(Bullet_Move(origin, target));
    }

    //적
    public void Shoot_Bullet(EnemyM origin, CharacterM target)
    {
        StartCoroutine(Bullet_Move(origin, target));
    }

    IEnumerator Bullet_Move(CharacterM origin, EnemyM target)
    {
        if(origin.Mcharacter_info.character_name.Equals("Archer"))
        {
            Now_Bullet = Archer;
        }
        else if (origin.Mcharacter_info.character_name.Equals("White Wizard"))
        {
            Now_Bullet = WhiteWizard;
        }
        else if (origin.Mcharacter_info.character_name.Equals("Black Wizard"))
        {
            Now_Bullet = BlackWizard;
        }
        else if (origin.Mcharacter_info.character_name.Equals("Enemy Wizard"))
        {
            Now_Bullet = EnemyWizard;
        }

        Now_Bullet.SetActive(true);

        while (true)
        {
            if((transform.position - target.transform.position).sqrMagnitude<0.3f)
            {
                target.GetHit(origin.Mcharacter_info.character_Damage);
                origin.On_hit_bullet = true;
                Now_Bullet.SetActive(false);
                transform.localPosition = origin.mBullet_POS;
                origin.Mana += origin.Mcharacter_info.character_VaryMP;
                break;
            }
            else
            {
                transform.LookAt(target.transform);
                transform.Translate(Vector3.forward * Time.deltaTime * Bullet_Speed);
            }
            yield return null;
        }
    }

    IEnumerator Bullet_Move(EnemyM origin, CharacterM target)
    {
         if (origin.Menemy_info.enemy_name.Equals("Enemy Wizard"))
        {
            Now_Bullet = EnemyWizard;
        }

        Now_Bullet.SetActive(true);

        while (true)
        {
            if ((transform.position - target.transform.position).sqrMagnitude< 0.3f)
            {
                target.GetHit(origin.Menemy_info.enemy_damage);
                origin.On_hit_bullet = true;
                Now_Bullet.SetActive(false);
                transform.localPosition = origin.mBullet_POS;
                origin.Mana += origin.Menemy_info.enemy_VaryMP;
                break;
            }
            else
            {
                transform.LookAt(target.transform);
                transform.Translate(Vector3.forward * Time.deltaTime * Bullet_Speed);
            }
            yield return null;
        }
    }
}
