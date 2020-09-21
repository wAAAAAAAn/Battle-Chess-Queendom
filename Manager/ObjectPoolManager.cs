using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 [ 09-03] 
 * 캐릭터를 제외한 생성/제거 할 오브젝트들(이펙트, 몬스터)을 미리 생성하여 활용하는 매니저
 * 캐릭터의 경우 종류가 많고 플레이어가 여러 개를 만들기 때문에 풀매니저에서 결정하지 않음
 * 특정 좌표를 설정해두고 그 위치중에서 랜덤으로 몬스터를 생성함
 */


// 이름을 체스포지션매니저(예정)
public class ObjectPoolManager : MonoBehaviour
{
    //public Vector3[,] position_point = new Vector3[8, 8];   // 보드내 모든 좌표

    //public bool[,] is_positioned_unit = new bool[8, 8];  //현재 배치된 곳이 유닛이 있는지 

    public int Character_max_poolobject = 20;  //초기 생성될 오브젝트 개수
    public int Enemy_max_poolobject = 10;  //초기 생성될 오브젝트 개수
    public int Arrow_max_poolobject = 5; // 초기 생성될 화살 개수

    public GameObject Character_poolFolder;
    public GameObject Enemy_poolFolder;
    public GameObject Arrow_poolFolder; // 아처 스킬 준비

    //public Dictionary<string, GameObject> mCharacter_Pool = new Dictionary<string, GameObject>();
    //public Dictionary<string, GameObject> mEnemy_Pool = new Dictionary<string, GameObject>();

    public GameObject character_unit;   // 생성할 캐릭터 프리펩
    public GameObject enemy_unit;   // 생성할 적 유닛 프리펩
    public GameObject Archer_Arrow; // 생성할 아처 화살 프리펩   

    public List<GameObject> Character_Unit_List = new List<GameObject>();
    public List<GameObject> Enemy_Unit_List = new List<GameObject>();
    public List<GameObject> Archor_Arrow_List = new List<GameObject>(); // 아처 스킬 준비

    private Reference_of_MGR ReferenceMGR;
    private GameManager gameMGR;
    private DataManager dataMGR;

    public void ObjectPool_First_Start(GameManager GameMGR)
    {
        gameMGR = GameMGR;
        dataMGR = GameMGR.DataMGR;

        character_unit = Resources.Load<GameObject>("Unit/" + "Character_unit");
        enemy_unit = Resources.Load<GameObject>("Unit/" + "Enemy_unit");
        Archer_Arrow = Resources.Load<GameObject>("Unit/" + "Archer_Arrow"); // 아처 스킬 준비


        // 아처 스킬 준비를 위한 버러지


        Init_Pooling();
    }


    private void Init_Pooling()
    {
        for (int i = 0; i < Character_max_poolobject; i++)
        {
            Character_Unit_List.Add(Instantiate(character_unit, Character_poolFolder.transform));   // 새로운 캐릭터 유닛 생성

            for (int j = 0; j < dataMGR.characters.Count; j++)
            {
                CharacterM character_ = Character_Unit_List[i].GetComponent<CharacterM>();
                string character_name = dataMGR.characters[j].character_name;
                //  Debug.Log(character_name);
                character_.Character_Models.Add(character_name, character_.transform.Find(character_name).gameObject);
                character_.Character_Models[character_name].SetActive(false);
            }

            Character_Unit_List[i].SetActive(false);
        }

        for (int i = 0; i < Enemy_max_poolobject; i++)
        {
            Enemy_Unit_List.Add(Instantiate(enemy_unit, Enemy_poolFolder.transform));   // 새로운 적 유닛 생성

            for (int j = 0; j < dataMGR.enemys.Count; j++)
            {
                EnemyM enemy_ = Enemy_Unit_List[i].GetComponent<EnemyM>();
                string enemy_name = dataMGR.enemys[j].enemy_name;
                enemy_.Enemy_Models.Add(enemy_name, enemy_.transform.Find(enemy_name).gameObject);
                enemy_.Enemy_Models[enemy_name].SetActive(false);
            }

            Enemy_Unit_List[i].SetActive(false);
        }

        // 지정된 수만큼 아처 오브젝트 생성

        for (int i = 0; i < Arrow_max_poolobject; i++)
        {
            Archor_Arrow_List.Add(Instantiate(Archer_Arrow, Arrow_poolFolder.transform));
        }
    }

    #region Archer_Arrow Pooling
    public void Pop_Pooling(CharacterM character_)
    {
        if (character_.MyTarget != null)
        {
            // 화살을 리스트에서 꺼낸다
            GameObject Return_Arrow = Archor_Arrow_List[0];
            Archor_Arrow_List.RemoveAt(0);

            // 화살의 위치를 잡는다
            Return_Arrow.transform.position = character_.mModel.transform.position;
            Debug.Log("mBullet_POS :" + character_.mBullet_POS);
            // 화살이 날릴 타겟의 방향을 바라본다
            Return_Arrow.transform.LookAt(character_.MyTarget.transform);

            // 화살을 킨다
            Return_Arrow.SetActive(true);

            // 날아가는 것을 바라본다.(우리가)
            Debug.Log("날아간다~");
        }
    }

    public void Push_Pooling(GameObject Arrow_)
    {
        Arrow_.SetActive(false);

        Archor_Arrow_List.Add(Arrow_);
    }
    #endregion

    #region Character Pooling
    public GameObject Pop_Pooling(Character info_)
    {
        // 캐릭터 정보에 따라 모델 활성화
        // 유닛 자식 순서 변경
        // 오브젝트 반환

        GameObject Return_Unit = Character_Unit_List[0];
        CharacterM character = Return_Unit.GetComponent<CharacterM>();
        GameObject model = character.Character_Models[info_.character_name];

        Return_Unit.SetActive(true);
        character.Character_Models[info_.character_name].SetActive(true);

        character.mModel = model;
        character.mModel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        character.mModel.transform.rotation = Quaternion.identity;

        character.Character_info_update(info_);
        Character_Unit_List.Remove(Return_Unit);

        return Return_Unit;
    }

    public void Push_Pooling(GameObject unit, Character info_)
    {
        CharacterM Char_ = unit.GetComponent<CharacterM>();

        unit.transform.parent = Character_poolFolder.transform;
        Char_.mModel.SetActive(false);

        Char_.mModel.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        Char_.mModel.transform.rotation = Quaternion.identity;

        Char_.mModel = null;
        unit.SetActive(false);

        Character_Unit_List.Add(unit);
    }
    #endregion

    #region Enemy Pooling
    public GameObject Pop_Pooling(Enemy info_)
    {
        GameObject Return_Unit = Enemy_Unit_List[0];
        EnemyM enemy = Return_Unit.GetComponent<EnemyM>();
        GameObject model = enemy.Enemy_Models[info_.enemy_name];


        Return_Unit.SetActive(true);
        model.SetActive(true);
        enemy.mModel = model;
        Enemy_Unit_List.Remove(Return_Unit);
        enemy.Enemy_info_update(info_);
        return Return_Unit;
    }

    public void Push_Pooling(GameObject unit, Enemy info_)
    {
        EnemyM enemy_ = unit.GetComponent<EnemyM>();

        //모델 비활성 후 null
        enemy_.mModel.SetActive(false);
        enemy_.mModel=null;
        enemy_.Menemy_info.Enemy_delete();


        //유닛 비활성 후 리스트 추가
        unit.transform.parent = Enemy_poolFolder.transform;
        unit.SetActive(false);
        Enemy_Unit_List.Add(unit);
    }

    #endregion
}