using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemFactory : MonoBehaviour
{
public GameObject gem;
    public Vector2Int tilePos;

    private void Start()
    {
        tilePos = Tile.instance.WorldToAxial(this.transform.position, Tile.instance.width);
    }
    public void CreateRefillGem()
    {
        GameObject gem = Instantiate(gameObject);
        gem.transform.position = this.transform.position;
        Vector2Int gemPos = tilePos;
        BoardManager.instance.gemPositions[gem] =gemPos;
        BoardManager.instance.gems[gemPos] = gem;

    }
}
