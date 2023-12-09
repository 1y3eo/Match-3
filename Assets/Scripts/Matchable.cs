using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private Cursor cursor;
    private MatchablePool pool;
    private MatchableGrid grid;

    private int type;
    public int Type => type;


    private MatchType powerup = MatchType.invalid;

    public bool IsBomb
    {
        get
        {
            return powerup == MatchType.match4;
        }
    }

    private SpriteRenderer spriteRenderer;

    public Vector2Int position;

    private void Awake()
    {
        cursor = Cursor.Instance;
        pool = (MatchablePool)MatchablePool.Instance;
        grid = (MatchableGrid)MatchableGrid.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    public void SetType(int type, string sprite, int color)
    {
        this.type = type;

        Sprite[] all = Resources.LoadAll<Sprite>("Sprites/" + sprite);
        spriteRenderer.sprite = all[color];
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {

        cursor.spriteRenderer.enabled = false;

        // bomb
        if (powerup != MatchType.invalid)
        {
            powerup = MatchType.invalid;
        }

        if (collectionPoint == null)
            yield break;

        spriteRenderer.sortingOrder = 2;
        yield return StartCoroutine(MoveToTransform(collectionPoint));

        spriteRenderer.sortingOrder = 1;

        pool.ReturnObjectToPool(this);
    }


    public Matchable Upgrade(MatchType powerupType, Sprite powerupSprite)
    {
        if (powerup != MatchType.invalid)
        {
            idle = false;
            StartCoroutine(Resolve(null));
            idle = true;

            type = -1;
            spriteRenderer.color = Color.white;
        }

        powerup = powerupType;
        spriteRenderer.sprite = powerupSprite;

        return this;
    }

    public int SortingOrder
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }
    }

    private void OnMouseDown()
    {
        cursor.SelectFirst(this);
    }

    private void OnMouseUp()
    {
        cursor.SelectFirst(null);
    }

    private void OnMouseEnter()
    {
        cursor.SelectSecond(this);
    }

    public override string ToString()
    {
        return gameObject.name;
    }
}
