using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Reciver : MonoBehaviour
{
    public Battle Unit_;
    public GameManager GameMGR;

    public AudioSource audioSource;
    public AudioClip Attack_sound;
    public AudioClip Skill_sound;
    public AudioClip Skill2_sound;

    private void Start()
    {
        Debug.Log(transform.parent.name);
        if(transform.parent.CompareTag("Character")|| transform.parent.CompareTag("Enemy"))
        {
            Unit_ = transform.parent.GetComponent<Battle>();
            GameMGR = Unit_.GameMGR;
            audioSource = Unit_.GetComponent<AudioSource>();
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }

    }

    // 근거리 공격
    public void Attack_Reciver_Character()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            Unit_.Short_Attack();
        }
    }

    //원거리 공격
    public void Attack_Reciver_Long()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            Unit_.Long_Attack();
        }
    }

    // 근거리 공격 사운드 재생
    public void Attack_sound_play()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            audioSource.clip = Attack_sound;
            audioSource.Play();
        }

        if (gameObject.CompareTag("WOTCM"))
        {
            audioSource.clip = Attack_sound;
            audioSource.Play();
        }
    }

    // 근거리 스킬 사운드 재생
    public void Skill_sound_play()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            audioSource.clip = Skill_sound;
            audioSource.Play();
        }

        if (gameObject.CompareTag("WOTCM"))
        {
            audioSource.clip = Skill_sound;
            audioSource.Play();
        }

    }

    //근거리 스킬 사운드2 재생
    public void Skill2_sound_play()
    {

            audioSource.clip = Skill2_sound;
            audioSource.Play();

    }

    //스킬 시전
    public void Reciver_Skill()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            Unit_.Use_Skill();
        }
    }
}