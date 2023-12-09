using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private string[] sprite_text;
    [SerializeField] private Sprite Bomb;


    public void RandomizeType(Matchable toRandomize)
    {
        int random = Random.Range(0, howManyTypes);

        toRandomize.SetType(random, sprite_text[random], random);
    }

    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();

        RandomizeType(randomMatchable);

        return randomMatchable;
    }

    public int NextType(Matchable matchable)
    {
        int nextType = (matchable.Type + 1) % howManyTypes;

        matchable.SetType(nextType, sprite_text[nextType], nextType);

        return nextType;
    }

    public Matchable UpgradeMatchable(Matchable toBeUpgraded, MatchType type)
    {
        if (type == MatchType.match4)
            return toBeUpgraded.Upgrade(MatchType.match4, Bomb);

        return toBeUpgraded;
    }

    public void ChangeType(Matchable toChange, int type)
    {
        toChange.SetType(type, sprite_text[type], type);
    }
}
