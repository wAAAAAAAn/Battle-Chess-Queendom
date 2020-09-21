#define TEST_VER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


////플레이시 첫 씬으로 이동
//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
// static void First_Load()
// {

//         //현재 활성화된 씬이 0번이 아니면 0번으로 이동
//         if (SceneManager.GetActiveScene().buildIndex != 0)
//         {
//             SceneManager.LoadScene(0);
//         }

// }


public class GameManager : MonoBehaviour
{
    public List<GameObject> Positioned_Character = new List<GameObject>(); //체스판에 배치된 캐릭터 리스트
    public List<GameObject> Positioned_Character_Alive = new List<GameObject>(); //체스판에 배치된 캐릭터 리스트(배치된 캐릭터만)
    public List<GameObject> Storage_Character = new List<GameObject>(); // 창고에 배치된 캐릭터 리스트
    public List<GameObject> All_Character = new List<GameObject>(); // 모든 캐릭터의 갯수
   // public List<GameObject> LevelUp_Character = new List<GameObject>(); // 레벨업할 캐릭터 리스트
    public Dictionary<string, int> mCharacter_Count = new Dictionary<string, int>();   
    public List<Transform> Character_origin_position = new List<Transform>();   // 체스판에 배치된 캐릭터의 위치 원본
    public List<GameObject> AliveEnemy = new List<GameObject>();    //배틀단계에서 배치된 적 리스트


    public int _state = Board_Define.GAME_INITIAL;  // 게임 상태
    public int Game_round = 1; // 게임 라운드L
    public bool Game_END = false;

    private float mPlayer_HP = 100; // 플레이어 체력

    public float Player_HP
    {
        get { return mPlayer_HP; }
        set
        {
            mPlayer_HP = value;
           //UiMGR.Slider_update();
        }
    }

    public int game_Result; // 게임의 결과

    public int Alive_Enemy_Num = 0; // 맵에 살아있는 적의 개수
    public int Alive_Character_Num = 0; // 맵에 살아있는 적의 개수

    public float GAMESPEED = 1; // 게임의 속도
    public float time = 30;  // 시간 초기값 30, 테스트는 5초

    public bool TESTMODE = false; // 미구현, 마우스 입력 여부

    public int Mouse_Type = Mouse_Mode.ON_EXIT;

    // 마우스 / 터치 동작 관련
    //public Touch mTouch;    // 터치 
    //public bool On_Touch = false;  // 터치시
    //public TouchPhase Tphase;   // 터치 상태


    public Vector3 mTouchPos;
    private Vector3 mOriginPos;
    private bool is_Board = false;  // 보드에 있는지

    // 각 매니저
    public Reference_of_MGR ReferenceMGR;
    public DataManager DataMGR;
    public ObjectPoolManager PoolMGR;
    public BoardManager BoardMGR;
    public Ui_manager UiMGR;
    public GridManager GridMGR;
    public StageManager StageMGR;
    public Move_Stage Move_Stage;

    public FadeControl mFadeControl;

    void Start()
    {
        ReferenceMGR = FindObjectOfType<Reference_of_MGR>();
        DataMGR = ReferenceMGR.Data_MGR;
        PoolMGR = ReferenceMGR.Pool_MGR;
        BoardMGR = ReferenceMGR.Board_MGR;
        UiMGR = ReferenceMGR.UI_MGR;
        GridMGR = ReferenceMGR.Grid_MGR;
        StageMGR = ReferenceMGR.Stage_MGR;

        mFadeControl.Fadein_start();

        StartCoroutine(Board_Manage_Rountine());
    }

    public void Game_Initial()
    {
        //  정보 초기화
        mPlayer_HP = 100;
        Game_round = 1;

        // 배치된 캐릭터들 초기화
        BoardMGR.Reset_All_playable();

        //game_end, state 초기화
        Game_END = false;
        _state = Board_Define.GAME_READY;
    }

