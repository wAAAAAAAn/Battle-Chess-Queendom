using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _shop : MonoBehaviour
{
    Reference_of_MGR ReferenceMGR;
    private GameManager GameMGR = null;
    private BoardManager BoardMGR = null;
    private Ui_manager UIMGR = null;
    private ObjectPoolManager PoolMGR = null;

    public GameObject shop_slot; //상점 슬롯
    public List<Slot_shop> shop_slots = new List<Slot_shop>(); //창고 슬롯 리스트
    private const int slot_count = 5; // 슬롯의 개수

    public void Start()
    {
        ReferenceMGR = FindObjectOfType<Reference_of_MGR>();
        GameMGR = ReferenceMGR.Game_MGR;
        BoardMGR = ReferenceMGR.Board_MGR;
        UIMGR = ReferenceMGR.UI_MGR;
        PoolMGR = ReferenceMGR.Pool_MGR;
    }

    // 상점 초기화
    public void initial_shop()
    {
        for (int i = 0; i < slot_count; i++)  
        {
            shop_slots.Add(Instantiate(shop_slot.gameObject,transform).GetComponent<Slot_shop>());
            shop_slots[i].index = i; // 슬롯에 인덱스 
            shop_slots[i].transform.SetParent(gameObject.transform);    // 슬롯 부모 설정
            shop_slots[i].transform.position = gameObject.transform.position;    // 슬롯 부모 설정
        }
        gameObject.transform.parent.gameObject.SetActive(false);    // 초기화 후 창 비활성화
    }

    // 활성/비활성
    public void onoff_shop()
    {
        if (GameMGR._state != Board_Define.GAME_MOVE)
        {
            if(GameMGR.Game_round > Board_Define.BossStage && GameMGR._state == Board_Define.GAME_BATTLE)
            {
                return;
            }
            
            gameObject.transform.parent.gameObject.SetActive(!gameObject.transform.parent.gameObject.activeSelf);
        }
    }

    //지정값에 의해 설정
    public void onoff_shop(bool flag)
    {
        gameObject.transform.parent.gameObject.SetActive(flag);
    }

    //오브젝트 풀 매니저로 교체 예정(09.28.wan)
    public void Reroll_shop()
    {
        GameMGR.Player_HP -= 10;
        for (int i = 0; i < slot_count; i++)
        {
            shop_slots[i].Reroll_slot();
        }
    }

   

    public void focusout_by_mouse()
    {
        if (GameMGR.Mouse_Type.Equals(Mouse_Mode.ON_CLICK))
        {
            Debug.Log("focusout_by_mouse");
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(GameMGR.mTouchPos), out hit, 50.0f, LayerMask.GetMask("UI")))
            {
                Ray ray = Camera.main.ScreenPointToRay(GameMGR.mTouchPos);
                Slot_shop slot_ = hit.collider.gameObject.GetComponent<Slot_shop>();

                Debug.DrawRay(ray.origin, hit.point, Color.magenta);

               // Debug.Log(slot_.mFocus + " / " + slot_.info_.character_index);

                if (slot_.mFocus.Equals(true) && !slot_.info_.character_index.Equals(-1))
                {
                    UIMGR.StorageCheck();
                    if(UIMGR.mEmptyPos!=null)
                    {
                        // slot_.mUnit.transform.GetChild(1).localScale -= new Vector3(0.8f, 0.8f, 0.8f);
                        slot_.mUnit.transform.GetChild(1).rotation = new Quaternion(0, 180, 0, 0);
                        slot_.mUnit.transform.rotation = Quaternion.identity;
                        BoardMGR.SpawnCharacter(slot_.info_, UIMGR.mEmptyPos);
                        PoolMGR.Push_Pooling(slot_.mUnit, slot_.info_);
                        GameMGR.LevelUp_Character_(slot_.info_);
                        GameMGR.Player_HP -= slot_.info_.character_rating * 5+5;  
                        slot_.info_.character_delete();
                        slot_.Class_image.sprite = slot_.Empty;
                        slot_.Price_image.sprite = slot_.Empty;
                        slot_.mUnit = null;
                    }
                }
                else
                {
                    slot_.mFocus = true;
                }

                for (int i = 0; i < shop_slots.Count; i++)
                {
                    if (shop_slots[i].Equals(slot_))
                    {
                        continue;
                    }
                    shop_slots[i].mFocus = false;
                }
            }
            else
            {
                for (int i = 0; i < shop_slots.Count; i++)
                {
                    shop_slots[i].mFocus = false;
                }
            }
        }
    }

}
