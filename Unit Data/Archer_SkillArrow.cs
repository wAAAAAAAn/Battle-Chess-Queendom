using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer_SkillArrow : MonoBehaviour
{
    float Bullet_Speed = 0;
    ObjectPoolManager ObjPool_MGR = null;
    bool Skill_Switch;

    private void Awake()
    {
        Bullet_Speed = 1.5f;
        ObjPool_MGR = transform.parent.transform.parent.GetComponent<ObjectPoolManager>();
        Skill_Switch = true;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(MoveArrow());
    }

    IEnumerator MoveArrow()
    {
        yield return null;

        while (true)
        {
            gameObject.transform.Translate(Vector3.forward * Time.deltaTime * Bullet_Speed);
            Debug.Log(transform.position);

            if (Skill_Switch == false)
            {
                break;
            }
            yield return null;
        }
        ObjPool_MGR.Push_Pooling(gameObject);
        Skill_Switch = true;
        yield break;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") == true)
        {
            other.gameObject.transform.parent.GetComponent<EnemyM>().GetHit(300f);
        }
        else if (other.CompareTag("Stadium") == true)
        {
            Skill_Switch = false;
        }
    }
}