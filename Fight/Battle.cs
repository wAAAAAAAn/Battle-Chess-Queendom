using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class Battle : MonoBehaviour
{
    public List<Node> mFinalPath;//The completed path that the red line will be drawn along

    //노드
    public Node now_node = null;
    [HideInInspector]
    public List<Node> mPath = null;
    protected const int path_index = 0;

    //public Transform MyTarget = null;
    private Reference_of_MGR ReferenceMGR;
    [HideInInspector]
    public GridManager GridMGR = null;//For referencing the grid class
    [HideInInspector]
    public GameManager GameMGR = null;
    [HideInInspector]
    public BoardManager BoardMGR = null;
    [HideInInspector]
    public ObjectPoolManager PoolMGR = null;
    [HideInInspector]
    public Animator ani_; // 애니메이션

    // 스킬 관련 변수들
    public IEnumerator mSKillBox = null;
    protected float mSkillTime = 0;

    protected virtual void Awake()
    {
        //인스턴스 캐싱
        ReferenceMGR = FindObjectOfType<Reference_of_MGR>();
        GameMGR = ReferenceMGR.Game_MGR;
        BoardMGR = ReferenceMGR.Board_MGR;
        GridMGR = ReferenceMGR.Grid_MGR;
        PoolMGR = ReferenceMGR.Pool_MGR;
    }

    /// <summary>
    /// 타겟을 넣어주는 함수
    /// </summary>
    /// <param name="Find_Target_List"></param>
    /// <returns></returns>
    public GameObject Find_Target(List<GameObject> Find_Target_List, Transform MyTrans)
    {
        float tmp;
        GameObject tmp_;

        //Debug.Log("총 타겟의 갯수 : " + Find_Target_List.Count);

        // 더 넣을 타겟이 없을때
        if (Find_Target_List.Count == 0)
        {
            //Debug.Log("타켓 없음");
            return null;
        }

        // 넣을 타겟이 있을때
        else if (Find_Target_List.Count != 0 || Find_Target_List.Count > 0)
        {
            //Debug.Log(MyTrans.position);
            tmp = Vector3.Distance(Find_Target_List[0].transform.position, MyTrans.position);
            tmp_ = Find_Target_List[0];

            for (int i = 0; i < Find_Target_List.Count; i++)
            {
                // Debug.Log(gameObject.name + " : "+ i);

                if (Find_Target_List[i].activeSelf == true)
                {
                    if (tmp >= Vector3.Distance(MyTrans.position, Find_Target_List[i].transform.position))
                    {
                        tmp_ = Find_Target_List[i];
                    }
                }
            }
            return tmp_;

        }
        return null;
    }

    /// <summary>
    /// 타겟을 바라보기
    /// </summary>
    /// <param name="target_"></param>
    /// <param name="MyTransform"></param>
    public void LookAt_Target(GameObject target_, ref Transform MyTransform)
    {
        Vector3 delta = target_.transform.position - MyTransform.position;
        Quaternion lookRot = Quaternion.LookRotation(delta);
        MyTransform.localRotation = Quaternion.Slerp(MyTransform.localRotation, lookRot, 5 * Time.deltaTime);
    }

    #region 경로 탐색 
    public List<Node> FindPath(Node a_StartPos, Node a_TargetPos)
    {
        Node StartNode = a_StartPos;//Gets the node closest to the starting position
        Node TargetNode = a_TargetPos;//Gets the node closest to the target position

        bool origin_state = TargetNode.bIsWall;
        if (TargetNode.bIsWall == false)
        {
            TargetNode.bIsWall = true;
        }

        List<Node> OpenList = new List<Node>();//List of nodes for the open list
        HashSet<Node> ClosedList = new HashSet<Node>();//Hashset of nodes for the closed list

        OpenList.Add(StartNode);//Add the starting node to the open list to begin the program

        while (OpenList.Count > 0)//Whilst there is something in the open list
        {
            Node CurrentNode = OpenList[0];//Create a node and set it to the first item in the open list
            for (int i = 1; i < OpenList.Count; i++)//Loop through the open list starting from the second object
            {
                if (OpenList[i].FCost < CurrentNode.FCost || OpenList[i].FCost == CurrentNode.FCost && OpenList[i].ihCost < CurrentNode.ihCost)//If the f cost of that object is less than or equal to the f cost of the current node
                {
                    CurrentNode = OpenList[i];//Set the current node to that object
                }
            }
            OpenList.Remove(CurrentNode);//Remove that from the open list
            ClosedList.Add(CurrentNode);//And add it to the closed list

            if (CurrentNode == TargetNode)//If the current node is the same as the target node
            {
                GetFinalPath(StartNode, TargetNode);//Calculate the final path
            }

            foreach (Node NeighborNode in GridMGR.GetNeighboringNodes(CurrentNode))//Loop through each neighbor of the current node
            {
                if (!NeighborNode.bIsWall || ClosedList.Contains(NeighborNode))//If the neighbor is a wall or has already been checked
                {
                    continue;//Skip it
                }
                int MoveCost = CurrentNode.igCost + GetManhattenDistance(CurrentNode, NeighborNode);//Get the F cost of that neighbor

                if (MoveCost < NeighborNode.igCost || !OpenList.Contains(NeighborNode))//If the f cost is greater than the g cost or it is not in the open list
                {
                    NeighborNode.igCost = MoveCost;//Set the g cost to the f cost
                    NeighborNode.ihCost = GetManhattenDistance(NeighborNode, TargetNode);//Set the h cost
                    NeighborNode.ParentNode = CurrentNode;//Set the parent of the node for retracing steps

                    if (!OpenList.Contains(NeighborNode))//If the neighbor is not in the openlist
                    {
                        OpenList.Add(NeighborNode);//Add it to the list
                    }
                }
            }
        }
        TargetNode.bIsWall = origin_state;

        return mFinalPath;
    }

    public void GetFinalPath(Node a_StartingNode, Node a_EndNode)
    {
        List<Node> FinalPath = new List<Node>();//List to hold the path sequentially 
        Node CurrentNode = a_EndNode;//Node to store the current node being checked

        while (CurrentNode != a_StartingNode)//While loop to work through each node going through the parents to the beginning of the path
        {
            FinalPath.Add(CurrentNode);//Add that node to the final path
            CurrentNode = CurrentNode.ParentNode;//Move onto its parent node
        }

        FinalPath.Reverse();//Reverse the path to get the correct order

        mFinalPath = FinalPath;//Set the final path
    }

    int GetManhattenDistance(Node a_nodeA, Node a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.iGridX - a_nodeB.iGridX);//x1-x2
        int iy = Mathf.Abs(a_nodeA.iGridY - a_nodeB.iGridY);//y1-y2

        return ix + iy;//Return the sum
    }
    #endregion

    public virtual void  Use_Skill()
    {

    }

    public virtual void Short_Attack()
    {

    }

    public virtual void Long_Attack()
    {

    }


}
    