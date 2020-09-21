using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Slot_shop : MonoBehaviour
{
    public int index;   // 슬롯 인덱스

    private Reference_of_MGR ReferenceMGR;
    private BoardManager BoardMGR;
    private Ui_manager UIMGR;
    private DataManager DataMGR;
    private ObjectPoolManager PoolMGR;
    private GameManager GameMGR;

    public bool is_focusing = false;
    public bool mFocus = false;

    public Character info_; // 슬롯에 들어갈 캐릭터 정보
    public GameObject mUnit = null;
    public Animator mAnimator;
    public Image Class_image;
    public SpriteRenderer Price_image;
    private Sprite Short_sprite;
    private Sprite Long_sprite;
    public Sprite[] Level_Price_sprites = new Sprite[3];
    public Sprite Empty;

    private bool mCharacter_update;
    public bool Character_update
    {
        get { return mCharacter_update; }
        set
        {
            mCharacter_update = value;

            if (mCharacter_update == false)
            {
                Character_slot_update();
                mFocus = false;
                mCharacter_update = true;
            }
        }
    }

    private void Awake()
    {
        ReferenceMGR = FindObjectOfType<Reference_of_MGR>();
        BoardMGR = ReferenceMGR.Board_MGR;
        UIMGR = ReferenceMGR.UI_MGR;
        DataMGR = ReferenceMGR.Data_MGR;
        PoolMGR = ReferenceMGR.Pool_MGR;
        GameMGR = ReferenceMGR.Game_MGR;

        Class_image = transform.GetChild(1).GetComponent<Image>();
        Price_image = transform.GetChild(2).GetComponent<SpriteRenderer>();
        Short_sprite = Resources.Load<Sprite>("Sprite/c_Short");
        Long_sprite = Resources.Load<Sprite>("Sprite/c_Long");
        Empty = Resources.Load<Sprite>("Sprite/empty");

        Level_Price_sprites[0] = Resources.Load<Sprite>("Sprite/In_Low_Price");
        Level_Price_sprites[1] = Resources.Load<Sprite>("Sprite/In_Middle_Price");
        Level_Price_sprites[2] = Resources.Load<Sprite>("Sprite/In_High_Price");
    }

    private void Start()
    {


        Character_update = false;
    }

    private void Update()
    {
        Slot_focus_update();
    }

    public void Reroll_slot()
    {
        Character_update = false;
    }

    //프로퍼티로 옵저버 패턴(09.29.wan)
    private void Slot_focus_update()
    {
        if (info_.character_index != -1)
        {
            if (mFocus)
            {
                mAnimator.SetInteger("Ani_State", Ani_Define.ATTACK);
                Class_image.gameObject.SetActive(true);
                // 파티클 추가로 좀 더 포커스 느낌을 주는게 좋을거 같음 (09.28.wan)
            }
            else
            {
                mAnimator.SetInteger("Ani_State", Ani_Define.IDLE);
                Class_image.gameObject.SetActive(false);
            }
        }
    }


    private void Character_slot_update()
    {

        if (mUnit != null)
        {
            PoolMGR.Push_Pooling(mUnit, info_);
            mUnit = null;
        }

        if (mUnit == null)
        {


            info_.chracter_initial(GameMGR.Random_Character());
            mUnit = PoolMGR.Pop_Pooling(info_);

            if (info_.character_AtkType.Equals(Attack_Type.Short_attack))
            {
                Class_image.sprite = Short_sprite;
                Class_image.gameObject.SetActive(false);
            }

            else if (info_.character_AtkType.Equals(Attack_Type.Long_attack))
            {
                Class_image.sprite = Long_sprite;
                Class_image.gameObject.SetActive(false);
            }

            if (info_.character_rating.Equals(Unit_Level.Level_1 + 1))
            {
                Price_image. sprite = Level_Price_sprites[Unit_Level.Level_1];
            }

            else if (info_.character_rating.Equals(Unit_Level.Level_2 + 1))
            {
                Price_image.sprite = Level_Price_sprites[Unit_Level.Level_2];
            }

            else if (info_.character_rating.Equals(Unit_Level.Level_3 + 1))
            {
                Price_image.sprite = Level_Price_sprites[Unit_Level.Level_3];
            }



            Transform mModel = mUnit.GetComponent<CharacterM>().mModel.transform;
            mUnit.transform.parent = transform.GetChild(0).transform;
            mUnit.transform.position = transform.GetChild(0).transform.position;

            mModel.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            mUnit.transform.localRotation = Quaternion.identity;
            mModel.localRotation = new Quaternion(0, 180, 0, 0);
            mAnimator = mModel.GetComponent<Animator>();

        }
    }

}
