using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int iGridX;//X Position in the Node Array
    public int iGridY;//Y Position in the Node Array

    public bool bIsWall;//Tells the program if this node is being obstructed.
    public bool bisEnemy;
    public bool bisCharacter;
    public Vector3 vPosition;//The world position of the node.

    public Node ParentNode;//For the AStar algoritm, will store what node it previously came from so it cn trace the shortest path.

    public int igCost;//The cost of moving to the next square.
    public int ihCost;//The distance to the goal from this node.

    public int FCost { get { return igCost + ihCost; } }//Quick get function to add G cost and H Cost, and since we'll never need to edit FCost, we dont need a set function.

    private GameManager GameMGR;
    public SpriteRenderer mLine;
    


    public void Node_Initial(bool a_bIsWall, Vector3 a_vPos, int a_igridX, int a_igridY)//Constructor
    {
        bIsWall = a_bIsWall;//Tells the program if this node is being obstructed.
        vPosition = a_vPos;//The world position of the node.
        iGridX = a_igridX;//X Position in the Node Array
        iGridY = a_igridY;//Y Position in the Node Array
    }

    private void Start()
    {
        GameMGR = FindObjectOfType<Reference_of_MGR>().Game_MGR;
        mLine = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(GameMGR._state == Board_Define.GAME_READY)
        {
            bIsWall = true;


            //// 노드 색상 적용중 1106
            //if(gameObject.layer.Equals(9))
            //{
            //    mLine.gameObject.SetActive(true);
            //}
        }

       else if (GameMGR._state == Board_Define.GAME_BATTLE  || GameMGR._state == Board_Define.GAME_BATTEL_END)
        {
            if (gameObject.layer.Equals(9))
            {
                mLine.gameObject.SetActive(false);
            }

        }

        
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.up, out hit, 3f, LayerMask.GetMask("Character")))
        {
            if (hit.collider.tag == "Enemy")
            {
                hit.collider.transform.parent.GetComponent<EnemyM>().now_node = this;
                bisEnemy = true;
            }

            if (hit.collider.tag == "Character")
            {
                hit.collider.transform.parent.GetComponent<CharacterM>().now_node = this;
                bisCharacter = true;

                if (gameObject.layer.Equals(9))
                {
                    // 보드에 최종 배치가 된 경우 1106
                    mLine.color = new Color(255,255,255,255);
                }
            }
        }
        else
        {
            bisCharacter = false;
            bisEnemy = false;

            if (gameObject.layer.Equals(9))
            {
                // 보드에 최종 배치가 된 경우 1106
                mLine.color = new Color(1, 1, 1,0.3f);
            }
        }
    }
}
