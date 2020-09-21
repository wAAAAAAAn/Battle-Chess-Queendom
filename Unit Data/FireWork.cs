using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWork : MonoBehaviour
{
    public float MoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //Follow_WOTCM();
    }

    public void Init()
    {
        MoveSpeed = 10f;
    }

    public IEnumerator Follow_WOTCM(GameObject wotcm_)
    {
        yield return null;


        while (true)
        {
            if ((transform.position - wotcm_.transform.position).sqrMagnitude < 0.3f)
            {
                wotcm_.GetComponent<WorldOfTheCuteMouse>().MasterKey.Game_MGR.Player_HP -= 5;

                if (wotcm_.GetComponent<WorldOfTheCuteMouse>().WOTCM_Health <= 0)
                {
                    wotcm_.GetComponent<WorldOfTheCuteMouse>().WOTCM_DIE();
                }
                else
                {
                    wotcm_.GetComponent<WorldOfTheCuteMouse>().GetHit();
                }

                transform.position = transform.parent.transform.position;
                gameObject.SetActive(false);
                break;
            }
            else
            {
                transform.LookAt(wotcm_.transform);
                transform.Translate(Vector3.forward * Time.deltaTime * MoveSpeed);
            }
            yield return null;
        }
    }
}
