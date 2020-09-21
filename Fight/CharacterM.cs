using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class CharacterM : Battle
{
     // 캐릭터 ui
    public GameObject Now_Bar;
    [HideInInspector]
    public Transform Now_Bar_Hp;
    [HideInInspector]
    public Transform Now_Bar_Cover;
    [HideInInspector]
    public Transform Now_Bar_Mp;
    public GameObject OneStar_Bar=null;
    public GameObject TwoStar_Bar=null;
    [HideInInspector]
    public GameObject mCollider = null;

    // 캐릭터 정보
    public Character Mcharacter_info = null;
    public GameObject MyTarget = null;
    public Transform mObjTrans = null;

    public Animator mAnimator;
    private AudioSource mAudio;
    public AudioClip mLevelup_clip;
    private GameObject mLevelup_effect;


    // 캐릭터 스킬 관련
    public GameObject mSkillObj = null;
    public bool Skill_Switch = false;

    // 전투/이동 코루틴
    Coroutine Next_Move;
    Coroutine While_Fight;

    //캐릭터 모델링
    [HideInInspector]
    public Dictionary<string, GameObject> Character_Models = new Dictionary<string, GameObject>();
    [HideInInspector]
    public GameObject mModel = null;
    [HideInInspector]
    public Bullet mBullet = null;
    [HideInInspector]
    public Vector3 mBullet_POS;

    // 보드에 올라가 있는 경우
    public bool mOn_board = false;
    public bool On_board
    {
        get { return mOn_board; }

        set
        {
            mOn_board = value;

            if (mOn_board.Equals(true))
            {
                Now_Bar.SetActive(true);
                Debug.Log("on_board");
            }
            else
            {
                Now_Bar.SetActive(false);
            }
        }
    }


    // 전투 수치
    public float Vary_Skill_Value = 1f;    // 데미지 변동치
   public  float Range;
   public  bool Long_attack;
    public bool On_hit_bullet = false;

    private bool is_Unit_init = false;
    public bool Is_Dead = false;

    // 현재 체력
    private float Current_HP;
    public float Health
    {
        get { return Current_HP; }
        set
        {
            Current_HP = value;
            Update_HP();
        }
    }

    //현재 마나
    private float Current_MP=0;
    public float Mana
    {
        get
        {
            return Current_MP;
        }

        set
        {
            Current_MP = value;
            Update_MP();
        }
    }

    // 체력/마나 퍼센트
    public float Health_per;
    public float Mana_per;

    float Health_cover;
    bool Health_Flag;
    Coroutine Health_coru;

    // 캐릭터 레벨
    public int mCharacter_Level = Unit_Level.Level_1;
    public int mLevel
    {
        get { return mCharacter_Level; }
        set
        {
            mCharacter_Level = value;

            //if(mModel==null)
            //{
            //    mCharacter_obj = transform.GetChild(1).gameObject;
            //    mUI_object = transform.GetChild(0).gameObject; 
            //}

            if (mCharacter_Level.Equals(Unit_Level.Level_2))
            {
                Debug.Log("레벨2");

                //이전 바  비활성
                Now_Bar.gameObject.SetActive(false);

                //1112 레벨업 이펙트
                mLevelup_effect.SetActive(true);

                //1112 레벨업 사운드
                mAudio.clip = mLevelup_clip;
                mAudio.Play();

                Mcharacter_info.character_Damage *= 1.5f;
                Mcharacter_info.character_MaxHp *= 1.5f;

                // 연결
                Now_Bar = TwoStar_Bar;
                Now_Bar_Hp = Now_Bar.transform.GetChild(2).transform;
                Now_Bar_Mp = Now_Bar.transform.GetChild(3).transform;
                Now_Bar_Cover = Now_Bar.transform.GetChild(4).transform;

                // 보드에 올라가 있는 경우 켜주기
                if (mOn_board == true)
                {
                    Now_Bar.gameObject.SetActive(true);
                }
            }

            //if (mCharacter_Level.Equals(Unit_Level.Level_3))
            //{
            //    Debug.Log("레벨3");

            //    // mCharacter_obj.transform.localScale += new Vector3(0.03f, 0.03f, 0.03f);
            //    // mCharacter_obj.transform.localRotation = Quaternion.identity;
            //}
        }

    }

    // 애니메이션 
    public int mAniState = Animator.StringToHash("Ani_State");

    // 캐릭터 스테이트
    public int Character_state;

    //코루틴 델리게이트
    delegate IEnumerator  mSKILL();
    mSKILL Now_Skill = null;

    public ObjectPoolManager ObjPool_MGR = null;
    EnemyM mTarget;


    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    void Update()
    {
        //Debug.Log("이동 코루틴 : " + Next_Move + "전투 코루틴" + While_Fight);
        if (On_board)
        {
            Up_To_Healthbar();

            if (GameMGR._state == Board_Define.GAME_READY)
            {
                Unit_init();
            }

            else if (GameMGR._state == Board_Define.GAME_BATTLE)
            {


                //탐색       ==========================================
                if (Character_state == Unit_Battle_State.FINDING)
                {
                    Set_Ani(Ani_Define.IDLE);
                    mFind_Target();
                }


                //이동       ==========================================
                else if (Character_state == Unit_Battle_State.MOVE)
                {
                    if (Next_Move == null)
                    {
                        Next_Move = StartCoroutine(Move_nextPos());
                    }
                }


                //공격       ==========================================
                else if (Character_state == Unit_Battle_State.ATTACK)
                {
                    if (While_Fight == null)
                    {
                        While_Fight = StartCoroutine(MinusHealth_c());
                    }
                }

                else if (Character_state == Unit_Battle_State.DIE)
                {
                    StartCoroutine(Die_Ani());
                }


            }

            else if (GameMGR._state == Board_Define.GAME_BATTEL_END)
            {
                is_Unit_init = false;
                Set_Ani(Ani_Define.VICTORY); // 승리포즈
            }
        }
    }

    #region 초기화/ui


    public void Init()
    {
        // Ui캐싱
        OneStar_Bar = transform.GetChild(1).gameObject;
        TwoStar_Bar = transform.GetChild(2).gameObject;
        Now_Bar = OneStar_Bar;
        Now_Bar_Hp = Now_Bar.transform.GetChild(2).transform;
        Now_Bar_Mp = Now_Bar.transform.GetChild(3).transform;
        Now_Bar_Cover = Now_Bar.transform.GetChild(4).transform;

        mSkillTime = 0;

        // mObjTrans = mModel.transform;
        mCollider = transform.GetChild(0).gameObject;
        mBullet = transform.GetChild(3).transform.GetChild(0).gameObject.GetComponent<Bullet>();
        mBullet_POS = transform.GetChild(4).transform.localPosition;

        mAudio = GetComponent<AudioSource>();
        mLevelup_effect = transform.GetChild(13).gameObject;

        //스테이트 초기화
        Character_state = Unit_Battle_State.READY;

    }

    private void Unit_init()
    {

        if (!is_Unit_init)
        {
            // 캐싱
            mAnimator = mModel.GetComponent<Animator>();
            mSkillObj = mModel.transform.GetChild(0).gameObject;
            // 수치 초기화
            Health = Mcharacter_info.character_MaxHp;
            Now_Bar_Cover.localScale = Now_Bar_Hp.localScale;
            Mana = 0;
            Is_Dead = false;
            Skill_Init();

            if (Next_Move != null)
            {
                StopCoroutine(Next_Move);
                Next_Move = null;
            }

            if (While_Fight != null)
            {
                StopCoroutine(While_Fight);
                While_Fight = null;
            }

            MyTarget = null;
            Set_Ani(Ani_Define.IDLE);
            Character_state = Unit_Battle_State.FINDING;
            mPath = null;
            Range = Mcharacter_info.character_AtkRange;

            // 보스전
            if (GameMGR.Game_round.Equals(Board_Define.BossStage))
            {
                if (Range > Attack_Type.Short_Range)
                {
                    Long_attack = true;
                    Range += 4;
                }
                else
                {
                    Long_attack = false;
                    Range += 4;
                }
            }

            // 일반전
            else
            {
                if (Range > Attack_Type.Short_Range)
                {
                    Long_attack = true;
                }
                else
                {
                    Long_attack = false;
                }
            }





            //끝
            Now_Bar.SetActive(true);
            gameObject.SetActive(true);
            is_Unit_init = true;
        }
    }

    // ui 위치 업데이트
    private void Up_To_Healthbar()
    {
        Now_Bar.transform.position = mModel.transform.position + new Vector3(0, 1.4f, 0);
        mCollider.transform.position = Now_Bar.transform.position + new Vector3(0, 0.5f, 0); ;
        Now_Bar.transform.LookAt(Camera.main.transform.position);
        Now_Bar.transform.rotation = Quaternion.Euler(Camera.main.gameObject.transform.eulerAngles.x, 0, 0);
    }

    // 캐릭터 정보 업데이트
    public void Character_info_update(Character info_)
    {
        Mcharacter_info.chracter_initial(info_);
    }

    // 애니메이션 설정
    public void Set_Ani(int flag)
    {
        if (mAnimator == null)
        {
            mAnimator = mModel.GetComponent<Animator>();
        }
        mAnimator.SetInteger(mAniState, flag);
    }




    #region 체력_마나_업데이트
    public void Update_HP()
    {
        // 체력의 수치에 대한 백분율을 기존체력대비 현재 체력으로 업데이트 시켜준다.
        Health_per = Current_HP / Mcharacter_info.character_MaxHp * 100;

        Health_cover = Health_per;


        // 체력바 스프라이트에 체력에 대한 백분율을 적용하여 랜더링 해주는 함수
        SetSize_HP(Health_per / 100f);

        Health_Flag = true;

        if(Health_coru==null)
        {
            Health_coru = StartCoroutine(Set_SIZE_HP_Cover());
        }


        if (Current_HP <= 0)
        {
            if (Next_Move != null)
            {
                StopCoroutine(Next_Move);
                Next_Move = null;
            }

            if (While_Fight != null)
            {
                StopCoroutine(While_Fight);
                While_Fight = null;
            }

            Is_Dead = true;
            now_node.bIsWall = true;
            Character_state = Unit_Battle_State.DIE;
            Now_Bar.SetActive(false);
        }
    }

    IEnumerator Set_SIZE_HP_Cover()
    {
        yield return null;

        while (true)
        {
            if (Current_HP <= 0)
            {
                Health_coru = null;
                yield break;
            }
            else
            {
                if (GameMGR._state == Board_Define.GAME_BATTEL_END)
                {
                    Health_coru = null;
                    yield break;
                }

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

    public void Update_MP()
    {
        SetSize_MP(Current_MP / 100f);
    }

    public void SetSize_HP(float sizeNormalized)
    {
        Now_Bar_Hp.localScale = new Vector3(sizeNormalized, 1f);
    }

    public void SetSize_MP(float sizeNormalized)
    {
        Now_Bar_Mp.localScale = new Vector3(sizeNormalized, 1f);
    }

    public Node Out_Node()
    {
        RaycastHit hit;
        if (Physics.Raycast(mCollider.transform.position, Vector3.down, out hit, 3f, LayerMask.GetMask("ChessPlane")))
        {
            return hit.collider.transform.GetComponent<Node>();
        }
        return null;
    }

    #endregion

    #endregion

    #region 탐색
    //// 타켓 탐색
    public void mFind_Target()
    {
        // 최단거리 타겟 검색
        MyTarget = Find_Target(GameMGR.AliveEnemy, transform);

        // 경로 탐색
        if (MyTarget != null)
        {
           // if (Vector3.Distance(transform.position, MyTarget.transform.position) <  Range)
            if (Vector3.Distance(transform.position,MyTarget.transform.position)<  Range)
            {
                Character_state = Unit_Battle_State.ATTACK;
                return;
            }

            mFind_Path();
        }
        Debug.Log(MyTarget);

    }

    // 경로 탐색
    public void mFind_Path()
    {
        mPath = FindPath(now_node, MyTarget.GetComponent<EnemyM>().now_node);
        Character_state = Unit_Battle_State.MOVE;

        if (Next_Move != null)
        {
            StopCoroutine(Next_Move);
            Next_Move = null;
        }

    }

    public void mFind_Path_detour(Node detour_detour)
    {
        mPath = FindPath(now_node, MyTarget.GetComponent<EnemyM>().now_node);
        Character_state = Unit_Battle_State.MOVE;
    }


    // 모든 적 거리 탐색
    public void Search_Distance()
    {
        float now_distance = Vector3.Distance(transform.position,MyTarget.transform.position);
        float tmp_distance = 0;

        for (int i = 0; i < GameMGR.AliveEnemy.Count; i++)
        {
            tmp_distance = Vector3.Distance(transform.position,GameMGR.AliveEnemy[i].transform.position);

            if (tmp_distance < Range)
            {
                MyTarget = GameMGR.AliveEnemy[i];
                Character_state = Unit_Battle_State.ATTACK;

                if (Next_Move != null)
                {
                    StopCoroutine(Next_Move);
                    Next_Move = null;
                }
                return;
            }

            if (tmp_distance < now_distance)
            {
                Debug.Log("타겟 변경");
                MyTarget = GameMGR.AliveEnemy[i];
                mFind_Path();
                return;
            }
        }
        mFind_Path();
    }
    #endregion

    #region 이동

    //이동
    IEnumerator Move_nextPos()
    {
        float Target_Distance;
        now_node.bIsWall = true;

        while (true)
        {
            if (mPath != null)
            {
                if (mPath.Count == 1)
                {
                    Search_Distance();
                }

                if (!mPath[path_index].Equals(now_node))
                {
                    if (mPath[path_index].bisEnemy)
                    {
                        // 가까운 적탐색
                        Search_Distance();
                    }

                    // 다음 노드가 캐릭터
                    else if (mPath[path_index].bisCharacter)
                    {
                        // 새로 경로 탐색
                        mFind_Path_detour(mPath[path_index]);
                    }
                    else
                    {
                        mPath[path_index].bisCharacter = true;
                    }
                }

                // 이동
                Set_Ani(Ani_Define.WALK);
                transform.LookAt(mPath[path_index].transform.position);
                transform.Translate(Vector3.forward * Time.deltaTime * 1.5f);
                Target_Distance = (transform.position - mPath[path_index].transform.position).sqrMagnitude;

                // 타겟 도착시
                if (mPath[path_index].Equals(now_node))
                {
                    mPath[path_index].bisCharacter = false;
                    Debug.Log("포인트 도착");
                    transform.position = mPath[path_index].transform.position;
                    Search_Distance();
                    yield break;
                }

                if (GameMGR._state == Board_Define.GAME_BATTEL_END)
                {
                    Next_Move = null;
                    yield return new WaitForSeconds(3f);
                    Character_state = Unit_Battle_State.READY;
                    yield break;
                }
            }
            yield return null;

        }
    }

    #endregion

    #region  전투
    //전투
    IEnumerator MinusHealth_c()
    {
        Debug.Log(mPath);

        if (mPath !=(null))
        {
            // 이동중에서 왔을경우
                float now_distance = (transform.position-now_node.transform.position).sqrMagnitude;
                float next_distance = (transform.position-mPath[path_index].transform.position).sqrMagnitude;
                transform.position = now_distance > next_distance ? mPath[path_index].transform.position : now_node.transform.position;
                mPath = null;
        }
        if (Next_Move != null)
        {
            StopCoroutine(Next_Move);
            Next_Move = null;
        }

        mTarget = MyTarget.GetComponent<EnemyM>();
        now_node.bIsWall = false;

        while (true)
        {
            if (GameMGR._state == Board_Define.GAME_BATTLE)
            {
                // 타겟이 살아있는 경우
                if (mTarget.Is_Dead == false)
                {
                    now_node.bisCharacter = true;
                    transform.LookAt(MyTarget.transform);

                    if (MyTarget != null)
                    {
                        // 나의 공격력만큼 타겟의 체력을 깎아라
                        if (Current_MP >= 100)
                        {
                            Current_MP = 0;
                            Skill_Switch = true;

                            // 스킬 애니메이션 실행
                            while (true)
                            {
                                Set_Ani(Ani_Define.SKILL);

                                if(Skill_Switch == false)
                                {
                                    break;
                                }
                                yield return null;
                            }
                        }
                        else
                        {
                            Set_Ani(Ani_Define.ATTACK);
                            Mana += Mcharacter_info.character_VaryMP;
                            yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

                            if (Long_attack)
                            {
                                while (true)
                                {
                                    Set_Ani(Ani_Define.IDLE);

                                    if(On_hit_bullet)
                                    {
                                        On_hit_bullet = false;
                                        break;
                                    }
                                    yield return null;
                                }
                            }
                            
                        }
                    }
                }

                else
                {
                    now_node.bisCharacter = false;
                    MyTarget = null;
                    While_Fight = null;
                    Character_state = Unit_Battle_State.FINDING;
                    yield break;
                }
            }
            else if (GameMGR._state == Board_Define.GAME_BATTEL_END)
            {
                mSkillObj.SetActive(false);
                mSKillBox = null;
                now_node.bIsWall = true;
                Character_state = Unit_Battle_State.READY;
                MyTarget = null;
                While_Fight = null;
                is_Unit_init = false;
                yield break;
            }
        }
    }

    // 데미지 입음
   public  void GetHit(float value)
    {
        //mAudio.Play();
        Health -= value * Vary_Skill_Value;

        //마나가 100을 넘었을경우 100으로 고정, 100을 넘을경우 마나바가 너무 길어짐 
        if (Mana + Mcharacter_info.character_VaryMP > 100)
        {
            Mana = 100;
        }
    }

    // 애니메이션 동작시 작동
    public override void Short_Attack()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            mTarget.GetHit(Mcharacter_info.character_Damage);
        }
    }

    // Bullet을 쏘는 타이밍을 잡아보자!
    public override void Long_Attack()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            mBullet.Shoot_Bullet(this, mTarget);
        }
    }
    #endregion

    #region 스킬

    public override void Use_Skill()
    {
        if (Now_Skill != null)
        {
            StartCoroutine(Now_Skill());
        }

        if (Mcharacter_info.character_index == 2)
        {
            mSkillTime = 5f;
        }
        Mana = 0;
        Update_MP();
    }

    private void Skill_Init()
    {
        // 유닛의 정보에 따라 사용하는 스킬을 바꿈
        switch (Mcharacter_info.character_index)
        {
            // 각 유닛의 스킬 작동
            case 0: // Archer
                {
                    Now_Skill = new mSKILL(Archer_skill);
                    break;
                }

            case 1: //Martial arts
                {
                    Now_Skill = new mSKILL(MartialArts_skill);
                    break;
                }

            case 2: //Warrior
                {
                    Now_Skill = new mSKILL(Warrior_skill);
                    break;
                }

            case 3: // White Wizard
                {
                    Now_Skill = new mSKILL(WhiteWizard_skill);
                    break;
                }

            case 4: //Black Wizard
                {
                    Now_Skill = new mSKILL(BlackWizard_skill);
                    break;
                }

            case 5: // Difencer
                {
                    Now_Skill = new mSKILL(Difencer_skill);
                    break;
                }

            case 6: //Sword Master
                {
                    if (Now_Skill == null)
                    {
                        Now_Skill = new mSKILL(SwordMaster_skill);
                    }

                    break;
                }

            case 7: // King
                {
                    Now_Skill = new mSKILL(King_skill);
                    break;
                }
        }
    }


     
    ////////////////////////////////////////////////////////////////////////////////////

    #region Archer_skill 함수
    IEnumerator Archer_skill()
    {
        ObjPool_MGR = transform.parent.GetComponent<BoardManager>().PoolMGR;
        mSkillObj.SetActive(true);
        yield return null;

        mSkillTime = 0;

        // 활 시위 당기는 애니메이션만큼 기다려라
        yield return new WaitForSeconds(1.5f);

        // 활성화 비활성화 처리
        mSkillObj.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
        mSkillObj.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);

        ObjPool_MGR.Pop_Pooling(this);

        mSkillObj.SetActive(false);
        mSkillObj.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        mSkillObj.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
        Skill_Switch = false;
        yield break;
    }
    #endregion

    #region MartialArts_skill 함수
    IEnumerator MartialArts_skill()
    {
        mSkillObj.SetActive(true);

        yield return null;

        // 캐릭터의 체력을 캐싱
        float MaxHP = Mcharacter_info.character_MaxHp;
        float HealValue = 0;

        while (true)
        {
            Debug.Log("Health : " + Health + " / MaxHP : " + MaxHP + " / HealValue : " + HealValue);

            // 현재 체력이 최대 체력을 넘으면 코루틴을 종료
            if (Health >= MaxHP)
            {
                Debug.Log("최대체력 넘었어!! / 코루틴 종료");
                //mSkillObj.SetActive(false);
                //Skill_Switch = false;
                Health = MaxHP;
                //yield break;
            }

            // 체력을 회복해라
            Health += 0.55f;
            HealValue += 0.55f;

            if (HealValue >= 40)
            {
                Debug.Log("회복 다했어!!! / 코루틴 종료");

                mSkillObj.SetActive(false);
                Skill_Switch = false;
                yield break;
            }
            yield return null;
        }
    }
    #endregion

    #region Warrior_skill 함수
    IEnumerator Warrior_skill()
    {
        mSkillObj.SetActive(true);

        mSkillTime = 5f;
        Debug.Log("Warrior_skill / Enter");

        Skill_Switch = false;
        yield return null;

        // 나의 공격력을 1.5배로 늘린다. 워리어의 일반 데미지는 10이다.
        Mcharacter_info.character_Damage = 15f;
        Debug.Log("++ 공격력 추가 / character_Damage : " + Mcharacter_info.character_Damage);

        yield return new WaitForSeconds(mSkillTime);
        mSkillObj.SetActive(false);

        // 버프 효과 이후
        Mcharacter_info.character_Damage = 10f;
        Debug.Log("++ 공격력 되돌리기 / character_Damage : " + Mcharacter_info.character_Damage);

        yield break;
    }
    #endregion

    #region WhiteWizard_skill 함수
    IEnumerator WhiteWizard_skill()
    {
        mSkillObj.SetActive(true);
        // 나중에 스킬 핸들링 들어갈때, mSkillObj안에 자식들(주문진, 맞는 타겟 연기 등등)을
        // 활성화 하고 비활성화 하는 방법으로 조금 더 완성된 퀄리티로 만들기.

        yield return null;
        Debug.Log("WhiteWizard_skill / Enter");

        GameObject Effect = mSkillObj.transform.GetChild(0).transform.GetChild(1).gameObject;

        // 나의 타겟
        Node EnemyNowNode = MyTarget.GetComponent<EnemyM>().now_node;

        List<GameObject> aliveEnemyList = GameMGR.AliveEnemy;

        // 타겟 주변의 노드를 가져와
        List<Node> NodeList = GridMGR.GetNeighboringNodes(EnemyNowNode);

        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < NodeList.Count; i++)
        {
            Node indexNode = NodeList[i];

            for (int j = 0; j < aliveEnemyList.Count; j++)
            {
                GameObject aliveEnemy = aliveEnemyList[j];

                if (aliveEnemy.GetComponent<EnemyM>().now_node.Equals(indexNode))
                {
                    aliveEnemy.GetComponent<EnemyM>().GetHit(100);
                }
            }
        }
        MyTarget.GetComponent<EnemyM>().GetHit(100);

        Effect.transform.position = MyTarget.transform.position;
        Effect.SetActive(true);

        Skill_Switch = false;

        yield return new WaitForSeconds(2f);

        Effect.SetActive(false);
        mSkillObj.SetActive(false);
        yield break;
    }
    #endregion

    #region BlackWizard_skill 함수
    IEnumerator BlackWizard_skill()
    {

        Debug.Log("My Name is:" + gameObject.name + " / I have a:" + mSkillObj.name);
        yield return null;
        Debug.Log("BlackWizard_skill / Enter");

        // 나의 타겟
        Node EnemyNowNode = MyTarget.GetComponent<EnemyM>().now_node;

        List<GameObject> aliveEnemyList = GameMGR.AliveEnemy;

        // 타겟 주변의 노드를 가져와
        List<Node> NodeList = GridMGR.GetNeighboringNodes(EnemyNowNode);

        // 나의 타겟의 위치에 mSkillObj를 보내놓고 활성화를 시킨다.
        Debug.Log("MyPosition : " + gameObject.transform.position + " / Target : " + MyTarget.transform.position);
        mSkillObj.transform.position = MyTarget.transform.position;
        mSkillObj.SetActive(true);

        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        for (int i = 0; i < NodeList.Count; i++)
        {
            Node indexNode = NodeList[i];

            for (int j = 0; j < aliveEnemyList.Count; j++)
            {
                GameObject aliveEnemy = aliveEnemyList[j];

                if (aliveEnemy.GetComponent<EnemyM>().now_node.Equals(indexNode))
                {
                    aliveEnemy.GetComponent<EnemyM>().GetHit(100);
                }
            }
        }
        MyTarget.GetComponent<EnemyM>().GetHit(100);
        Skill_Switch = false;




        mSkillObj.SetActive(false);
        yield break;
    }
    #endregion

    #region Difencer_skill 함수
    IEnumerator Difencer_skill()
    {
        mSkillObj.SetActive(true);

        Debug.Log(mSkillObj.name);
        Debug.Log("Difencer_skill / Enter");
        yield return null;

        // 피해량 70% 감소
        Vary_Skill_Value = 0.3f;
        Debug.Log("MySkill_Value : " + Vary_Skill_Value);

        yield return new WaitForSeconds(1f);

        Skill_Switch = false;

        yield return new WaitForSeconds(4f);

        // 버프 효과 이후
        Vary_Skill_Value = 1f;
        Debug.Log("MySkill_Value : " + Vary_Skill_Value);

        mSkillObj.SetActive(false);
        yield break;
    }
    #endregion

    #region SwordMaster_skill 함수
    IEnumerator SwordMaster_skill()
    {
        yield return null;
        mSkillObj = mModel.transform.GetChild(45)
            .transform.GetChild(1)
            .transform.GetChild(0)
            .transform.GetChild(0).gameObject;

        mSkillTime++;

        mSkillObj.SetActive(true);

        yield return new WaitForSeconds(1.8f);
        Debug.Log(mSkillObj.name);
        
        
        mSkillObj.SetActive(false);
        Skill_Switch = false;
        yield break;
    }


    #endregion

    #region King_skill 함수
    // WhiteWizard 타겟을 나로 바꾸고 주변 노드만 가져와서 때리면 됨
    IEnumerator King_skill()
    {
        mSkillObj.SetActive(true);

        Debug.Log(mSkillObj.name);
        Debug.Log("King_skill / Enter");

        // 나의 타겟 노드를 나의 노드로
        Node EnemyNowNode = now_node;

        List<GameObject> aliveEnemyList = GameMGR.AliveEnemy;

        // 타겟 주변의 노드를 가져와
        List<Node> NodeList = GridMGR.GetNeighboringNodes(EnemyNowNode);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < NodeList.Count; i++)
        {
            Node indexNode = NodeList[i];

            for (int j = 0; j < aliveEnemyList.Count; j++)
            {
                GameObject aliveEnemy = aliveEnemyList[j];

                if (aliveEnemy.GetComponent<EnemyM>().now_node.Equals(indexNode))
                {
                    aliveEnemy.GetComponent<EnemyM>().GetHit(100);
                }
            }
        }
        mSkillObj.SetActive(false);
        Skill_Switch = false;
        yield break;
    }
    #endregion



    #endregion

    #region 사망

    IEnumerator Die_Ani()
    {
        mSkillObj.SetActive(false);
        Set_Ani(Ani_Define.DIE);
        //yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);
        yield return new WaitForSeconds(1.5f);

        GameMGR.Positioned_Character_Alive.Remove(gameObject);
        GameMGR.Alive_Character_Num--;
        gameObject.SetActive(false);
        is_Unit_init = false;
        gameObject.SetActive(false);
        Character_state = Unit_Battle_State.READY;
    }

    #endregion

}