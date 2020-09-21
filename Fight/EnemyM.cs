using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyM : Battle
{
    // 캐릭터 ui
    public GameObject Boss_Healthbar;

    public GameObject Now_Bar;
    [HideInInspector]
    public Transform Now_Bar_Hp;
    [HideInInspector]
    public Transform Now_Bar_Cover;
    [HideInInspector]
    public Transform Now_Bar_Mp;

    public GameObject mShadow;

    public Enemy Menemy_info = null;
    public GameObject MyTarget = null;

    [HideInInspector]
    public Animator mAnimator;
    [HideInInspector]
    public Dictionary<string, GameObject> Enemy_Models = new Dictionary<string, GameObject>();

    [HideInInspector]
    public GameObject mModel = null;
    [HideInInspector]
    public GameObject mCollider = null;
    [HideInInspector]
    public Bullet mBullet = null;
    [HideInInspector]
    public Vector3 mBullet_POS;


    // 공격 종류
    private float Range;    // 공격 범위
    public bool On_hit_bullet = false;  // 원거리 공격시 적중 
    public bool Long_attack; // 원거리 공격 여부
    float Vary_Skill_Value = 1f;    // 데미지 변동치


    // 초기화 관련 변수
    bool is_Unit_init = false;  // 유닛 초기화 실행 여부 
    public bool Is_Dead = false;   // 사망 여부


    // 캐릭터 스킬 관련
    public GameObject mSkillObj = null;
    public bool Skill_Switch = false;
   
    // 코루틴
    Coroutine Next_Move;
    Coroutine While_Fight;

    //코루틴 델리게이트
    delegate IEnumerator mSKILL();
    mSKILL Now_Skill = null;

    [HideInInspector]
    public int mAniState = Animator.StringToHash("Ani_State");

    // 옵저버 패턴
    [SerializeField]
    public float Current_HP;
    public float Health
    {
        get { return Current_HP; }
        set
        {
            Current_HP = value;
            Update_HP();
        }
    }

    private float Current_MP = 0;
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

    // 수치를 퍼센트로 변환해 캐싱할 변수
    public float Health_per;
    public float Mana_per;

    float Health_cover;
    bool Health_Flag;
    Coroutine Health_coru;

    public int Enemy_state = Unit_Battle_State.READY;

    private void OnEnable()
    {
        if(Now_Bar!=null)
        {
            Now_Bar.SetActive(true);
        }

        is_Unit_init = false;
    }

    CharacterM mTarget;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (Menemy_info.enemy_index != 3)
        {
            Up_To_Healthbar();
        }

        if (GameMGR._state == Board_Define.GAME_READY)
        {
            Unit_Init();
        }

        else if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            if (Menemy_info.enemy_index == 3)
            {
                Now_Bar.gameObject.SetActive(true);
            }

            if (Enemy_state == Unit_Battle_State.READY)
            {
                Set_Ivisible(false);

                Enemy_state = Unit_Battle_State.FINDING;
            }


            // 탐색       ==========================================
            if (Enemy_state == Unit_Battle_State.FINDING)
            {
                Set_Ani(Ani_Define.IDLE);
                mFind_Target();
            }


            //이동       ==========================================
            else if (Enemy_state == Unit_Battle_State.MOVE)
            {
                Move_pos();
            }


            //공격       ==========================================
            else if (Enemy_state == Unit_Battle_State.ATTACK)
            {
                if (While_Fight == null)
                {
                    Debug.Log("적 어택 코루틴 실행");
                    While_Fight = StartCoroutine(MinusHealth_e());
                }
            }

            else if (Enemy_state == Unit_Battle_State.DIE)
            {
                StartCoroutine(Die_Ani());
            }
        }

        else if (GameMGR._state == Board_Define.GAME_BATTEL_END)
        {
            Set_Ani(Ani_Define.VICTORY); // 승리포즈
            is_Unit_init = false;
        }
    }



    private void Unit_Init()
    {
        if (!is_Unit_init)
        {

            //  Debug.Log("이동 코루틴 : " + Next_Move + "전투 코루틴" + While_Fight);

            if (GameMGR._state == Board_Define.GAME_READY)
            {
                Set_Ivisible(true);
                Is_Dead = false;
                Enemy_state = Unit_Battle_State.READY;
                Range = Menemy_info.enemy_AtkRange;
                Health = Menemy_info.eneny_hp;
                Now_Bar_Cover.localScale = Now_Bar_Hp.localScale;

                Mana = 0;
                mPath = null;
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
                mAnimator = mModel.GetComponent<Animator>();

                if(Menemy_info.enemy_index != 3) // 드래곤이 아니라면
                {
                    mSkillObj = mModel.transform.GetChild(0).gameObject;
                }

                else if(Menemy_info.enemy_index == 3)
                {
                    Now_Bar.gameObject.SetActive(false);
                    Now_Bar = GameObject.FindObjectOfType<Move_Stage>().transform.GetChild(0).GetChild(0).gameObject;
                    Now_Bar_Hp = Now_Bar.transform.GetChild(2);
                    Now_Bar_Mp = Now_Bar.transform.GetChild(3);
                    Now_Bar_Cover = Now_Bar.transform.GetChild(4);
                }

                Set_Ani(Ani_Define.IDLE);


                if (GameMGR.Game_round.Equals(Board_Define.BossStage)&&Menemy_info.enemy_name.Equals("ICE_DRAGON"))
                {
                    Long_attack = false;
                    mCollider.GetComponent<BoxCollider>().size = new Vector3(4, 0.1f, 3);
                    mShadow.gameObject.SetActive(false);
                    //mShadow.transform.localScale = new Vector3(5.5f, 7, 7);
                    //mShadow.transform.localPosition = new Vector3(-0.42f, 0, -0.05f);
                }
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
            }
            is_Unit_init = true;
        }
    }

    #region 초기화/ui

    // 맨처음 초기화
    private void Init()
    {
        // 
        Now_Bar = transform.GetChild(1).gameObject;
        Now_Bar_Hp = Now_Bar.transform.GetChild(2);
        Now_Bar_Mp = Now_Bar.transform.GetChild(3);
        Now_Bar_Cover = Now_Bar.transform.GetChild(4);

        mCollider = transform.GetChild(0).gameObject;
        mAnimator = mModel.GetComponent<Animator>();

        mBullet = transform.GetChild(3).gameObject.GetComponent<Bullet>();
        mBullet_POS = transform.GetChild(4).transform.localPosition;


        Current_HP = Menemy_info.eneny_hp;
        Enemy_state = Unit_Battle_State.FINDING;
    }

    private void Up_To_Healthbar()
    {

        Now_Bar.transform.position = mModel.transform.position + new Vector3(0, 1.4f, 0);
        Now_Bar.transform.LookAt(Camera.main.transform.position);
        Now_Bar.transform.rotation = Quaternion.Euler(Camera.main.gameObject.transform.eulerAngles.x, 0, 0);
        mCollider.transform.position = Now_Bar.transform.position + new Vector3(0, 0.5f, 0);
    }

    public void Set_Ani(int flag)
    {
        mAnimator.SetInteger(mAniState, flag);
    }

    public void Enemy_info_update(Enemy info_)
    {
        Menemy_info.enemy_initial(info_);
    }

    //투명도 설정
    public void Set_Ivisible(bool flag_)
    {

        //if (flag_)
        //{
        //    for (int i = 0; i < mModel.transform.childCount - 1; i++)
        //    {
        //        mModel.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().material = mMaterial[1];
        //    }
        //}
        //else
        //{
        //    for (int i = 0; i < mModel.transform.childCount - 1; i++)
        //    {
        //        mModel.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().material = mMaterial[0];
        //    }
        //}
    }

    public void Update_HP()
    {

        // 체력의 수치에 대한 백분율을 기존체력대비 현재 체력으로 업데이트 시켜준다.
        Health_per = Current_HP / Menemy_info.eneny_hp * 100;

        Health_cover = Health_per;


        // 체력바 스프라이트에 체력에 대한 백분율을 적용하여 랜더링 해주는 함수
        SetSize_HP(Health_per / 100f);

        Health_Flag = true;

        if (Health_coru == null)
        {
            Health_coru = StartCoroutine(Set_SIZE_HP_Cover());
        }

        if (Current_HP <= 0)
        {
            Is_Dead = true;

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

            now_node.bIsWall = true;
            Enemy_state = Unit_Battle_State.DIE;
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
                if(GameMGR._state == Board_Define.GAME_BATTEL_END)
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


    #endregion

    #region 탐색
    // 타켓 탐색
    public void mFind_Target()
    {
        // 최단거리 타겟 검색
        MyTarget = Find_Target(GameMGR.Positioned_Character_Alive, transform);

        // 경로 탐색
        if (MyTarget != null)
        {
            if (Vector3.Distance(transform.position, MyTarget.transform.position) < Range)
            {
                Enemy_state = Unit_Battle_State.ATTACK;
                return;
            }
            mFind_Path();
        }

        Debug.Log(MyTarget);
    }

    // 경로 탐색
    public void mFind_Path()
    {

        transform.position = now_node.transform.position;
        Debug.Log(now_node + "/" + MyTarget.GetComponent<CharacterM>().now_node);

        mPath = FindPath(now_node, MyTarget.GetComponent<CharacterM>().now_node);

        Enemy_state = Unit_Battle_State.MOVE;

        if (Next_Move != null)
        {
            StopCoroutine(Next_Move);
            Next_Move = null;
        }

    }

    //우회경로탐색
    public void mFind_Path_detour(Node detour_detour)
    {
        transform.position = now_node.transform.position;

        if (MyTarget.GetComponent<CharacterM>().now_node == null)
        {
            Debug.Log("타겟노드없음");
        }

        mPath = FindPath(now_node, MyTarget.GetComponent<CharacterM>().now_node);
        Enemy_state = Unit_Battle_State.MOVE;
    }


    // 모든 적 거리 탐색
    public void Search_Distance()
    {
        float now_distance = (transform.position - MyTarget.transform.position).sqrMagnitude;
        float tmp_distance = 0;

        for (int i = 0; i < GameMGR.Positioned_Character_Alive.Count; i++)
        {
            tmp_distance = (transform.position - GameMGR.Positioned_Character_Alive[i].transform.position).sqrMagnitude;

            if (Vector3.Distance(transform.position, GameMGR.Positioned_Character_Alive[i].transform.position) < Range)
            {
                MyTarget = GameMGR.Positioned_Character_Alive[i];
                Enemy_state = Unit_Battle_State.ATTACK;

                if (Next_Move != null)
                {
                    StopCoroutine(Next_Move);
                    Next_Move = null;
                }
                return;
            }

            if (tmp_distance < now_distance)
            {
                MyTarget = GameMGR.Positioned_Character_Alive[i];
                mFind_Path();
                return;
            }
        }
        mFind_Path();
    }
    #endregion

    #region 이동
    //이동

    public void Move_pos()
    {
        //경로에 따른 이동
        if (Next_Move == null)
        {
            Next_Move = StartCoroutine(Move_nextPos());
        }
    }


    IEnumerator Move_nextPos()
    {
        float Target_Distance;

        now_node.bIsWall = true;

        while (true)
        {
            //Debug.Log(mPath);
            // 경로가 있는 경우에 실행
            if (mPath != null)
            {
                //
                if (mPath.Count == 1)
                {
                    Debug.Log("path : 1");
                    Search_Distance();
                }

                if (!mPath[path_index].Equals(now_node))
                {
                    if (mPath[path_index].bisCharacter)
                    {
                        Debug.Log("path : 타겟탐색");
                        Search_Distance();
                    }

                    else if (mPath[path_index].bisEnemy)
                    {
                        Debug.Log("path : 우회");
                        mFind_Path_detour(mPath[path_index]);
                    }

                    else
                    {
                        mPath[path_index].bisEnemy = true;
                    }
                }

                Set_Ani(Ani_Define.WALK);
                transform.LookAt(mPath[path_index].transform.position);
                transform.Translate(Vector3.forward * Time.deltaTime * 1.5f);
                Target_Distance = (transform.position - mPath[path_index].transform.position).sqrMagnitude;


                // 타겟 도착시,
                if (mPath[path_index].Equals(now_node))
                {
                    mPath[path_index].bisEnemy = false;
                    //transform.position = mPath[path_index].vPosition;
                    Search_Distance();
                    yield break;
                }


                if (GameMGR._state == Board_Define.GAME_BATTEL_END)
                {
                    Next_Move = null;
                    Enemy_state = Unit_Battle_State.READY;
                    yield break;
                }
            }
            yield return null;
        }
    }
    #endregion

    #region 전투
    //전투
    IEnumerator MinusHealth_e()
    {
        Debug.Log(mPath);

        if (mPath != null)
        {
            float now_distance = (transform.position - now_node.transform.position).sqrMagnitude;
            float next_distance = (transform.position - mPath[path_index].transform.position).sqrMagnitude;
            transform.position = now_distance > next_distance ? mPath[path_index].transform.position : now_node.transform.position;
            mPath = null;
        }
        if (Next_Move != null)
        {
            StopCoroutine(Next_Move);
            Next_Move = null;
        }

        now_node.bIsWall = false;
        mTarget = MyTarget.GetComponent<CharacterM>();

        while (true)
        {
            if (GameMGR._state == Board_Define.GAME_BATTLE)
            {
                // 타겟이 살아있는 경우
                if (mTarget.Is_Dead == false)
                {
                    now_node.bisEnemy = true;
                    // MoveMGR.LookAt_Target(MyTarget, ref MyTrans);
                    transform.LookAt(MyTarget.transform);

                    if (MyTarget != null)
                    {
                        if (Current_MP >= 100)
                        {
                            Current_MP = 0;
                            Skill_Switch = true;

                            if (Menemy_info.enemy_index == 3)
                            {
                                gameObject.transform.eulerAngles = new Vector3(0, 180, 0);
                            }

                            // 스킬 애니메이션 실행
                            while (true)
                            {
                                Set_Ani(Ani_Define.SKILL);

                                if (Skill_Switch == false)
                                {
                                    break;
                                }
                                yield return null;
                            }
                        }
                        else
                        {
                            Set_Ani(Ani_Define.ATTACK);
                            Mana += Menemy_info.enemy_VaryMP;
                            yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

                            if (Long_attack)
                            {
                                while (true)
                                {
                                    Set_Ani(Ani_Define.IDLE);

                                    if (On_hit_bullet)
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

                // 타겟이 죽은 경우
                else
                {
                    now_node.bisEnemy = false;
                    MyTarget = null;
                    While_Fight = null;
                    Enemy_state = Unit_Battle_State.FINDING;
                    yield break;
                }
            }
            else if (GameMGR._state == Board_Define.GAME_BATTEL_END)
            {
                mSkillObj.SetActive(false);
                mSKillBox = null;
                now_node.bIsWall = true;
                Enemy_state = Unit_Battle_State.READY;
                MyTarget = null;
                While_Fight = null;
                is_Unit_init = false;
                yield break;
            }
        }
    }

    public void GetHit(float value)
    {
        //mAudio.Play();
        Health -= value * Vary_Skill_Value;

        //마나가 100을 넘었을경우 100으로 고정, 100을 넘을경우 마나바가 너무 길어짐 
        if (Mana + Menemy_info.enemy_VaryMP > 100)
        {
            Mana = 100;
        }
    }

    public override void Short_Attack()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            mTarget.GetHit(Menemy_info.enemy_damage);
        }
    }

    public override void Long_Attack()
    {
        if (GameMGR._state == Board_Define.GAME_BATTLE)
        {
            mBullet.Shoot_Bullet(this, mTarget);
        }
    }

    #endregion

    #region 사망

    IEnumerator Die_Ani()
    {
        mSkillObj.SetActive(false);
        Set_Ani(Ani_Define.DIE);
        yield return new WaitForSeconds(mAnimator.GetCurrentAnimatorStateInfo(0).length);

        GameMGR.AliveEnemy.Remove(gameObject);
        GameMGR.Alive_Enemy_Num--;
        gameObject.SetActive(false);
        PoolMGR.Push_Pooling(gameObject, Menemy_info);
        is_Unit_init = false;
        gameObject.SetActive(false);
        Enemy_state = Unit_Battle_State.READY;
    }

    #endregion

    #region 스킬!

    public override void Use_Skill()
    {
        if (Now_Skill != null)
        {
            Debug.Log("스킬!!!!!!!!!!!!!!!!!!!!!!!!");

            StartCoroutine(Now_Skill());
        }

        Mana = 0;
        Update_MP();
    }

    private void Skill_Init()
    {
        // 유닛의 정보에 따라 사용하는 스킬을 바꿈
        switch (Menemy_info.enemy_index)
        {
            // 각 유닛의 스킬 작동
            case 0: // Enemy Warrior
                {
                    Now_Skill = new mSKILL(EnemyWarrior_skill);
                    break;
                }

            case 1: //Enemy Wizard
                {
                    Now_Skill = new mSKILL(EnemyWizard_skill);
                    break;
                }
            case 3: // ICE_DRAGON
                {
                    Now_Skill = new mSKILL(ICE_DRAGON_skill);
                    break;
                }
        }
    }



    #region EnemyWarrior_skill 함수
    IEnumerator EnemyWarrior_skill()
    {
        yield return null;

        mSkillObj = mModel.transform.GetChild(2) // Hips
                        .transform.GetChild(1) // Spine
                        .transform.GetChild(4) // Upper_Arm_R
                        .transform.GetChild(0) // Hand_R
                        .transform.GetChild(0) // Index_Proximal_R
                        .transform.GetChild(0)
                        .transform.GetChild(0).gameObject;    // z_weapon container 1

        Debug.Log(mSkillObj.name);

        mSkillObj.SetActive(true);

        yield return new WaitForSeconds(0.7f);

        MyTarget.GetComponent<CharacterM>().GetHit(100f);

        mSkillObj.SetActive(false);
        Skill_Switch = false;
        yield break;
    }
    #endregion

    #region EnemyWizard_skill 함수
    IEnumerator EnemyWizard_skill()
    {
        yield return null;
        Debug.Log("EnemyWizard_skill / Enter");

        // 나의 타겟의 위치에 mSkillObj를 보내놓고 활성화를 시킨다.
        mSkillObj.transform.position = MyTarget.transform.position;
        mSkillObj.SetActive(true);

        // 나의 타겟
        Node EnemyNowNode = MyTarget.GetComponent<CharacterM>().now_node;

        List<GameObject> aliveCharacterList = GameMGR.Positioned_Character;

        // 타겟 주변의 노드를 가져와
        List<Node> NodeList = GridMGR.GetNeighboringNodes(EnemyNowNode);

        for (int i = 0; i < NodeList.Count; i++)
        {
            Node indexNode = NodeList[i];

            for (int j = 0; j < aliveCharacterList.Count; j++)
            {
                GameObject aliveCharacter = aliveCharacterList[j];

                if (aliveCharacter.GetComponent<CharacterM>().now_node.Equals(indexNode))
                {
                    aliveCharacter.GetComponent<CharacterM>().GetHit(100);
                }
            }
        }
        MyTarget.GetComponent<CharacterM>().GetHit(100);
        Skill_Switch = false;

        yield return new WaitForSeconds(2f);

        mSkillObj.SetActive(false);
        yield break;
    }
    #endregion

    //void EnemyDifencer_skill()
    //{

    //}

    #region ICE_DRAGON_skill 함수
    IEnumerator ICE_DRAGON_skill()
    {
        Debug.Log("ICE_DRAGON_skill / Enter");
        yield return null;

        GameObject DragonBreath
            = mModel.transform.GetChild(0) // CG
            .transform.GetChild(0) // Pelvis
            .transform.GetChild(2) // Spine
            .transform.GetChild(0) // Spine1
            .transform.GetChild(0) // Spine2
            .transform.GetChild(2) // Neck
            .transform.GetChild(0) // Neck1
            .transform.GetChild(0) // Neck2
            .transform.GetChild(0) // Neck3
            .transform.GetChild(0) // Neck4
            .transform.GetChild(0) // Head
            .transform.GetChild(0) // Jaw
            .transform.GetChild(1).gameObject; // Dragon_Skill

        DragonBreath.SetActive(true);

        Debug.Log("@@@:"+DragonBreath.name);
        yield return new WaitForSeconds(2.4f);

        List<GameObject> aliveCharacterList = GameMGR.Positioned_Character;
        for (int i = 0; i < aliveCharacterList.Count; i++)
        {
            aliveCharacterList[i].GetComponent<CharacterM>().GetHit(100);
        }

        DragonBreath.SetActive(false);
        Skill_Switch = false;
        yield break;
    }
    #endregion

    #endregion
}







