using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Dictionary<string, int> Stage_info = new Dictionary<string, int>();
    public Vector3[,] Spawn_point = new Vector3[4, 8];   // 몬스터 생성 지점

    private GameManager gameMGR;
    private DataManager dataMGR;
    private GridManager gridMGR;

    public void Stage_First_Start(GameManager gameManager_)
    {
        gameMGR = gameManager_;
        dataMGR = gameManager_.DataMGR;
        gridMGR = gameManager_.GridMGR;

    }

    // 랜덤 좌표값 지정
    public void Random_position(ref Vector3 position_, int Type_)  // 0930 _ 랜덤 위치 
    {
        Vector3 tmp = Vector3.zero;

        bool is_overlap = false;

        while (true)
        {
            switch (Type_)
            {
                case Enemy_Type.Short:
                    {
                        // tmp = Spawn_point[Random.Range(1, 2), Random.Range(0, 7)];
                        tmp = gridMGR.NodeArray[Random.Range(0, 7), Random.Range(6, 5)].transform.position;
                        break;
                    }
                case Enemy_Type.Long:
                    {
                        //  tmp = Spawn_point[Random.Range(3, 3), Random.Range(0, 7)];
                        tmp = gridMGR.NodeArray[Random.Range(0, 7), 7].transform.position;

                        break;
                    }
                case Enemy_Type.Difencer:
                    {
                        //   tmp = Spawn_point[Random.Range(0, 0), Random.Range(0, 7)];
                        tmp = gridMGR.NodeArray[Random.Range(0, 7), 4].transform.position;

                        break;
                    }
                case Enemy_Type.Boss:
                    {
                        //tmp = Spawn_point[3,3];
                        tmp = gridMGR.NodeArray[3, 7].transform.position;
                        break;
                    }
            }

            is_overlap = false;

            for (int i = 0; i < gameMGR.AliveEnemy.Count; i++)
            {
                if (tmp == gameMGR.AliveEnemy[i].transform.position)
                {
                    is_overlap = true;
                }
            }

            if (is_overlap == false)
            {
                break;
            }
        }

        position_ = tmp;
    }


}
