using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot_storage : MonoBehaviour
{
    public int index;   // 슬롯 인덱스
    public Character info_; // 슬롯에 들어갈 캐릭터 정보
    private GameObject button_;
    protected BoardManager board_;

    private GameObject canvas_;
    protected Ui_manager ui_;
    public DataManager data_;

    public bool On_Character = false;

    public bool ExamineTile()
    {
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.up, out hit, 3f, LayerMask.GetMask("Character")))
        {
            On_Character = true;
            Debug.DrawLine(gameObject.transform.position, hit.point, Color.green);
        }
        else
        {
            On_Character = false;
        }
        return On_Character;
    }

}
