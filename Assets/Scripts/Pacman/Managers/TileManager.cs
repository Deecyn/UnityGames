using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TileManager : MonoBehaviour
{

    public class Tile
    {
        public int x { get; set; }
        public int y { get; set; }
        public bool occupied { get; set; }
        public int adjacentCount { get; set; }
        public bool isIntersection { get; set; }

        public Tile left, right, up, down;

        // 初始化整个tile
        public Tile(int x_in, int y_in)
        {
            x = x_in; y = y_in;
            occupied = false;
            left = right = up = down = null;
        }


    };

    public List<Tile> tiles = new List<Tile>();

    // Use this for initialization
    void Start()
    {
        ReadTiles();

    }

    // Update is called once per frame
    void Update()
    {


    }


    // 0为墙 1为空
    void ReadTiles()
    {
        // 直接刚：）
        string data = @"0000000000000000000000000000
0111111111111001111111111110
0100001000001001000001000010
0100001000001111000001000010
0100001000001001000001000010
0111111111111001111111111110
0100001001000000001001000010
0100001001000000001001000010
0111111001111001111001111110
0001001000001001000001001000
0001001000001001000001001000
0111001111111111111111001110
0100001001000000001001000010
0100001001000000001001000010
0111111001000000001001111110
0100001001000000001001000010
0100001001000000001001000010
0111001001111111111001001110
0001001001000000001001001000
0001001001000000001001001000
0111111111111111111111111110
0100001000001001000001000010
0100001000001001000001000010
0111001111111001111111001110
0001001001000000001001001000
0001001001000000001001001000
0111111001111001111001111110
0100001000001001000001000010
0100001000001001000001000010
0111111111111111111111111110
0000000000000000000000000000";

        int X = 1, Y = 31;
        using (StringReader reader = new StringReader(data))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {

                X = 1; // 对每一行
                for (int i = 0; i < line.Length; ++i)
                {
                    Tile newTile = new Tile(X, Y);

                    // 如果tile有效就可以移动
                    if (line[i] == '1')
                    {
                        // 检查左右的tile是否有效
                        if (i != 0 && line[i - 1] == '1')
                        {
                            // 分配好左右的两个tile
                            newTile.left = tiles[tiles.Count - 1];
                            tiles[tiles.Count - 1].right = newTile;

                            // 调整每个tile的相邻数
                            newTile.adjacentCount++;
                            tiles[tiles.Count - 1].adjacentCount++;
                        }
                    }

                    // 如果newtile不可走
                    else newTile.occupied = true;

                    // 检查上下的tile 首先从第二行开始
                    int upNeighbor = tiles.Count - line.Length;
                    if (Y < 30 && !newTile.occupied && !tiles[upNeighbor].occupied)
                    {
                        tiles[upNeighbor].down = newTile;
                        newTile.up = tiles[upNeighbor];


                        newTile.adjacentCount++;
                        tiles[upNeighbor].adjacentCount++;
                    }

                    tiles.Add(newTile);
                    X++;
                }

                Y--;
            }
        }

        // 读完左右tile以后判定是否为交叉口
        foreach (Tile tile in tiles)
        {
            if (tile.adjacentCount > 2)
                tile.isIntersection = true;
        }

    }

    // 用来debug找像素与位置
    void DrawNeighbors()
    {
        foreach (Tile tile in tiles)
        {
            Vector3 pos = new Vector3(tile.x, tile.y, 0);
            Vector3 up = new Vector3(tile.x + 0.1f, tile.y + 1, 0);
            Vector3 down = new Vector3(tile.x - 0.1f, tile.y - 1, 0);
            Vector3 left = new Vector3(tile.x - 1, tile.y + 0.1f, 0);
            Vector3 right = new Vector3(tile.x + 1, tile.y - 0.1f, 0);

            if (tile.up != null) Debug.DrawLine(pos, up);
            if (tile.down != null) Debug.DrawLine(pos, down);
            if (tile.left != null) Debug.DrawLine(pos, left);
            if (tile.right != null) Debug.DrawLine(pos, right);
        }

    }


    // 根据坐标得到相应的tile值
    public int Index(int X, int Y)
    {

        //Debug.Log ("Index called for X: " + X + ", Y: " + Y);
        if (X >= 1 && X <= 28 && Y <= 31 && Y >= 1)
            return (31 - Y) * 28 + X - 1;

        // 在范围之外时
        if (X < 1) X = 1;
        if (X > 28) X = 28;
        if (Y < 1) Y = 1;
        if (Y > 31) Y = 31;

        return (31 - Y) * 28 + X - 1;
    }


    public int Index(Tile tile)
    {
        return (31 - tile.y) * 28 + tile.x - 1;
    }


    // 返回两个tile之间的距离
    public float distance(Tile tile1, Tile tile2)
    {
        return Mathf.Sqrt(Mathf.Pow(tile1.x - tile2.x, 2) + Mathf.Pow(tile1.y - tile2.y, 2));
    }
}
