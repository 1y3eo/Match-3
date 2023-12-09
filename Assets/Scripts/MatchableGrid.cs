using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    private MatchablePool pool;
    private ScoreManager score;
    private AudioMixer audiomixer;


    [SerializeField] private Vector3 offscreenOffset;

    [SerializeField] private List<Matchable> possibleMoves;

    [SerializeField] public ParticleSystem boom;


    private void Start()
    {
        pool = (MatchablePool)MatchablePool.Instance;
        score = ScoreManager.Instance;
        audiomixer = AudioMixer.Instance;
    }

    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulaition = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();

        Matchable newMatchable;
        Vector3 onscreenPosition;

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
            {
                if (IsEmpty(x, y))
                {
                    newMatchable = pool.GetRandomMatchable();

                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offscreenOffset;

                    newMatchable.gameObject.SetActive(true);

                    newMatchable.position = new Vector2Int(x, y);

                    PutItemAt(newMatchable, x, y);

                    newMatchables.Add(newMatchable);

                    int initialType = newMatchable.Type;


                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {
                        if (pool.NextType(newMatchable) == initialType)
                        {
                            Debug.Break();
                            yield return null;
                            break;
                        }
                    }
                }
            }

        for (int i = 0; i != newMatchables.Count; ++i)
        {
            // calculate screen position
            onscreenPosition = transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y);

            audiomixer.PlayDelayedSound(SoundEffects.land, 1f / newMatchables[i].Speed);

            // move on screen position
            if (i == newMatchables.Count - 1)
                yield return StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));
            else
                StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            if (!initialPopulaition)
                yield return new WaitForSeconds(0.1f);
        }
    }

    private bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0,
             verticalMatches = 0;

        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
            return true;

        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
            return true;

        return false;
    }

    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;
        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position) && GetItemAt(position).Type == toMatch.Type)
        {
            ++matches;
            position += direction;
        }
        return matches;
    }

    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        yield return StartCoroutine(Swap(copies));

        // check bomb
        if (copies[0].IsBomb && copies[1].IsBomb)
        {
            MatchEverything();
            yield break;
        }
        if (copies[0].IsBomb)
        {
            MatchSquare(copies[0]);
            yield break;
        }
        else if (copies[1].IsBomb)
        {
            MatchSquare(copies[1]);
            yield break;
        }

        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);

        // check matches
        if (matches[0] != null)
        {
            StartCoroutine(score.ResolveMatch(matches[0]));
        }
        if (matches[1] != null)
        {
            StartCoroutine(score.ResolveMatch(matches[1]));
        }

        // back to the previous position
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            if (ScanForMatches())
                StartCoroutine(FillAndScanGrid());
        }
        else
            yield return StartCoroutine(FillAndScanGrid());
    }

    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        // swap in grid data -> each matchable -> world position

        SwapItemsAt(toBeSwapped[0].position, toBeSwapped[1].position);


        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;


        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;


        audiomixer.PlaySound(SoundEffects.swap);

        // move
        StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }

    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);

        Match horizontalMatch, verticalMatch;

        horizontalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            GetBranches(match, horizontalMatch, Orientation.vertical);
        }

        verticalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            GetBranches(match, verticalMatch, Orientation.horizontal);
        }

        if (match.Count == 1)
            return null;

        return match;
    }

    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();
        Matchable next;
        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);

            if (next.Type == toMatch.Type && next.Idle)
            {
                if (!tree.Contains(next))
                    match.AddMatchable(next);
                else
                    match.AddUnlisted();

                position += direction;
            }
            else
                break;
        }
        return match;
    }


    private void GetBranches(Match tree, Match branchToSearch, Orientation perpendicular)
    {
        Match branch;

        foreach (Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, perpendicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = perpendicular;

            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, perpendicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }
        }
    }


    private IEnumerator FillAndScanGrid()
    {
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));

        // scan grid for chain reactions
        if (ScanForMatches())
            StartCoroutine(FillAndScanGrid());
        else
            CheckPossibleMoves();
    }

    public void CheckPossibleMoves()
    {
        if (ScanForMoves() == 0)
        {
            GameManager.Instance.NoMoreMoves();
        }
    }


    private void CollapseGrid()
    {
        for (int x = 0; x != Dimensions.x; ++x)
            for (int yEmpty = 0; yEmpty != Dimensions.y - 1; ++yEmpty)
                if (IsEmpty(x, yEmpty))
                    for (int yNotEmpty = yEmpty + 1; yNotEmpty != Dimensions.y; ++yNotEmpty)
                        if (!IsEmpty(x, yNotEmpty) && GetItemAt(x, yNotEmpty).Idle)
                        {
                            MoveMatchableToPosition(GetItemAt(x, yNotEmpty), x, yEmpty);
                            break;
                        }
    }


    private void MoveMatchableToPosition(Matchable toMove, int x, int y)
    {
        // move to new position
        MoveItemTo(toMove.position, new Vector2Int(x, y));

        toMove.position = new Vector2Int(x, y);

        StartCoroutine(toMove.MoveToPosition(transform.position + new Vector3(x, y)));

        audiomixer.PlayDelayedSound(SoundEffects.land, 1f / toMove.Speed);
    }

    private bool ScanForMatches()
    {
        bool madeAMatch = false;
        Matchable toMatch;
        Match match;

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (!IsEmpty(x, y))
                {
                    toMatch = GetItemAt(x, y);

                    if (!toMatch.Idle)
                        continue;

                    match = GetMatch(toMatch);

                    if (match != null)
                    {
                        madeAMatch = true;
                        StartCoroutine(score.ResolveMatch(match));
                    }
                }
        return madeAMatch;
    }


    public void MatchSquare(Matchable powerup)
    {
        Match square3 = new Match();

        for (int y = powerup.position.y - 1; y < powerup.position.y + 2; ++y)
            for (int x = powerup.position.x - 1; x < powerup.position.x + 2; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    square3.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(square3, MatchType.match4));
        StartCoroutine(FillAndScanGrid());

        ParticleSystem.ShapeModule shape = boom.shape;
        shape.position = new Vector3(powerup.position.x, powerup.position.y, 0f);
        boom.Play();
        audiomixer.PlaySound(SoundEffects.powerup);
    }


    public void MatchEverything()
    {
        Match everything = new Match();

        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                    everything.AddMatchable(GetItemAt(x, y));

        StartCoroutine(score.ResolveMatch(everything, MatchType.match4));
        StartCoroutine(FillAndScanGrid());

        audiomixer.PlaySound(SoundEffects.powerup);
    }

    private int ScanForMoves()
    {
        possibleMoves = new List<Matchable>();

        // scan grid
        // if a matchable can move, add it to the list of possible moves
        for (int y = 0; y != Dimensions.y; ++y)
            for (int x = 0; x != Dimensions.x; ++x)
                if (CheckBounds(x, y) && !IsEmpty(x, y) && CanMove(GetItemAt(x, y)))
                    possibleMoves.Add(GetItemAt(x, y));

        return possibleMoves.Count;
    }


    private bool CanMove(Matchable toCheck)
    {
        if (CanMove(toCheck, Vector2Int.up)
            || CanMove(toCheck, Vector2Int.right)
            || CanMove(toCheck, Vector2Int.down)
            || CanMove(toCheck, Vector2Int.left))
            return true;

        if (toCheck.IsBomb)
            return true;

        return false;
    }

    // Can this matchable move in 1 direction?
    private bool CanMove(Matchable toCheck, Vector2Int direction)
    {
        // Look 2 and 3 positions away straight ahead
        Vector2Int position1 = toCheck.position + direction * 2,
                      position2 = toCheck.position + direction * 3;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        Vector2Int cw = new Vector2Int(direction.y, -direction.x),
                      ccw = new Vector2Int(-direction.y, direction.x);

        // Look diagonally clockwise
        position1 = toCheck.position + direction + cw;
        position2 = toCheck.position + direction + cw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        // Look at both diagnals
        position2 = toCheck.position + direction * ccw;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        // look diagnally counterclockwise
        position1 = toCheck.position + direction + ccw * 2;

        if (IsAPotentialMatch(toCheck, position1, position2))
            return true;

        return false;
    }

    // will these machable form a potential match?
    private bool IsAPotentialMatch(Matchable toCompare, Vector2Int position1, Vector2Int position2)
    {
        if
        (
            CheckBounds(position1) && CheckBounds(position2)
            && !IsEmpty(position1) && !IsEmpty(position2)
            && GetItemAt(position1).Idle && GetItemAt(position2).Idle
            && GetItemAt(position1).Type == toCompare.Type && GetItemAt(position2).Type == toCompare.Type
        )
            return true;
        return false;
    }

    // show a hint to the player
    // public void ShowHint()
    // {
    //     hint.IndicateHint(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);
    // }
}
