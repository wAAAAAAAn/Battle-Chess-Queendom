using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    
    public List<Character> characters = new List<Character>();  // 캐릭터의 정보를 가질 리스트
    public List<Enemy> enemys = new List<Enemy>();  // 캐릭터의 정보를 가질 리스트
    public List<Stage> Stage_info = new List<Stage>();//스테이지의 정보
    public List<float> Characters_Random_Range = new List<float>();    // 캐릭터의 랜덤 범위

    public int character_count;    // 캐릭터의 개수
    public int enemy_count;
    public int round_count;

    public float Max_Character_Range = 0;

    List<Dictionary<string, object>> data;

    public void Data_First_Start(GameManager GameMGR)
    {
        data = CSVReader.Read("Character_data");    // CSV 파일 불러오기

        character_count = 0;    // 기본 0으로 초기화
        enemy_count = 0;

        for (int i = 0; i < data.Count; i++)
        {
            characters.Add(new Character());
            characters[i].Character_info_update(character_count++, data[i]["NAME"].ToString(),
                                                        int.Parse(data[i]["RATE"].ToString()),
                                                        int.Parse(data[i]["HP"].ToString()),
                                                        int.Parse(data[i]["DAMAGE"].ToString()),
                                                        float.Parse(data[i]["ATKRANGE"].ToString()),
                                                       int.Parse(data[i]["ATKTYPE"].ToString()),
                                                        int.Parse(data[i]["VARYMP"].ToString()));

            GameMGR.mCharacter_Count.Add(characters[i].character_name, 0);

            switch (characters[i].character_rating)
            {
                case Rare_percentage.Low_Level:
                    {
                        Characters_Random_Range.Add(Rare_percentage.Low_Init_value);
                        Max_Character_Range += Rare_percentage.Low_Init_value;
                        break;
                    }

                case Rare_percentage.Middle_Level:
                    {
                        Characters_Random_Range.Add(Rare_percentage.Middle_Init_value);
                        Max_Character_Range += Rare_percentage.Middle_Init_value;
                        break;
                    }

                case Rare_percentage.High_Level:
                    {
                        Characters_Random_Range.Add(Rare_percentage.High_Init_value);
                        Max_Character_Range += Rare_percentage.High_Init_value;
                        break;
                    }
            }

            // Debug.Log(characters[i].character_name + " / " + Characters_Random_Range[i]);
        }
        data = CSVReader.Read("Enemy_data");    // CSV 파일 불러오기

        for (int i = 0; i < data.Count; i++)
        {
            enemys.Add(new Enemy(enemy_count++, data[i]["NAME"].ToString(),
                                                    int.Parse(data[i]["HP"].ToString()),
                                                    int.Parse(data[i]["DAMAGE"].ToString()),
                                                    float.Parse(data[i]["ATKRANGE"].ToString()),
                                                    int.Parse(data[i]["VARYMP"].ToString())));
        }

        data = CSVReader.Read("Stage_Enemy");
        for (int i = 0; i < data.Count; i++)
        {
            Stage_info.Add(new Stage(int.Parse(data[i]["Round"].ToString()),
                                                 int.Parse(data[i]["EnemyWarrior"].ToString()),
                                                 int.Parse(data[i]["EnemyWizard"].ToString()),
                                                 int.Parse(data[i]["EnemyDifencer"].ToString()),
                                                 int.Parse(data[i]["ICE_DRAGON"].ToString())));
        }
    }

    //인덱스로 캐릭터 탐색
    public Character Character_find(int index_)
    {
        for (int i = 0; i < character_count; i++)
        {
            if (characters[i].character_index == index_)
            {
                return characters[i];
            }
        }
        return null;
    }

    //캐릭터 이름으로 캐릭터 탐색
    public Character Character_find(string name)
    {
        for (int i = 0; i < character_count; i++)
        {
            if (characters[i].character_name.Equals(name))
            {
                return characters[i];
            }
        }
        return null;
    }

    //인덱스로 적 탐색
    public Enemy Enemy_find(int index_)
    {
        for (int i = 0; i < enemy_count; i++)
        {
            if (enemys[i].enemy_index == index_)
            {
                return enemys[i];
            }
        }
        return null;
    }

    //캐릭터 이름으로 적 탐색
    public Enemy Enemy_find(string name)
    {
        for (int i = 0; i < enemy_count; i++)
        {
            if (enemys[i].enemy_name.Equals(name))
            {
                return enemys[i];
            }
        }
        return null;
    }

}
