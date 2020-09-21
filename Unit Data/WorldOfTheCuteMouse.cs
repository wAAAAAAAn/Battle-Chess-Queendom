using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldOfTheCuteMouse : MonoBehaviour
{
    public GameManager Game_MGR = null;
    public BoardManager Board_MGR = null;
    public Ui_manager Ui_MGR = null;

    public Reference_of_MGR MasterKey = null;
    public Animator mAnimator = null;
    public Quaternion Init_Q = Quaternion.identity;
    public GameObject Alive_Enemy = null;
    public Coroutine WOTCM_Action = null;

    public bool Click_Switch = false;
    public Coroutine Click_Coru = null;
    public int mAniState = Animator.StringToHash("Ani_State");

    // 체력 업데이트 관련 변수들
    private float Health;
    public float WOTCM_Health
    {
        get { return Health; }
        set
        {
            Health = value;

            Update_HP();
        }
    }

    public float Health_cover;
    public bool Health_Flag;

    public Coroutine Health_coru = null;
    public GameObject Now_Bar;
    public Transform Now_Bar_Hp;
    public Transform Now_Bar_Cover;
    /////////////////////////////////////


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        Game_MGR = MasterKey.Game_MGR;
        Board_MGR = MasterKey.Board_MGR;
        Ui_MGR = MasterKey.UI_MGR;

        Init_Q = gameObject.transform.rotation;
        Click_Switch = false;

        // 체력 관련 초기화
        //Now_Bar = transform.parent.gameObject.transform.GetChild(1).gameObject;
        //Now_Bar_Hp = Now_Bar.transform.GetChild(2).transform;
        //Now_Bar_Hp = Now_Bar.transform.GetChild(4).transform;

        Health_Flag = false;

    }

    private void Update()
    {
        // 체력 관련 초기화
        WOTCM_Health = Game_MGR.Player_HP;
    }

    public void MouseInit()
    {
        gameObject.transform.rotation = Init_Q;
        Set_Ani(Mouse_Ani_Define.IDLE);
    }

    #region WOTCM_Health_Update
    public void Set_Ani(int flag)
    {
        if (mAnimator == null)
        {
            mAnimator = gameObject.GetComponent<Animator>();
        }
        mAnimator.SetInteger(mAniState, flag);
    }

    public void Update_HP()
    {
        Health_cover = WOTCM_Health;


        // 체력바 스프라이트에 체력에 대한 백분율을 적용하여 랜더링 해주는 함수
        SetSize_HP(WOTCM_Health / 100f);

        Health_Flag = true;

        if (Health_coru == null)
        {
            Health_coru = StartCoroutine(Set_SIZE_HP_Cover());
        }

        if (WOTCM_Health <= 0)
        {
            Now_Bar.SetActive(false);
        }
    }

    public void SetSize_HP(float sizeNormalized)
    {
        Now_Bar_Hp.localScale = new Vector3(sizeNormalized, 1f);
    }

    IEnumerator Set_SIZE_HP_Cover()
    {
        yield return null;

        while (true)
        {
            if (WOTCM_Health <= 0)
            {
                yield break;
            }
            else
            {
                if (Health_Flag == true)
                {
                    //Debug.Log("노란색줄여~");

                    yield return new WaitForSeconds(1f);
                    Now_Bar_Cover.localScale = Now_Bar_Hp.localScale;

                    Health_Flag = false;
                }
                else
                {
                    yield return null;
                }
            }
            yield return null;
        }
    }
    #endregion

    #region WOTCM 행동 로직

    #region WOTCM_SELL
    public IEnumerator WOTCM_SELL()
    {
        yield return null;

        MouseInit();

        Set_Ani(Mouse_Ani_Define.VICTORY);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        Set_Ani(Mouse_Ani_Define.IDLE);

        yield break;
    }
    #endregion

    #region WOTCM_CLICK
    public void WOTCM_CLICK_STAY(Transform target_)
    {
        // 클릭하고 있는 캐릭터를 LookAt한다
        Vector3 delta = target_.transform.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(delta);
        transform.rotation= Quaternion.Slerp(transform.rotation, lookRot, 7 * Time.deltaTime);

        Click_Switch = true;
    }

    public IEnumerator WOTCM_CLICK_END()
    {
        yield return null;

        Click_Switch = false;

        Set_Ani(Mouse_Ani_Define.ATTACK1);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        Set_Ani(Mouse_Ani_Define.IDLE);
        MouseInit();

        yield break;
    }
    #endregion

    #region WOTCM_ROUND_VICTORY
    public IEnumerator WOTCM_ROUND_VICTORY()
    {
        yield return null;

        Set_Ani(Mouse_Ani_Define.VICTORY);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        Set_Ani(Mouse_Ani_Define.IDLE);
        MouseInit();

        yield break;
    }
    #endregion

    #region WOTCM_ROUND_LOSE
    public IEnumerator WOTCM_ROUND_LOSE()
    {
        yield return null;

        for(int i = 0; i < Game_MGR.AliveEnemy.Count; i++)
        {
            Alive_Enemy = Game_MGR.AliveEnemy[i];

            // 남은 에네미의 수만큼 불꽃 피격 파티션 활성화
            GameObject Enemy_FireWork = Alive_Enemy.transform.GetChild(4).transform.GetChild(0).gameObject;
            Enemy_FireWork.SetActive(true);

            StartCoroutine(Enemy_FireWork.GetComponent<FireWork>().Follow_WOTCM(gameObject));
        }

        Set_Ani(Mouse_Ani_Define.IDLE);
        MouseInit();

        yield break;
    }
    #endregion

    #region WOTCM_INGAME_VICTORY
    IEnumerator WOTCM_INGAME_VICTORY()
    {
        yield return null;

        Set_Ani(Mouse_Ani_Define.VICTORY);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        Set_Ani(Mouse_Ani_Define.IDLE);
        MouseInit();

        yield break;
    }
    #endregion

    #region WOTCM_INGAME_DIE

    public void WOTCM_DIE()
    {
        StartCoroutine(WOTCM_INGAME_DIE());
    }

    public IEnumerator WOTCM_INGAME_DIE()
    {
        yield return null;

        Set_Ani(Mouse_Ani_Define.DIE);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);
    }
    #endregion

    #region WOTCM_GETHIT
    public void GetHit()
    {
        WOTCM_Action = StartCoroutine(WOTCM_GETHIT());
    }

    public IEnumerator WOTCM_GETHIT()
    {
        if(WOTCM_Health <= 0)
        {
            yield break;
        }

        yield return null;
        Debug.Log("GETHIT_ENTER1");

        Set_Ani(Mouse_Ani_Define.GETHIT);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        MouseInit();
        Debug.Log("GETHIT_ENTER2");
        yield break;
    }
    #endregion

    #endregion
}
