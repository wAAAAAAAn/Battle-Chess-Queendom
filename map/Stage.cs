using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[System.Serializable]
public class Stage
{
    public int Round = 0;
    public int EnemyWarrior_count=0;
    public int EnemyWizard_count=0;
    public int EnemyDifencer_count=0;
    public int Boss_count=0;

    public Stage(int round_, int warrior_,int wizard_, int difencer_, int boss_)
    {
        Round = round_;
        EnemyWarrior_count = warrior_;
        EnemyWizard_count = wizard_;
        EnemyDifencer_count = difencer_;
        Boss_count = boss_;
    }
}
    