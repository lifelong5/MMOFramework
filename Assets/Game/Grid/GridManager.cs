using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node
{
    //先实现平面的 之后再考虑加入z值
    public int x;
    public int y;

    public int gCost;//起点到当前点的代价
    public int hCost;//当前点到目标点的消耗

    public Node parent;//父节点
    public bool walkable;//是否可以行走
    //F = G + H
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public Node(int x,int y,bool walkable)
    {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }
}
public class GridManager : Singleton<GridManager>
{
    //长宽的格子数
    public int width = 50;
    public int height = 50;
    //每个Grid的宽度
    public float cellSize = 1f;
    //node数组
    private Node[,] nodes;

    private GridManager()
    {
        createNodes();
    }
    /// <summary>
    /// Node数组初始化
    /// </summary>
    private void createNodes()
    {
        nodes = new Node[width, height];
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3 worldPosition = GetWorldPosition(x, y);
                //判断当前位置是否有BoxCollider如果有表示该地方不能走
                //bool walkale = !Physics.CheckBox(worldPosition, Vector3.one * cellSize * 0.4f);
                nodes[x, y] = new Node(x, y, true);
            }
        }
    }
    /// <summary>
    /// 获得对应的世界坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector3 GetWorldPosition(int x,int y)
    {
        return new Vector3(x * cellSize, 0.5f, y * cellSize);
    }
    /// <summary>
    /// 通过下标获取Node
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Node GetNode(int x,int y)
    {
        if((x>=0 && x<width) && (y>=0 && y < height))
        {
            return nodes[x, y];
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 通过世界坐标获取Node
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Node GetNode(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);//横下标
        int y = Mathf.RoundToInt(worldPosition.z / cellSize);//纵下标
        if ((x >= 0 && x < width) && (y >= 0 && y < height))
        {
            return nodes[x, y];
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 获取Node周围的可以移动点
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        //暂时只考虑四个方向 二维数组
        int[,] directions = {
            {0,1 },
            {0,-1},
            {-1,0},
            {1,0}
        };
        for(int i = 0; i < 4; i++)
        {
            int x = node.x + directions[i, 0];
            int y = node.y + directions[i, 1];
            Node neighbour = GetNode(x, y);
            if (neighbour != null && neighbour.walkable)
            {
                neighbours.Add(neighbour);
            }
        }
        return neighbours;
    }
}
