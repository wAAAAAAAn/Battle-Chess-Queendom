using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _storage : MonoBehaviour
{

    //창고
    public GameObject Storage_Slot; // 창고 슬롯
    public Ui_manager UiMGR;
    public List<Slot_storage> mStorage_slots = new List<Slot_storage>();    // 창고 슬롯
    public bool[] mStorage_Exist = new bool[8];
    public bool[] storage_onCharacter = new bool[8];
    const int Storage_Max_num = 8;

    private void Start()
    {
        UiMGR = FindObjectOfType<Reference_of_MGR>().UI_MGR;
        initial_storage();
    }

    // 창고 초기화
    public void initial_storage()
    {
        for (int i = 0; i < Storage_Max_num; i++)
        {
            GameObject slot = Instantiate(Storage_Slot, new Vector3(0.505f * ((i + 1) * 2) - 0.5f, 0, 0.9f), Quaternion.identity, gameObject.transform);
            mStorage_slots.Add(slot.GetComponent<Slot_storage>());
        }
    }

    public void Examine_all_storage()
    {
        for (int i = 0; i < Storage_Max_num; i++)
        {
            mStorage_Exist[i] = mStorage_slots[i].ExamineTile();
        }

        // 나중에 따로 뺄수도 있는 부분, 순서의 오류가 생길 수 있음
        for (int i = 0; i < Storage_Max_num; i++)
        {
            if (mStorage_Exist[i] == false)
            {
                UiMGR.mEmptyPos = mStorage_slots[i].transform;
                break;
            }
            UiMGR.mEmptyPos = null;
        }
    }
}