    IEnumerator Board_Manage_Rountine()
    {
        StartCoroutine(Mouse_Input());

        while (true)
        {
            if(_state == Board_Define.GAME_INITIAL)
            {
                DataMGR.Data_First_Start(this);
                PoolMGR.ObjectPool_First_Start(this);
                GridMGR.Grid_First_Start();
                BoardMGR.Board_First_Start(this);
                StageMGR.Stage_First_Start(this);
                UiMGR.UI_First_Start(this);

                _state = Board_Define.GAME_READY;
            }

            if (Game_END == false)
            {
                if (_state == Board_Define.GAME_READY)
                {
                    BoardMGR.Board_Game_Ready();
                    UiMGR.Ui_Reddy();

                    // 원준
                    if(Player_HP <= 0)
                    {
                        Game_END = true;
                        _state = Board_Define.GAME_BATTEL_END;
                        BoardMGR.Board_Game_Battle_End();
                        UiMGR.Ui_End();
                        yield return new WaitForSeconds(3.0f);  // 승리/패배 애니메이션 대기 시간
                    }
                }

                else if (_state == Board_Define.GAME_BATTLE)
                {
                    if(Game_round > Board_Define.BossStage)
                    {
                        UiMGR.Set_Off_uI_BOSS();
                    }
                    UiMGR.mShop.onoff_shop(false);
                    BoardMGR.Board_Game_Battle();
                }

                else if (_state == Board_Define.GAME_BATTEL_END)
                {
                    BoardMGR.Board_Game_Battle_End();
                    UiMGR.Ui_End();
                    yield return new WaitForSeconds(3.0f);  // 승리/패배 애니메이션 대기 시간

                    // 원준
                    if(Player_HP > 0)
                    {
                        _state = Board_Define.GAME_MOVE;
                    }
                }
                else if (_state == Board_Define.GAME_MOVE)
                {
                    Move_Stage.Start_Stage_Move();
                }

            }


            yield return null;
         
            #region 터치 동작
            /*
            if (Input.touchCount > 0)
            {
                mTouch = Input.GetTouch(0);
                mTouchPos = mTouch.position;
                On_Touch = true;
                Tphase = mTouch.phase;
                Debug.Log("GetTouchDown");

            }
            else
            {
                On_Touch = false;
            }
            */
            #endregion

            // 항상 업데이트
            Time.timeScale = GAMESPEED;
        }
    }

    IEnumerator Mouse_Input()
    {
        while (true)
        {
            if (_state != Board_Define.GAME_INITIAL)
            {
                // 마우스 버튼 다운
                if (Input.GetMouseButtonDown(0))
                {
                    Mouse_Type = Mouse_Mode.ON_CLICK;
                    mTouchPos = Input.mousePosition;
                    mOriginPos = Input.mousePosition;    // 누른 위치 저장

                    UiMGR.Ui_Update();
                }

                // 마우스 버튼 중
                if (Input.GetMouseButton(0))
                {
                    Mouse_Type = Mouse_Mode.ON_CLICK;
                    if ((mOriginPos - Input.mousePosition).sqrMagnitude > 150f)
                    {
                        Mouse_Type = Mouse_Mode.ON_DRAG;
                        mTouchPos = Input.mousePosition;
                    }
                }

                // 마우스 버튼 업
                if (Input.GetMouseButtonUp(0))
                {
                    Mouse_Type = Mouse_Mode.ON_EXIT;
                    mOriginPos = Vector3.zero;
                }
            }
            yield return null;
        }
    }

