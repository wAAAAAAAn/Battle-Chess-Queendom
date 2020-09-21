public class Board_Define
{
    //게임 단계
    public const int GAME_INITIAL = 0;
    public const int GAME_READY = 1;
    public const int GAME_BATTLE = 2;
    public const int GAME_BATTEL_END = 3;
    public const int GAME_MOVE = 4;

    //라운드 승리/패배
    public const int WIN = 0;
    public const int LOOSE = 1;

    //배치 종류
    public const int NONE = -1;
    public const int ONBOARD = 0;
    public const int ONSTORAGE = 1;
    public const int ONSELLZONE = 2;

    //보스스테이지
    public const int BossStage =5;
}

public class Mouse_Mode
{
    public const int ON_CLICK = 1;
    public const int ON_CLICKING = 2;
    public const int ON_DRAG = 3;
    public const int ON_EXIT = 4;
}

public class Ani_Define
{
    //애니메이션 단계
    public const int IDLE = 0;
    public const int WALK = 1;
    public const int ATTACK = 2;
    public const int DIE = 3;
    public const int SKILL = 4;
    public const int VICTORY = 5;
}

public class Mouse_Ani_Define
{
    // 애니메이션 단계
    public const int IDLE = 0;
    public const int WALK = 1;
    public const int ATTACK1 = 2;
    public const int ATTACK2 = 3;
    public const int DIZZY = 4;
    public const int DEFEND = 5;
    public const int GETHIT = 6;
    public const int DIERECOVER = 7;
    public const int DIE = 8;
    public const int VICTORY = 9;
}

public class Unit_Battle_State
{
    public const int READY = -1;
    public const int FINDING = 0;   // 탐색
    public const int MOVE = 1;      // 이동
    public const int ATTACK = 2;    // 공격
    public const int DIE = 3;           //사망
}

public class Unit_Level
{
    public const int Level_1 = 0;
    public const int Level_2 = 1;
    public const int Level_3 = 2;
}

public class Unit_Attack_Type
{
    public const int Short_Range_Attack = 0;
    public const int Long_Range_Attack = 1;
}

public class Rare_percentage
{
    // 캐릭터 등장 등급
    public const int Low_Level = 1;
    public const int Middle_Level = 2;
    public const int High_Level = 3;

    //캐릭터 초기값
    public const float Low_Init_value = 1;
    public const float Middle_Init_value = 0.1f;
    public const float High_Init_value = 0;

    public const float Low_Variable_value = -0.18f;
    public const float Middle_Variable_value = 0.25f;
    public const float High_Variable_value = 0.3f;
}

public class Attack_Type
{
    public const int Short_attack = 0;
    public const int Long_attack = 1;
    public const int Short_Range = 2;
}

public class Enemy_Type
{
    public const int Short = 0;
    public const int Long = 1;
    public const int Difencer = 2;
    public const int Boss = 3;
}

public class Text_Define
{
    public const string STAGE = "STAGE ";
}
