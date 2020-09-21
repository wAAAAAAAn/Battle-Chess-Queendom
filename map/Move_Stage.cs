using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Move_Stage : MonoBehaviour
{
    private List<float> mMove_Pos = new List<float>();
    public GameManager gameMGR;
    private Transform mTransform;
    private Coroutine Move_coru =null;
    public GameObject Setoff_obj;
    public FadeControl mfadeControl;

    //카메라
    public Canvas canvas_;
    public Camera BossCam_;

    //마법진 
    public GameObject circle_a;
    public GameObject circle_b;

    //라운드 텍스트, 애니메이션
    public Text Round_Text;
    public Animator Round_Animation;

    void Start()
    {
        mMove_Pos.Add(20);  //2스테이지
        mMove_Pos.Add(40);  //3스테이지
        mMove_Pos.Add(60);  //4스테이지
        mMove_Pos.Add(100);  //5스테이지

        mTransform = transform;
    }

    public void Start_Stage_Move()
    {
        if (Move_coru == null)
        {
            Move_coru = StartCoroutine(Move_Stage_coru());
        }
    }

    private IEnumerator Move_Stage_coru()
    {
        Setoff_obj.SetActive(false);
        gameMGR.UiMGR.mShop.onoff_shop(false);

        if (gameMGR.Game_round  == 5)
        {
            while (true)
            {
                mTransform.position = Vector3.Lerp(mTransform.position, new Vector3(2, 0, 75), 0.025f);

                if (75 - transform.position.z < 1)
                {
                    Move_coru = null;
                    Round_Animation.SetBool("FADE", false);

                    circle_a.SetActive(true);
                    circle_b.SetActive(true);

                    yield return new WaitForSeconds(4);
                    mfadeControl.gameObject.SetActive(true);
                    mfadeControl.Fadeout_start();
                    yield return new WaitForSeconds(3);
                    Camera.main.gameObject.SetActive(false);
                    canvas_.worldCamera = BossCam_;
                    BossCam_.gameObject.SetActive(true);
                    mfadeControl.Fadein_start();


                    Setoff_obj.SetActive(true);
                    gameMGR._state = Board_Define.GAME_READY;
                    mTransform.position = new Vector3(0, 0, mMove_Pos[gameMGR.Game_round - 2]);
                    yield break;
                }
                yield return null;
            }
        }
        else
        {
            Round_Text.text = Text_Define.STAGE + gameMGR.Game_round;
            Round_Animation.SetBool("FADE", true);

            while (true)
            {
                //transform.Translate(Vector3.forward * Time.deltaTime*0.1f);
                mTransform.position = Vector3.Lerp(mTransform.position, new Vector3(0, 0, mMove_Pos[gameMGR.Game_round - 2]), 0.03f);

                Debug.Log(gameMGR.Game_round);
                if (mMove_Pos[gameMGR.Game_round - 2] - transform.position.z < 1)
                {
                    Setoff_obj.SetActive(true);
                    //mTransform.position = new Vector3(0, 0, mMove_Pos[gameMGR.Game_round-2]);
                    gameMGR._state = Board_Define.GAME_READY;
                    Move_coru = null;
                    Round_Animation.SetBool("FADE", false);
                    yield break;
                }

                yield return null;
            }
        }
       
    }

    
}