    public void LevelUp_Character_(Character info_)
    {
        Transform Character_pos=null;
        CharacterM tmp_character;
        GameObject tmp = null;

        // 상점 구매시 호출, 구매한 캐릭터의 정보로 개수를 추가한다.
        mCharacter_Count[info_.character_name]++;
       // Debug.Log(info_.character_name + " / " + mCharacter_Count[info_.character_name]);

        // 추가하고 개수가 3개인 경우 레벨업 진행
        if (mCharacter_Count[info_.character_name] ==3)
        {
            //배치된 캐릭터 분석
            for(int i = 0; i != Positioned_Character.Count;)
            {
                tmp_character = Positioned_Character[i].GetComponent<CharacterM>();

                // 같은 정보의 캐릭터를 제거
                if (tmp_character.Mcharacter_info.character_index.Equals(info_.character_index)) 
                {
                    Character_pos = tmp_character.transform;    // 배치된 캐릭터 위치 저장

                    //제거 작업
                    PoolMGR.Push_Pooling(tmp_character.gameObject, tmp_character.Mcharacter_info);
                    Character_origin_position.Remove(tmp_character.now_node.transform);
                    Positioned_Character.Remove(tmp_character.gameObject);
                    All_Character.Remove(tmp_character.gameObject);

                    is_Board = true;    // 보드위 설정

                    i = 0;  // 리스트 
                } 
                else
                {
                    i++;
                }
            }

            // 상점의 캐릭터 분석
            for(int i=0;i!=Storage_Character.Count;)
            {
                tmp_character = Storage_Character[i].GetComponent<CharacterM>();

                // if (tmp_character.Mcharacter_info.character_name == info_.character_name)
                if(tmp_character.Mcharacter_info.character_index.Equals(info_.character_index))
                {
                    PoolMGR.Push_Pooling(tmp_character.gameObject, tmp_character.Mcharacter_info);
                    Storage_Character.Remove(tmp_character.gameObject);
                    All_Character.Remove(tmp_character.gameObject);

                    // 배치된 캐릭터가 없는 경우
                    if (Character_pos == null)
                    {
                        Character_pos = tmp_character.transform;    // 창고의 캐릭터의 
                        is_Board = false;
                    }

                    i = 0;
                }
                else
                {
                    i++; 
                }    

            }

            BoardMGR.SpawnCharacter(info_, Character_pos, ref tmp);

            if(is_Board)
            {
                Positioned_Character.Add(tmp);
                Character_origin_position.Add(tmp.GetComponent<CharacterM>().Out_Node().transform);
            }
            else
            {
                Storage_Character.Add(tmp);
            }

            CharacterM tmp_character_ = tmp.GetComponent<CharacterM>();
            tmp_character_.Init();
            tmp_character_.On_board = is_Board;
            tmp_character_.mLevel++;
            UiMGR.StorageCheck();
            mCharacter_Count[info_.character_name] = 0;
        }
    }

    public Character Random_Character()
    {

        float input_value = Random.Range(0.0f,DataMGR.Max_Character_Range);
        float max_range_=0;
        float min_range=0;

        for (int i=0;i< DataMGR.character_count;i++)
        {
            max_range_+= DataMGR.Characters_Random_Range[i];

            if (min_range<input_value && input_value<max_range_)
            {
                return DataMGR.characters[i];
            }
            min_range = max_range_;
        }

        Debug.Log("랜덤에서 첫번째 캐릭터 반환함");
        return DataMGR.characters[0];
    }

    public void Random_value_update()
    {
        for (int i = 0; i < DataMGR.character_count; i++)
        {
            switch(DataMGR.characters[i].character_rating)
            {
                case Rare_percentage.Low_Level:
                    {
                        DataMGR.Characters_Random_Range[i] += (Game_round - 1) * (Rare_percentage.Low_Init_value);
                        DataMGR.Max_Character_Range += (Game_round-1)*(Rare_percentage.Low_Init_value);
                        break;
                    }

                case Rare_percentage.Middle_Level:
                    {
                        DataMGR.Characters_Random_Range[i] += (Game_round - 1) * (Rare_percentage.Middle_Init_value);
                        DataMGR.Max_Character_Range += (Game_round - 1) * (Rare_percentage.Middle_Init_value);
                        break;
                    }

                case Rare_percentage.High_Level:
                    {
                        DataMGR.Characters_Random_Range[i] += (Game_round - 1) * (Rare_percentage.High_Init_value);
                        DataMGR.Max_Character_Range += (Game_round - 1) * (Rare_percentage.High_Init_value);
                        break;
                    }
            }
        }

        for(int i = 0; i<DataMGR.character_count;i++)
        {
           // Debug.Log( DataMGR.characters[i].character_name + " / "+ DataMGR.Characters_Random_Range[i]);
        }
    }
}
