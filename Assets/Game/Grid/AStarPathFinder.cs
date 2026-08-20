using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class AStarPathFinder: Singleton<AStarPathFinder>
{
    private AStarPathFinder() { }
    public List<Node> FindPath(Vector3 startPos,Vector3 targetPos)
    {
        Node startNode = GridManager.Instance.GetNode(startPos);
        Node endNode = GridManager.Instance.GetNode(targetPos);
        List<Node> openList = new List<Node>();//发现但是还没有处理的节点
        List<Node> closeList = new List<Node>();//已经处理过的节点
        openList.Add(startNode);
        while(openList.Count > 0)
        {
            //找到F最小的节点
            Node currentNode = openList[0];
            for(int i = 1; i < openList.Count; i++)
            {
                Node node = openList[i];
                //F小的 或者是 F相同h小的
                if(node.fCost < currentNode.fCost || 
                    (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost))
                {
                    currentNode = node;
                }
            }
            openList.Remove(currentNode);
            closeList.Add(currentNode);
            //找到终点
            if(currentNode == endNode)
            {
                //这里是正常出口
                return RetracePath(startNode, endNode);
            }
            List<Node> neighbours = GridManager.Instance.GetNeighbours(currentNode);
            Debug.Log("当前的邻居" + neighbours.Count);
            foreach (Node neighbour in neighbours)
            {
                //如果已经走过就不考虑了
                if (closeList.Contains(neighbour))
                {
                    continue;
                }
                //新的gCost
                int newCost = currentNode.gCost + GetDistance(currentNode, neighbour);
                if( newCost < neighbour.gCost || !openList.Contains(neighbour))
                {
                    //如果该周围节点已经被发现 如果当前的路径比之前的路径gcost小 进行更新
                    //如果该周围节点还没有被遍历
                    neighbour.gCost = newCost;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;
                    if (!openList.Contains(neighbour))
                    {
                        openList.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 获取两个Node的距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int GetDistance(Node a,Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    /// <summary>
    /// 获取路径列表
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private List<Node> RetracePath(Node startNode,Node endNode)
    {
        List<Node> path = new List<Node>();
        //从后往前遍历
        Node currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(startNode);
        path.Reverse();//翻转
        //foreach(Node node in path)
        //{
        //    Debug.Log(node.x + ":" + node.y);
        //}
        return path;
    }
}
