using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class spawn_check : MonoBehaviour
{
    public bool spawn_order;    // 선택한 지점에 캐릭터가 있는지를 나타내는 변수

    private Reference_of_MGR ReferenceMGR;
    private BoardManager BoardMGR;
    private GameManager GameMGR;
    public bool dispos_mode = false;


    private void Start()
    {
        ReferenceMGR = FindObjectOfType<Reference_of_MGR>();
        BoardMGR = ReferenceMGR.Board_MGR;
        GameMGR = ReferenceMGR.Game_MGR;
        spawn_order = true; // 기본적으로 생성이 가능한 상태
    }

    private void Update()
    {
        if (GameMGR.Mouse_Type == Mouse_Mode.ON_CLICK|| GameMGR.Mouse_Type== Mouse_Mode.ON_DRAG)
        {
            RaycastHit hit;
            if (Physics.Raycast(gameObject.transform.position, Vector3.up, out hit, 10f, LayerMask.GetMask("Character")))
            {
                spawn_order = false;

                if (dispos_mode == false)
                {
                    Debug.DrawLine(gameObject.transform.position, hit.point + new Vector3(0, 10, 10), Color.cyan);
                    BoardMGR.Dispose_unit = hit.collider.transform.parent.gameObject;
                }

            }
            else
            {
                if (dispos_mode == false)
                {
                    // board_.Dispose_unit = null;
                }

                spawn_order = true;
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
