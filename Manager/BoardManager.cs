using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class BoardManager : MonoBehaviour
{
    // 타일 사이즈
    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;


    //// 맵 상의 선택한 좌표
    //public int selectionX = -1;
    //public int selectionY = -1;
    public Vector3 SelectionPos;

    //맵에 
    public int Mouse_on_Type;

    // 배치/생성 관련 오브젝트
    public spawn_check Check_;  // 마우스 선택한 구역에 캐릭터가 생성되어 있는지 확인하는 오브젝트
   // public GameObject Line_;

    Camera Main_camera;

    public GameObject Dispose_unit = null; // 배치할 캐릭터 오브젝트

    public bool Is_Enemy_spawn = false; // 적 배치 완료
    public bool is_state_initial = false;   // 라운드 초기화 완료

    public Coroutine Dispose_coroutine = null; // 배치 코루틴

    public ObjectPoolManager PoolMGR;
    public DataManager DataMGR;
    public WorldOfTheCuteMouse WOTCM_MGR;
    private GameManager gameMGR;
    private StageManager StageMGR;
    private GridManager GridMGR;

    #region 단계별 동작

    public void Board_First_Start(GameManager GameMGR_)
    {
        gameMGR = GameMGR_;
        DataMGR = gameMGR.DataMGR;
        PoolMGR = gameMGR.PoolMGR;
        StageMGR = gameMGR.StageMGR;
        GridMGR = gameMGR.GridMGR;

        Check_ = transform.GetChild(0).GetComponent<spawn_check>();
        Main_camera = Camera.main;
    }

    public void Board_Game_Ready()
    {

        Initial_Ready_state();
        UpdateSelection();
        Dispose_character();
        gameMGR.Alive_Character_Num = gameMGR.Positioned_Character.Count;
        // 리롤하다가 체력 0 되어도 죽어야지..
        PlayerDIE();
    }

    public void Board_Game_Battle()
    {
        Initial_Battle_state();
        Result_Battle();
    }

    public void Board_Game_Battle_End()
    {
        // 원준
        PlayerDIE();

        if(gameMGR.Game_round > Board_Define.BossStage)
        {
            gameMGR.Game_END = true;
        }
        else
        {
            Is_Enemy_spawn = false;

            //gameMGR.Player_HP += 10;
            if (gameMGR.Player_HP > 100)
            {
                gameMGR.Player_HP = 100;
            }

            is_state_initial = false;
            //gameMGR._state = Board_Define.GAME_READY;
        }
    }
    #endregion

    // 원준
    void PlayerDIE()
    {
        if (gameMGR.Player_HP <= 0)
        {
            // 와큼 로직
            gameMGR.game_Result = Board_Define.LOOSE;
            StartCoroutine(WOTCM_MGR.WOTCM_INGAME_DIE());
            gameMGR.mFadeControl.gameObject.SetActive(true);
            gameMGR.mFadeControl.Result_Fadeout_start(gameMGR.UiMGR);
        }
    }

    #region 생성/데이터 관련
    /// <summary>
    ///  선택된 보드 좌표 구하기
    /// </summary>
    /// <param name="lbl"></param>
    private void UpdateSelection()
    {
        if (gameMGR.Mouse_Type == Mouse_Mode.ON_EXIT)
        {
            return;
        }

        if (!Main_camera)   // 메인 카메라가 아닌경우에는 실행하지 않는다
            return;

        RaycastHit hit; // 선택된 곳을 나타낼 레이캐스트 변수

        // 카메라에서 보이는 화면에서 마우스 포인트와 체스 보드판인 곳을 선택한 지점에 닿았을 경우
        if (Physics.Raycast(Main_camera.ScreenPointToRay(gameMGR.mTouchPos), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            Ray ray = Main_camera.ScreenPointToRay(gameMGR.mTouchPos);
            Debug.DrawRay(ray.origin, hit.point, Color.blue);
            // 선택한 지점을 좌표로 받는다
            SelectionPos = hit.collider.gameObject.transform.position;
            Mouse_on_Type = Board_Define.ONBOARD;


        }

        else if (Physics.Raycast(Main_camera.ScreenPointToRay(gameMGR.mTouchPos), out hit, 25.0f, LayerMask.GetMask("StoragePlane")))
        {
            Ray ray = Main_camera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, hit.point, Color.red);

            //// 기존 좌표 방식
            //selectionX = (int)hit.point.x;
            //selectionY = (int)hit.point.z;

            //노드 인식 방식

            SelectionPos = hit.collider.gameObject.transform.position;

            Debug.Log(SelectionPos);

            Mouse_on_Type = Board_Define.ONSTORAGE;
        }
        else if (Physics.Raycast(Main_camera.ScreenPointToRay(gameMGR.mTouchPos), out hit, 55.0f, LayerMask.GetMask("SellPlane")))
        {

            Ray ray = Main_camera.ScreenPointToRay(gameMGR.mTouchPos);
            Debug.DrawRay(ray.origin, hit.point, Color.yellow);
            Mouse_on_Type = Board_Define.ONSELLZONE;
        }
        else
        {
            // 선택이 안된 상태로 바꾼다
            Mouse_on_Type = Board_Define.NONE;
        }



        // 선택한 곳이 플레이어의 영역인 경우
        if (Mouse_on_Type == Board_Define.ONBOARD)
        {
            Check_.gameObject.SetActive(true);
            Check_.transform.position = SelectionPos;  // 캐릭터를 생성을 확인할 오브젝트를 이동
                                                       // Check_.transform.position = GetTileCenter(selectionX, selectionY);  // 캐릭터를 생성을 확인할 오브젝트를 이동
        }

        else if (Mouse_on_Type == Board_Define.ONSTORAGE)
        {
            Check_.gameObject.SetActive(true);
            Check_.transform.position = SelectionPos;
            //Check_.transform.position = GetTileCenter(selectionX, selectionY);  // 캐릭터를 생성을 확인할 오브젝트를 이동
        }
        else
        {
            Check_.gameObject.SetActive(false);
        }

        if (Mouse_on_Type == Board_Define.NONE)
            Check_.gameObject.SetActive(false);
    }

    public void SpawnCharacter(Character info_, Transform trans_, ref GameObject tmp_)
    {
        GameObject unit_ = PoolMGR.Pop_Pooling(info_);
        Debug.Log(unit_.transform.GetChild(1));
        unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
        unit_.transform.position = trans_.position;
        unit_.transform.rotation = Quaternion.identity;

        unit_.GetComponent<CharacterM>().mModel.transform.rotation = Quaternion.identity;

        //unit_.GetComponent<CharacterM>().On_board = false;
        //   gameMGR.Storage_Character.Add(unit_);

        gameMGR.All_Character.Add(unit_);

        tmp_ = unit_;
    }

    // 상점 구매 스폰
    public void SpawnCharacter(Character info_, Transform trans_)
    {
        GameObject unit_ = PoolMGR.Pop_Pooling(info_);
        //  Debug.Log(unit_.transform.GetChild(1));
        unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
        unit_.transform.position = trans_.position;
        unit_.transform.rotation = Quaternion.identity;
        unit_.GetComponent<CharacterM>().mModel.transform.rotation = Quaternion.identity;
        //  unit_.GetComponent<CharacterM>().On_board = false;
        gameMGR.Storage_Character.Add(unit_);
        gameMGR.All_Character.Add(unit_);
    }




    /// <summary>
    ///  적 생성
    /// </summary>
    /// <param name="lbl"></param>
    private void SpawnEnemy()
    {
        Vector3 Espawn_position = Vector3.zero;

        if (Is_Enemy_spawn == false)
        {
            //근접적 생성
            for (int i = 0; i < DataMGR.Stage_info[gameMGR.Game_round - 1].EnemyWarrior_count; i++)
            {
                GameObject unit_ = PoolMGR.Pop_Pooling(DataMGR.Enemy_find(0));
                StageMGR.Random_position(ref Espawn_position, Enemy_Type.Short);

                unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
                unit_.transform.position = Espawn_position;
                unit_.transform.localRotation = new Quaternion(0, 180, 0, 0);
                gameMGR.AliveEnemy.Add(unit_);
            }

            //원거리 적 생성
            for (int i = 0; i < DataMGR.Stage_info[gameMGR.Game_round - 1].EnemyWizard_count; i++)
            {
                GameObject unit_ = PoolMGR.Pop_Pooling(DataMGR.Enemy_find(1));
                StageMGR.Random_position(ref Espawn_position, Enemy_Type.Long);

                unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
                unit_.transform.position = Espawn_position;
                unit_.transform.localRotation = new Quaternion(0, 180, 0, 0);
                gameMGR.AliveEnemy.Add(unit_);
            }

            //골렘 생성
            for (int i = 0; i < DataMGR.Stage_info[gameMGR.Game_round - 1].EnemyDifencer_count; i++)
            {
                GameObject unit_ = PoolMGR.Pop_Pooling(DataMGR.Enemy_find(2));
                StageMGR.Random_position(ref Espawn_position, Enemy_Type.Difencer);

                unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
                unit_.transform.position = Espawn_position;
                unit_.transform.localRotation = new Quaternion(0, 180, 0, 0);
                gameMGR.AliveEnemy.Add(unit_);
            }

            //용 생성
            for (int i = 0; i < DataMGR.Stage_info[gameMGR.Game_round - 1].Boss_count; i++)
            {
                GameObject unit_ = PoolMGR.Pop_Pooling(DataMGR.Enemy_find(3));
                StageMGR.Random_position(ref Espawn_position, Enemy_Type.Boss);

                unit_.transform.parent = transform; // 보드 매니저로 부모로 이동
                unit_.transform.position = Espawn_position;
                unit_.transform.localRotation = new Quaternion(0, 180, 0, 0);
                gameMGR.AliveEnemy.Add(unit_);
            }

            Is_Enemy_spawn = true;

        }

    }

    #region 단계별 초기화 작업

    private void Initial_Ready_state()
    {
        if (is_state_initial == false)
        {
            gameMGR.Random_value_update();
            // line_.SetActive(true); // 배치 라인 비활성화 , 1021 스프라이트 교체 예정
            Reset_Ready_playable();
            SpawnEnemy();
            is_state_initial = true;
        }
    }

    private void Initial_Battle_state()
    {
        if (is_state_initial == false)
        {

            for (int i = 0; i < gameMGR.Positioned_Character.Count; i++)
            {
                gameMGR.Positioned_Character_Alive.Add(gameMGR.Positioned_Character[i]);
                //  Debug.Log(gameMGR.Positioned_Character.Count + "   /   " + gameMGR.Positioned_Character_Alive[i]);
            }

            is_state_initial = true;
        }
    }

    #endregion

    /// <summary>
    ///  Ready 단계, 캐릭터/적 초기화
    /// </summary>
    /// <param name="lbl"></param>
    private void Reset_Ready_playable()
    {
        if (gameMGR._state == Board_Define.GAME_READY)
        {
            if (gameMGR.Game_round >= 1)
            {
                for (int i = 0; i < gameMGR.Positioned_Character.Count; i++)
                {
                    gameMGR.Positioned_Character[i].transform.position = gameMGR.Character_origin_position[i].position;
                    gameMGR.Positioned_Character[i].transform.rotation = Quaternion.identity;
                    gameMGR.Positioned_Character[i].transform.GetChild(1).localPosition = Vector3.zero;
                    gameMGR.Positioned_Character[i].transform.GetChild(1).transform.rotation = Quaternion.identity;
                    gameMGR.Positioned_Character[i].SetActive(true);
                }
                while (true)
                {
                    if (gameMGR.AliveEnemy.Count == 0)
                    {
                        break;
                    }
                    PoolMGR.Push_Pooling(gameMGR.AliveEnemy[0], gameMGR.AliveEnemy[0].gameObject.GetComponent<EnemyM>().Menemy_info);
                    gameMGR.AliveEnemy.Remove(gameMGR.AliveEnemy[0]);
                }
            }
        }
    }

    public void Reset_All_playable()
    {
        for (int i = 0; i < gameMGR.DataMGR.characters.Count; i++)
        {
            gameMGR.mCharacter_Count[DataMGR.characters[i].character_name] = 0;
        }

        // 보드에 배치된 캐릭터 초기화
        while (true)
        {
            if (gameMGR.Positioned_Character.Count == 0)
            {
                break;
            }
            PoolMGR.Push_Pooling(gameMGR.Positioned_Character[0], gameMGR.Positioned_Character[0].gameObject.GetComponent<CharacterM>().Mcharacter_info);
            gameMGR.Positioned_Character.Remove(gameMGR.Positioned_Character[0]);
        }

        // 캐릭터 초기화
        while (true)
        {
            if (gameMGR.All_Character.Count == 0)
            {
                break;
            }
            PoolMGR.Push_Pooling(gameMGR.All_Character[0], gameMGR.All_Character[0].gameObject.GetComponent<CharacterM>().Mcharacter_info);
            gameMGR.All_Character.Remove(gameMGR.All_Character[0]);
        }

        // 창고 캐릭터 초기화
        while (true)
        {
            if (gameMGR.Storage_Character.Count == 0)
            {
                break;
            }
            PoolMGR.Push_Pooling(gameMGR.Storage_Character[0], gameMGR.Storage_Character[0].gameObject.GetComponent<CharacterM>().Mcharacter_info);
            gameMGR.Storage_Character.Remove(gameMGR.Storage_Character[0]);
        }

        // 적 초기화
        while (true)
        {
            if (gameMGR.AliveEnemy.Count == 0)
            {
                break;
            }
            PoolMGR.Push_Pooling(gameMGR.AliveEnemy[0], gameMGR.AliveEnemy[0].gameObject.GetComponent<EnemyM>().Menemy_info);
            gameMGR.AliveEnemy.Remove(gameMGR.AliveEnemy[0]);
        }
    }

    /// <summary>
    ///  전투 결과
    /// </summary>
    /// <param name="lbl"></param>
    private void Result_Battle()
    {
        if (gameMGR.AliveEnemy.Count == 0)
        {
            gameMGR.game_Result = Board_Define.WIN;
            gameMGR.Player_HP += 10;     //0930 _ 승리시 체력 증가

            if (gameMGR.Player_HP > 100)
            {
                gameMGR.Player_HP = 100;
            }
            gameMGR._state = Board_Define.GAME_BATTEL_END;

            StartCoroutine(WOTCM_MGR.WOTCM_ROUND_VICTORY());
        }

        else if (gameMGR.Positioned_Character_Alive.Count == 0)
        {
            gameMGR.game_Result = Board_Define.LOOSE;
            //gameMGR.Player_HP -= 10;     //0930 _ 패배시 체력 감소
            gameMGR._state = Board_Define.GAME_BATTEL_END;

            // 와큼 로직
            StartCoroutine(WOTCM_MGR.WOTCM_ROUND_LOSE());
        }

        if (gameMGR._state == Board_Define.GAME_BATTEL_END)
        {
            gameMGR.Positioned_Character_Alive.Clear();
            //for (int i = 0; i < gameMGR.Positioned_Character_Alive.Count; i++)
            //    gameMGR.Positioned_Character_Alive.Remove(gameMGR.Positioned_Character_Alive[i]);
        }

    }

    /// <summary>
    ///  타일의 가운데값 반환
    /// </summary>
    /// <param name="lbl"></param>
    public Vector3 GetTileCenter(int x, int y)
    {
        Vector3 orgin = Vector3.zero;
        orgin.x += (TILE_SIZE * x) + TILE_OFFSET;
        orgin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return orgin;
    }

    public void Dispose_character()
    {
        #region 터치시 동작
        //if ((gameMGR.Tphase== TouchPhase.Stationary||gameMGR.Mouse_Type==Mouse_Mode.ON_CLICK)&& Dispose_unit!=null)
        if (gameMGR.Mouse_Type == Mouse_Mode.ON_CLICK && Dispose_unit != null)
        {
            if (Dispose_coroutine != null)
            {
                return;
            }
            Debug.Log(Dispose_unit);
            Dispose_coroutine = StartCoroutine(Dispose_unit_mouse());
        }

        else if (Dispose_coroutine == null)
        {
            Dispose_unit = null;
        }
        #endregion
    }

    #endregion



    IEnumerator Dispose_unit_mouse()
    {
        Transform origin_Transform = Dispose_unit.transform;
        Vector3 origin_position = Dispose_unit.transform.position;
        CharacterM unit_ = Dispose_unit.GetComponent<CharacterM>();
        int Origin_mouse_type = Mouse_on_Type;
        //Line_.SetActive(true);
        GridMGR.Line_Set(true);
        Debug.Log("Dispose_unit_mouse : start");

        while (true)
        {
            yield return null;
            if (gameMGR.Mouse_Type == Mouse_Mode.ON_EXIT)
            {
                //Line_.SetActive(false);
                GridMGR.Line_Set(false);
                Check_.dispos_mode = false;
                unit_.mCollider.gameObject.SetActive(true);
                unit_.Set_Ani(Ani_Define.IDLE);

                // 왓큼로직
                if (WOTCM_MGR.Click_Switch == true && WOTCM_MGR.Click_Coru == null)
                {
                    WOTCM_MGR.Click_Coru = StartCoroutine(WOTCM_MGR.WOTCM_CLICK_END());
                }

                if (Origin_mouse_type != Mouse_on_Type)
                {
                    if (Mouse_on_Type == Board_Define.ONBOARD)
                    {
                        gameMGR.Positioned_Character.Add(Dispose_unit);
                        gameMGR.Storage_Character.Remove(Dispose_unit);
                        gameMGR.Character_origin_position.Add(unit_.Out_Node().transform);
                        unit_.On_board = true;
                    }

                    else if (Mouse_on_Type == Board_Define.ONSTORAGE)
                    {
                        gameMGR.Storage_Character.Add(Dispose_unit);
                        gameMGR.Positioned_Character.Remove(Dispose_unit);
                        gameMGR.Character_origin_position.Remove(unit_.now_node.transform);
                        unit_.On_board = false;
                    }

                    else if (Mouse_on_Type == Board_Define.ONSELLZONE)
                    {
                        if (Origin_mouse_type == Board_Define.ONBOARD)
                        {
                            gameMGR.Positioned_Character.Remove(Dispose_unit);
                            gameMGR.Character_origin_position.Remove(unit_.now_node.transform);
                        }
                        else if (Origin_mouse_type == Board_Define.ONSTORAGE)
                        {
                            gameMGR.Storage_Character.Remove(Dispose_unit);
                        }

                        gameMGR.mCharacter_Count[Dispose_unit.GetComponent<CharacterM>().Mcharacter_info.character_name]--;
                        gameMGR.All_Character.Remove(Dispose_unit);
                        PoolMGR.Push_Pooling(Dispose_unit, Dispose_unit.GetComponent<CharacterM>().Mcharacter_info);

                        gameMGR.Player_HP += 5;  //0930 _ 판매시 체력 증가

                        // 왓큼 로직
                        WOTCM_MGR.Click_Coru = StartCoroutine(WOTCM_MGR.WOTCM_SELL());
                    }
                }
                else
                {
                    if (Mouse_on_Type == Board_Define.ONBOARD)
                    {
                        gameMGR.Character_origin_position.Remove(unit_.now_node.transform);
                        gameMGR.Character_origin_position.Add(unit_.Out_Node().transform);
                    }
                }

                if (Mouse_on_Type == Board_Define.NONE || Check_.spawn_order == false)
                {
                    Debug.Log(Dispose_unit.transform.position + " / " + origin_position);
                    Dispose_unit.transform.position = origin_position;
                }

                Dispose_unit = null;
                //gameMGR.On_Touch = false;
                Check_.gameObject.SetActive(false);

                Dispose_coroutine = null;
                Mouse_on_Type = Board_Define.NONE;
                Check_.gameObject.transform.position = Vector3.zero;
                yield break;
            }
            //왓큼 로직
            else
            {
                WOTCM_MGR.Click_Coru = null;
            }

            Check_.dispos_mode = true;
            unit_.mCollider.SetActive(false);

            if (Check_.gameObject.activeSelf)
            {
                Dispose_unit.transform.position = Check_.transform.position;
            }
            else if (Mouse_on_Type == Board_Define.NONE || Mouse_on_Type == Board_Define.ONSELLZONE && gameMGR.Mouse_Type != Mouse_Mode.ON_EXIT)
            {
                Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 21);
                Dispose_unit.transform.position = Main_camera.ScreenToWorldPoint(mousePosition);

                // 왓튼에게 Dispose_unit.transform 을 던져주면서 바라보게 만들면됨.
                WOTCM_MGR.WOTCM_CLICK_STAY(Dispose_unit.transform);
            }

            if (Mouse_on_Type == Board_Define.ONSELLZONE)
            {
                Debug.Log(Dispose_unit);
                unit_.Set_Ani(Ani_Define.VICTORY);
            }
            else
            {
                unit_.Set_Ani(Ani_Define.ATTACK);
            }


        }
    }
}
