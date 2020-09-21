using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Ui_manager : MonoBehaviour
{
    public GameManager GameMGR; // 체스 보드 매니저
    public BoardManager BoardMGR;
    public DataManager DataMGR;
    public _shop mShop; // 상점 오브젝트
    private _storage mStorage; // 상점 오브젝트

    public GameObject Exit_Panel;
    public bool Initial_Flag = false;

    public Text timer_;
    public Image Stage_image;
    public GameObject WOTCM_Healthbar;

    public Sprite[] Stage_sprites = new Sprite[2];
    //public Image  Battle_sprite;

    public Sprite Battle_sprite_image;
    public Sprite Empty_image;

    public GameObject Result_Panel;

    public AudioSource mAudioSource;

    public GameObject Video_Panel;
    public VideoPlayer mVPlayer;
    public VideoClip Vitory_clip;
    public VideoClip Defeated_clip;
    public Text Video_Text;

    // 툴팁

    //ui 캐릭터 (오브젝트 풀 매니저 교체 예정, wan)
    //public List<GameObject> Ui_character_List = new List<GameObject>();
    public Transform Ui_character_;

    public List<GameObject> shop_slots = new List<GameObject>(); //창고 슬롯 리스트


    [HideInInspector]
    public Transform mEmptyPos;

    
    public Camera mCamera;  //메인 카메라로 교체 예정, 1017 피드백
    
    public void UI_First_Start(GameManager GameMGR_)
    {
        GameMGR = GameMGR_;
        BoardMGR = GameMGR_.BoardMGR;
        DataMGR = GameMGR_.DataMGR;
        mAudioSource = GetComponent<AudioSource>();

        //mCamera = GameObject.Find("ui_Camera").GetComponent<Camera>();
        mShop = GameObject.Find("shop panel").GetComponent<_shop>();
        mStorage = GameObject.Find("Storage").GetComponent<_storage>();

        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0, 180, 0);

        // 각 오브젝트를 초기화
        mShop.initial_shop();
    }

    #region ui 업데이트 관련


   public void Ui_Reddy()
    {
        Initial_ui();
        on_Timer_UI();
    }

    public void Ui_End()
    {
        Initial_Flag = false;
        Stage_image.sprite = Stage_sprites[0];
        if (GameMGR.Game_END== true)
        {
            GameMGR.mFadeControl.gameObject.SetActive(true);
            GameMGR.mFadeControl.Result_Fadeout_start(this);    // 1111  보스 라운드 클리어시 승리 비디오
        }
    }

    public void Ui_Update()
    {
        mShop.focusout_by_mouse();
    }
    
    void on_Timer_UI()
    {
            GameMGR.time -= Time.deltaTime; // 시간에 따라 감소
            timer_.text = Mathf.Ceil(GameMGR.time).ToString();  // 정수형으로 반환하여 텍스트 변경
         

        if (GameMGR.time < 4&& GameMGR.time>3)
        {
            mAudioSource.Play();
        }

        else if (GameMGR.time < 0)   // 시간초가 다된경우 전투 상태로 전환, 텍스트 전투중으로 변경
            {

                GameMGR._state = Board_Define.GAME_BATTLE;
                GameMGR.Game_round++;
                Stage_image.sprite = Stage_sprites[1];
                GameMGR.time = 30;
                timer_.gameObject.SetActive(false);
                BoardMGR.is_state_initial = false;
            }
    }

    public void Initial_ui()
    {
        if (Initial_Flag == false)
        {
            GameMGR.time = 30;
            timer_.gameObject.SetActive(true);
            Stage_image.sprite = Stage_sprites[0];
            //Stage_image.sprite = Stage_sprites[GameMGR.Game_round-1];
            GameMGR.Player_HP += 10;
            mShop.Reroll_shop();
            Initial_Flag = true;
        }
    }

    public void StorageCheck()
    {
        mStorage.Examine_all_storage();
    }

    public void Set_Off_uI_BOSS()
    {
        WOTCM_Healthbar.gameObject.SetActive(false);
        Stage_image.gameObject.SetActive(false);
        timer_.gameObject.SetActive(false);
    }

    // 게임 재시작
    public void Game_Restart()
    {
        GameMGR.Game_Initial();
        Result_Panel.SetActive(false);
    }

    // 게임 종료
    public void Game_Exit()
    {
        Application.Quit();
    }

    public void Go_To_MAIN()
    {
        SceneManager.LoadScene("MENU_START");
    }

    public void Video_setting()
    {
        if (GameMGR.game_Result == Board_Define.WIN)
        {
            mVPlayer.clip = Vitory_clip;
            Video_Text.text = "승 리";
        }

        else if (GameMGR.game_Result == Board_Define.LOOSE)
        {
            mVPlayer.clip = Defeated_clip;
            Video_Text.text = "패 배";
        }
    }

    public void Video_play()
    {
        Video_Panel.SetActive(true);
        mVPlayer.Play();
    }

    public void Exit_Game_panel_Set()
    {
        mShop.onoff_shop(false);
        Exit_Panel.SetActive(!Exit_Panel.activeSelf);
    }

    public void Exit_Game_Exit()
    {
        Application.Quit();
    }

    public void Skip_Dispose()
    {
        GameMGR.time = 0;
    }

    #endregion


}
