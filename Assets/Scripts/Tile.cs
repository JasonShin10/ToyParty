using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class Tile : MonoBehaviour
{
    public static Tile instance;
    public GameObject hexPrefab;
    public GameObject[] gemPrefabs;
    public GameObject newGem;


    public float width;
    private const int MIN_Q = -3;
    private const int MAX_Q = 3;
    private const int MIN_EXTRA_Q = -9;

    private const float Z_OFFSET = -0.25f;
     private const float HexWidthFactor = 0.75f;
    private const float HexHeightFactor = 0.5f;

    private Dictionary<Vector2Int, GameObject> gems = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<GameObject, Vector2Int> gemPositions = new Dictionary<GameObject, Vector2Int>();

    public List<Vector2Int> gemFactoryTiles = new List<Vector2Int>();

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }
    private void Start()
    {
        BoardManager.instance.RegisterTileScript(this);
        CreateTiles();
        CreateGems();  
    }

    public Dictionary<Vector2Int, GameObject> Gems { get { return gems; } }
    public Dictionary<GameObject, Vector2Int> GemPositions { get { return gemPositions; } }

    public Dictionary<Vector2Int, GameObject> newGems = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<GameObject, Vector2Int> newGemsPos = new Dictionary<GameObject, Vector2Int>();

    private void CreateTiles()
    {
        width = hexPrefab.transform.localScale.y;

        for (int q = MIN_Q; q <= MAX_Q; q++)
        {
            int r1 = Mathf.Max(MIN_Q, -q + MIN_Q);
            int r2 = Mathf.Min(MAX_Q, -q + MAX_Q);

            for (int r = r1; r < r2; r++)
            {            
                Vector2Int axialCoord = new Vector2Int(q, r);
                Vector3 worldPos = AxialToWorld(axialCoord, width);

                GameObject tile = Instantiate(hexPrefab,transform);
                tile.transform.position = worldPos;
                tile.name = string.Format("Tile {0} , {1}", q, r);
            }
        }
    }


    public bool SkipTile(int q, int r)
    {
        if (q == -3 && Mathf.Abs(r) > 3)
        {
            return true;
        }
        else if (q == -2 && Mathf.Abs(r) > 5)
        {
            return true;
        }
        else if (q == -1 && Mathf.Abs(r) > 7)
        {
            return true;
        }
        else if (q == 1 && Mathf.Abs(r) > 8)
        {
            return true;
        }
        else if (q == 2 && Mathf.Abs(r) > 7)
        {
            return true;
        }
        else if (q == 3 && Mathf.Abs(r) > 6)
        {
            return true;
        }
        return false;
    }

    private void CreateGems()
    {       
        float scaleY = hexPrefab.transform.localScale.y;

        for (int q = MIN_Q; q <= MAX_Q; q++)
        {
            int r1 = Mathf.Max(MIN_EXTRA_Q, -q + MIN_EXTRA_Q);
            int r2 = Mathf.Min(MAX_Q, -q + MAX_Q);

            for (int r = r1; r < r2; r++)
            {
                if (SkipTile(q, r))
                {
                    continue;
                }
                Vector2Int axialCoord = new Vector2Int(q, r);
                Vector3 worldPos = AxialToWorld(axialCoord, scaleY);

                GameObject gem = CreateGemAtPosition(worldPos, axialCoord, Z_OFFSET);

                if (!IsPositionExcluded(q, r))
                {
                    gemFactoryTiles.Add(axialCoord);
                    newGems[axialCoord] = gem;
                    newGemsPos[gem] = axialCoord;
                }
                else
                {
                    gems[axialCoord] = gem;
                    gemPositions[gem] = axialCoord;
                }
               CheckAndDestroyGem(ref r, gem, axialCoord);
            }
        }
    }

    private GameObject CreateGemAtPosition(Vector3 worldPos, Vector2Int axialCoord, float zOffset)
    {
        int randomIndex = Random.Range(0, gemPrefabs.Length);
        GameObject gem = Instantiate(gemPrefabs[randomIndex], newGem.transform);
        gem.transform.position = worldPos + new Vector3(0, 0, zOffset);
        gem.name = $"{gem.tag} {axialCoord.x} , {axialCoord.y}";

        return gem;
    }

    private bool IsPositionExcluded(int q, int r)
    {
        int excludeR1 = Mathf.Max(MIN_Q, -q + MIN_Q);
        int excludeR2 = Mathf.Min(MAX_Q, -q + MAX_Q);

        return q >= MIN_Q && q <= MAX_Q && r >= excludeR1 && r < excludeR2;
    }

    private void CheckAndDestroyGem(ref int r, GameObject gem, Vector2Int axialCoord)
    {
        bool isMatched = BoardManager.instance.CheckForMatches(axialCoord, out List<GameObject> destroyGems);

        if (!isMatched)
        {
            r--;
            Destroy(gem);
            gems.Remove(axialCoord);
            gemPositions.Remove(gem);
        }
    }








    public Vector3 AxialToWorld(Vector2Int axialCoord, float width)
    {
        float x = axialCoord.x * width * HexWidthFactor;
        float y = (axialCoord.y + (axialCoord.x / 2.0f)) * Mathf.Sqrt(3) * width * HexHeightFactor;

        return new Vector3(x, -y, -Z_OFFSET);
    }
    public Vector2Int WorldToAxial(Vector3 worldPos, float width)
    {
        float x = worldPos.x / (width * HexWidthFactor);
        float y = -worldPos.y / (Mathf.Sqrt(3) * width * HexHeightFactor) - (x / 2.0f);

        int q = Mathf.RoundToInt(x);
        int r = Mathf.RoundToInt(y);

        return new Vector2Int(q, r);
    }

    public void OnResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}