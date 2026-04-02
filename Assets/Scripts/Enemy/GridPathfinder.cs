using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    [SerializeField] private Vector2Int minCell = new Vector2Int(-18, -10);
    [SerializeField] private Vector2Int maxCell = new Vector2Int(18, 10);
    [SerializeField] private Vector2Int[] blockedCells;

    private readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
    private readonly List<Vector2Int> open = new List<Vector2Int>();
    private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    private readonly Dictionary<Vector2Int, int> costSoFar = new Dictionary<Vector2Int, int>();

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Awake()
    {
        RebuildBlockedSet();
    }

    public bool TryGetDirection(Vector2 fromWorld, Vector2 toWorld, out Vector2 direction)
    {
        Vector2Int start = FindNearestWalkable(WorldToCell(fromWorld));
        Vector2Int goal = FindNearestWalkable(WorldToCell(toWorld));
        direction = Vector2.zero;

        if (!IsWalkable(start) || !IsWalkable(goal))
            return false;

        if (start == goal)
        {
            direction = (toWorld - fromWorld).normalized;
            return direction.sqrMagnitude > 0.001f;
        }

        if (!TryFindPath(start, goal, out Vector2Int nextCell))
            return false;

        direction = (CellToWorld(nextCell) - fromWorld).normalized;
        return direction.sqrMagnitude > 0.001f;
    }

    private void RebuildBlockedSet()
    {
        blocked.Clear();
        if (blockedCells == null)
            return;

        foreach (Vector2Int cell in blockedCells)
            blocked.Add(cell);
    }

    private bool TryFindPath(Vector2Int start, Vector2Int goal, out Vector2Int nextCell)
    {
        open.Clear();
        cameFrom.Clear();
        costSoFar.Clear();

        open.Add(start);
        cameFrom[start] = start;
        costSoFar[start] = 0;

        int guard = 0;
        while (open.Count > 0 && guard++ < 1600)
        {
            Vector2Int current = PopBestOpen(goal);
            if (current == goal)
                break;

            foreach (Vector2Int offset in Directions)
            {
                Vector2Int next = current + offset;
                if (!IsWalkable(next))
                    continue;

                int newCost = costSoFar[current] + 1;
                if (costSoFar.TryGetValue(next, out int oldCost) && newCost >= oldCost)
                    continue;

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                if (!open.Contains(next))
                    open.Add(next);
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            nextCell = start;
            return false;
        }

        Vector2Int step = goal;
        while (cameFrom[step] != start)
            step = cameFrom[step];

        nextCell = step;
        return true;
    }

    private Vector2Int PopBestOpen(Vector2Int goal)
    {
        int bestIndex = 0;
        int bestScore = int.MaxValue;
        for (int i = 0; i < open.Count; i++)
        {
            Vector2Int cell = open[i];
            int score = costSoFar[cell] + Heuristic(cell, goal);
            if (score >= bestScore)
                continue;

            bestScore = score;
            bestIndex = i;
        }

        Vector2Int best = open[bestIndex];
        open.RemoveAt(bestIndex);
        return best;
    }

    private Vector2Int FindNearestWalkable(Vector2Int cell)
    {
        if (IsWalkable(cell))
            return cell;

        for (int radius = 1; radius <= 4; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    Vector2Int candidate = cell + new Vector2Int(x, y);
                    if (IsWalkable(candidate))
                        return candidate;
                }
            }
        }

        return cell;
    }

    private bool IsWalkable(Vector2Int cell)
    {
        return cell.x >= minCell.x
            && cell.x <= maxCell.x
            && cell.y >= minCell.y
            && cell.y <= maxCell.y
            && !blocked.Contains(cell);
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static Vector2Int WorldToCell(Vector2 world)
    {
        return new Vector2Int(Mathf.RoundToInt(world.x), Mathf.RoundToInt(world.y));
    }

    private static Vector2 CellToWorld(Vector2Int cell)
    {
        return new Vector2(cell.x, cell.y);
    }
}
