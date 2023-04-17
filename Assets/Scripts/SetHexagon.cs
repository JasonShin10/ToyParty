using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetHexagon : MonoBehaviour
{
    public static SetHexagon instance;
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }

    }
    public GameObject hexPrefab;
    public int numRows = 7;
    public int numColumns = 7;
    private float scaleY;
    private float xOffset;
    private float yOffset;
    public List<GameObject> james = new List<GameObject>();
    public GameObject blue;
    public GameObject green;
    public GameObject red;
    public GameObject orange;
    public GameObject yellow;
    public GameObject purple;
    public GameObject[] arrayJames;
    public List<List<GameObject>> tiles = new List<List<GameObject>>();
    public List<GameObject> rowList = new List<GameObject>();
    public List<GameObject> roundCheck = new List<GameObject>();
    // 한블록 건너 네개인지 확인하는 보석
    public List<GameObject> fourCheck = new List<GameObject>();
    public List<GameObject> queue = new List<GameObject>();
    // 같은 잼 비교하기 위해 담는 리스트
    public List<GameObject> compareJames = new List<GameObject>();
    // 한블록 건너 네개인지 확인하는 보석
    public List<GameObject> jumpJames = new List<GameObject>();
    // 세개 잼이 연속으로 있는지 확인하는 리스트
    public List<GameObject> lineJames = new List<GameObject>();
    void Start()
    {
        arrayJames = new GameObject[] { blue, green, red, orange, yellow, purple };
        scaleY = hexPrefab.transform.localScale.y;
        xOffset = scaleY * Mathf.Sqrt(3) / 2;
        yOffset = scaleY * 3 / 4;
        for (int row = 0; row < numRows; row++)
        {
            rowList = new List<GameObject>();
            for (int col = 0; col < numColumns; col++)
            {
                if ((row == 0 && (col == 0)) || (row == 0 && (col == 1)) || (row == 0 && (col == 5)) || (row == 1 && (col == 0)) || (row == 1 && (col == 5)) || (row == 2 && (col == 0)) || (row == 4 && (col == 0)) || (row == 5 && (col == 0)) || (row == 5 && (col == 0)) || (row == 5 && (col == 5)) || (row == 6 && (col == 0) || (row == 6 && (col == 1)) || (row == 6 && (col == 5))))
                {
                    rowList.Add(null);
                    continue;

                }
                float xPos = col * xOffset;
                float yPos = row * yOffset;
                if (row % 2 == 1)
                {
                    xPos += xOffset / 2f;

                }
                Vector3 hexPos = new Vector3(xPos, 0f, yPos);
                GameObject hex = Instantiate(hexPrefab, hexPos, Quaternion.identity, transform);
                hex.name = string.Format("Hex ({0}, {1})", row, col);
                rowList.Add(hex);
            }
            tiles.Add(rowList);
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < rowList.Count; j++)
            {
                Debug.Log("(" + i + ", " + j + ") : " + tiles[i][j]);
            }
        }

        CreateJam();

        LocateJam();
    }
    void Update()
    {

    }

    public void CreateJam()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < rowList.Count; j++)
            {
                int randomNum = Random.Range(0, arrayJames.Length);
                james.Add(arrayJames[randomNum]);
            }
        }
    }

    public void LocateJam()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < rowList.Count; j++)
            {
                if (tiles[i][j] != null)
                {
                    Vector3 hexPos = tiles[i][j].transform.position;
                    GameObject jam = Instantiate(james[j], hexPos + new Vector3(0, 0.3f, 0), Quaternion.identity);
                    james.Remove(james[j]);
                }
            }
        }
    }

    public void SameJam(GameObject jam)
    {
        //jam.GetComponent<Jam>().hitObject
        //해당 Hexagon의 정보를 가져온다
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < rowList.Count; j++)
            {
                if (tiles[i][j] == jam)
                {
                    if ((i % 2 == 1))
                    {
                        compareJames.Add(tiles[i][j].GetComponent<TileRay>().color);
                        queue.Add(tiles[i][j].GetComponent<TileRay>().color);
                        jumpJames.Add(tiles[i][j].GetComponent<TileRay>().color);
                        lineJames.Add(tiles[i][j].GetComponent<TileRay>().color);

                        if ( j+1>= 0 && j + 1 <rowList.Count && j + 2 >= 0 && j + 2 < rowList.Count && tiles[i][j +1] && tiles[i][j + 2])
                        {
                            compareJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            queue.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j + 2].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 && j + 2 < rowList.Count && tiles[i - 1][j +2])
                        {
                            jumpJames.Add(tiles[i - 1][j + 2].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 &&  i - 2 >= 0 && j + 1 < rowList.Count && tiles[i - 1][j+1] && tiles[i - 2][j +1])
                        {
                            compareJames.Add(tiles[i - 1][j + 1].GetComponent<TileRay>().color);
                            queue.Add(tiles[i - 1][j + 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i - 1][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 1][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 2][j + 1].GetComponent<TileRay>().color);
                        }
                        if (i - 2 >= 0 && tiles[i - 2][j])
                        {
                            jumpJames.Add(tiles[i - 2][j].GetComponent<TileRay>().color);
                        }
                        if (i-1 >= 0 && i -2 >= 0 && j - 1 >= 0 && tiles[i - 1][j] && tiles[i - 2][j -1])
                        {
                            compareJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            queue.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 2][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i-1 >= 0 && j-1 >= 0 && tiles[i - 1][j -1])
                        {
                            jumpJames.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                        }
                        if (j-1 >= 0 && j-2 >= 0 && tiles[i][j -1] && tiles[i][j -2])
                        {
                            compareJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j - 2].GetComponent<TileRay>().color);
                        }
                        if (i + 1 < tiles.Count && j-1 >= 0 && tiles[i + 1][j -1])
                        {
                            jumpJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                        }
                        if (j-1 >= 0 && i+1 < tiles.Count && i+2 < tiles.Count && tiles[i + 1][j] && tiles[i + 2][j -1])
                        {
                            compareJames.Add(tiles[i + 1][j].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i + 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 2][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i + 2 < tiles.Count && tiles[i + 2][j])
                        {
                            jumpJames.Add(tiles[i + 2][j].GetComponent<TileRay>().color);
                        }
                        if (i + 2 < tiles.Count && i + 1 < tiles.Count && j+1 <rowList.Count && tiles[i + 1][j + 1] && tiles[i + 2][j +1])
                        {
                            compareJames.Add(tiles[i + 1][j + 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i + 1][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 1][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 2][j + 1].GetComponent<TileRay>().color);
                        }
                        if (i + 1 < tiles.Count && j + 2 < rowList.Count - 1 && tiles[i+1][j+2])
                        {
                            jumpJames.Add(tiles[i + 1][j + 2].GetComponent<TileRay>().color);
                        }
                    }
                    else
                    {
                        compareJames.Add(tiles[i][j].GetComponent<TileRay>().color);
                        queue.Add(tiles[i][j].GetComponent<TileRay>().color);
                        jumpJames.Add(tiles[i][j].GetComponent<TileRay>().color);
                        lineJames.Add(tiles[i][j].GetComponent<TileRay>().color);
                        if (j + 1 < rowList.Count && j +2 < rowList.Count - 1 && tiles[i][j + 1] && tiles[i][j +2])
                        {
                            compareJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            queue.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j + 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j + 2].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 && j+2 < rowList.Count -1 && tiles[i - 1][j + 2] != null)
                        {
                            jumpJames.Add(tiles[i - 1][j + 2].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 && i - 2 >= 0 && j  + 1 < rowList.Count - 1 && tiles[i - 1][j] != null && tiles[i - 2][j +1])
                        {
                            compareJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            queue.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 2][j + 1].GetComponent<TileRay>().color);

                        }
                        if (i -2 >= 0 && tiles[i - 2][j] != null)
                        {
                            jumpJames.Add(tiles[i - 2][j].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 && i -2 >= 0 && j-1 >= 0 && tiles[i - 1][j-1] != null && tiles[i - 2][j -1])
                        {
                            compareJames.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                            queue.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i - 2][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i - 1 >= 0 && j -1 >= 0 && tiles[i - 1][j-1] != null)
                        {
                            jumpJames.Add(tiles[i - 1][j - 1].GetComponent<TileRay>().color);
                        }
                        if (j - 2 >= 0 && j -1 >= 0 && tiles[i][j-2] != null && tiles[i][j-1] != null)
                        {
                            compareJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i][j - 2].GetComponent<TileRay>().color);
                        }
                        if (j - 1 >= 0 && i + 1 < tiles.Count && tiles[i + 1][j -1] != null)
                        {
                            jumpJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i + 1 < tiles.Count && j -1 < rowList.Count -1 && tiles[i+1][j -1] != null && tiles[i + 1][j-1] && tiles[i + 2][j -1])
                        {
                            compareJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 2][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i + 2 < tiles.Count)
                        {
                            jumpJames.Add(tiles[i + 2][j].GetComponent<TileRay>().color);
                        }
                        if (i + 1 < tiles.Count && j - 1 >= 0 && i + 2 < tiles.Count && tiles[i + 1][j] != null && tiles[i + 2][j -1] != null)
                        {   
                            compareJames.Add(tiles[i + 1][j].GetComponent<TileRay>().color);
                            jumpJames.Add(tiles[i + 1][j].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 1][j - 1].GetComponent<TileRay>().color);
                            lineJames.Add(tiles[i + 2][j - 1].GetComponent<TileRay>().color);
                        }
                        if (i + 1 < tiles.Count && j +2 < rowList.Count - 1)
                        {
                            jumpJames.Add(tiles[i + 1][j + 2].GetComponent<TileRay>().color);
                        }

                    }

                    break;
                }
            }
        }
        FourJam();
        FourAboveCheck();
        if (lineJames.Count != 0)
        {
        LineCheck();
        }

        //해당 Hexagon이 가진 jam정보를 주변 헥사곤 애들과 비교한다.
        //해당 핵사곤의 위치를 먼저 찾아야 겠지?
        //리스트 어디간에 있을텐데 이름으로 찾으면 될거같은데

    }
    int a = 0;
    public void FourJam()
    {
        Dictionary<string, int> count = new Dictionary<string, int>();

        foreach (GameObject jam in compareJames)
        {
            if (!count.ContainsKey(jam.name))
            {
                count[jam.name] = 1;
            }
            else
            {
                count[jam.name]++;

            }

            if (count[jam.name] >= 3)
            {
                foreach (GameObject sameJam in compareJames)
                {
                    if (sameJam.name == jam.name && !roundCheck.Contains(sameJam))
                    {
                        roundCheck.Add(sameJam);
                    }
                }
            }

            //if (count[jam.name] >= 2)
            //{
            //    foreach (GameObject sameJam in compareJames)
            //    {
            //        if (sameJam.name == jam.name && !fourCheck.Contains(sameJam))
            //        {
            //            fourCheck.Add(sameJam);
            //        }
            //    }
            //}
        }
        if (roundCheck.Count != 0)
        {
            jamEqualCheck();
        }


    }

    public void jamEqualCheck()
    {
        roundCheck.RemoveAt(0);
        queue.RemoveAt(0);
        for (int i = 0; i < compareJames.Count; i++)
        {
            bool listEqual = roundCheck.SequenceEqual(queue);
            if (listEqual == true)
            {
                print("같다1");
                Destroy(compareJames[0]);
                Destroy(queue[0]);
                Destroy(queue[1]);
                Destroy(queue[2]);
                break;
            }
            else
            {
                a = i + compareJames.Count - queue.Count - 1;
                if (a >= compareJames.Count - 1)
                {
                    a = i - (compareJames.Count - queue.Count - 1);
                }
                queue.RemoveAt(0);
                queue.Add(compareJames[a + 1]);
            }

        }
    }
    public void FourAboveCheck()
    {
        for (int i = 1; i < jumpJames.Count - 2; i++)
        {
            if (jumpJames[i].name == jumpJames[i + 1].name && jumpJames[i].name == jumpJames[i + 2].name)
            {
                if (jumpJames[0].name == jumpJames[i].name)
                {

                    print("같다2");
                    Destroy(jumpJames[0]);
                    Destroy(jumpJames[i]);
                    Destroy(jumpJames[i + 1]);
                    Destroy(jumpJames[i + 2]);
                    break;
                }
            }
        }


    }

    public void LineCheck()
    {
        for (int i = 1; i< lineJames.Count -2; i++)
        {
            if (lineJames[i].name == lineJames[i +1].name)
            {
                if (lineJames[0].name == james[i].name)
                {
                    print("같다3");
                    Destroy(lineJames[0]);
                    Destroy(lineJames[i]);
                    Destroy(lineJames[i+1]);
                }
            }
        }
    }


}
