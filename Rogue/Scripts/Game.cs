using Godot;
using System;
using System.Collections.Generic;

public enum Tile { Period, Hash, At, A, B, C, D, E, F, G, H, I, J, K, L, LessThan, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, GreaterThan, Plus, Empty }

public class Game : Node2D
{
    const int MAX_SIZE = 80;
    const int MIN_SIZE = 60;

    double chanceWalkerDirection = 0.5;
    double chanceWalkerSpawn = 0.05;
    double chanceWalkerDestroy = 0.05;
    int maxWalkers = 10;

    double percentToFill = 0.05;

    struct Walker
    {
        public Vector2 direction;
        public Vector2 position;
    }

    List<Walker> walkers;


    TileMap tileMap;
    TileMap visibilityMap;
    Sprite player;
    Vector2 playerPosition;

    public Tile[,] map;

    Random random;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        tileMap = (TileMap)GetNode("TileMap");
        visibilityMap = (TileMap)GetNode("VisibilityMap");
        player = (Sprite)GetNode("/root/Game/Player");
        playerPosition = player.GetPosition();

        random = new Random();

        Setup();
        CreateFloors();
        CreateWalls();
        PlacePlayer();
        SpawnLevel();
    }

    private void Setup()
    {
        // Set map size.
        int mapHeight = random.Next(MIN_SIZE, MAX_SIZE);
        int mapWidth = random.Next(mapHeight, MAX_SIZE);

        map = new Tile[mapWidth, mapHeight];

        // Create empty map.
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                map[x, y] = Tile.Empty;
                visibilityMap.SetCell(x, y, 0);
            }
        }

        // Set first walker.
        // Initialize list.
        walkers = new List<Walker>();
        // Create new walker.
        Walker newWalker = new Walker();
        newWalker.direction = RandomDirection();

        // Find center of the map.
        Vector2 spawnPosition = new Vector2(Mathf.RoundToInt(map.GetLength(0) / 2.0f), Mathf.RoundToInt(map.GetLength(1) / 2.0f));

        newWalker.position = spawnPosition;
        // Add walker to list.
        walkers.Add(newWalker);
    }

    private void PlacePlayer()
    {
        Vector2 playerSpawn = new Vector2(Mathf.RoundToInt(map.GetLength(0) / 2.0f), Mathf.RoundToInt(map.GetLength(1) / 2.0f));

        map[(int)playerSpawn.x, (int)playerSpawn.y] = Tile.At;
        SetTile((int)playerSpawn.x, (int)playerSpawn.y, Tile.At);
        player.SetPosition(tileMap.MapToWorld(playerSpawn));
        
        GD.Print(tileMap.MapToWorld(playerSpawn).ToString());
    }

    private void CreateFloors()
    {
        // Loop won't run forever.
        int iterations = 0;

        do
        {
            if (walkers.Count > 0)
            {
                //GD.Print(RandomDirection().ToString());

                foreach (Walker walker in walkers)
                {
                    map[(int)walker.position.x, (int)walker.position.y] = Tile.Period;
                }

                // Chance walker gets destroyed
                int numberChecks = walkers.Count;
                for (int i = 0; i < numberChecks; i++)
                {
                    if (random.NextDouble() < chanceWalkerDestroy && walkers.Count > 1)
                    {
                        walkers.RemoveAt(i);
                        // Only destroy one per iteration.
                        break;
                    }
                }

                // Chance walker picks new direction
                for (int i = 0; i < walkers.Count; i++)
                {
                    if (random.NextDouble() < chanceWalkerDirection)
                    {
                        Walker walker = walkers[i];
                        walker.direction = RandomDirection();
                        walkers[i] = walker;
                    }
                }

                // Chance new walker spawns
                for (int i = 0; i < walkers.Count; i++)
                {
                    if (random.NextDouble() < chanceWalkerSpawn && walkers.Count < maxWalkers)
                    {
                        Walker walker = new Walker();
                        walker.direction = RandomDirection();
                        walker.position = walkers[i].position;
                        walkers.Add(walker);
                    }
                }

                // Move walkers.
                for (int i = 0; i < walkers.Count; i++)
                {
                    Walker walker = walkers[i];
                    //GD.Print("Previous position: " + walker.position.ToString());
                    walker.position += walker.direction;
                    //GD.Print("New position: " + walker.position.ToString());
                    walkers[i] = walker;
                }

                // Avoid border.
                for (int i = 0; i < walkers.Count; i++)
                {
                    Walker walker = walkers[i];
                    // Clamp x,y to leave a 1 space border -- leave room for walls.
                    walker.position.x = Mathf.Clamp(walker.position.x, 1, map.GetLength(0) - 2);
                    walker.position.y = Mathf.Clamp(walker.position.y, 1, map.GetLength(1) - 2);
                    walkers[i] = walker;
                }

                // Check to exit loop.
                if ((float)NumberOfFloors() / (float)map.Length > percentToFill)
                {
                    break;
                }

                iterations++;
            }
        }
        while (iterations < 100000);
    }

    private void SpawnLevel()
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                switch (map[x, y])
                {
                    case Tile.Empty:
                        break;
                    case Tile.Period:
                        SetTile(x, y, Tile.Period);
                        break;
                    case Tile.Hash:
                        SetTile(x, y, Tile.Hash);
                        break;
                }
            }
        }
    }

    private void CreateWalls()
    {
        // Loop through every map tile.
        for (int x = 0; x < map.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < map.GetLength(1) - 1; y++)
            {
                if (map[x, y] == Tile.Period)
                {
                    if (map[x, y + 1] == Tile.Empty)
                        map[x, y + 1] = Tile.Hash;
                    if (map[x, y - 1] == Tile.Empty)
                        map[x, y - 1] = Tile.Hash;
                    if (map[x + 1, y] == Tile.Empty)
                        map[x + 1, y] = Tile.Hash;
                    if (map[x - 1, y] == Tile.Empty)
                        map[x - 1, y] = Tile.Hash;
                }
            }
        }
    }

    private Vector2 RandomDirection()
    {
        // Pick random int from 0-3.
        int choice = (int)random.Next(4);
        //GD.Print(choice);
        // Choose direction.
        switch (choice)
        {
            case 0:
                return Vector2.Down;
            case 1:
                return Vector2.Left;
            case 3:
                return Vector2.Up;
            default:
                return Vector2.Right;
        }
    }

    private int NumberOfFloors()
    {
        int count = 0;

        foreach (Tile space in map)
        {
            if (space == Tile.Period)
                count++;
        }

        return count;
    }

    private void SetTile(int x, int y, Tile type)
    {
        map[x, y] = type;
        tileMap.SetCell(x, y, (int)type);
    }

    public override void _Process(float delta)
    {
        CallDeferred("UpdateVisibility");
    }

    public override void _Input(InputEvent ev)
    {
        if (!ev.IsPressed())
            return;


        if (ev.IsAction("up"))
            TryMove(0, -1);
        else if (ev.IsAction("up_right"))
            TryMove(1, -1);
        else if (ev.IsAction("right"))
            TryMove(1, 0);
        else if (ev.IsAction("down_right"))
            TryMove(1, 1);
        else if (ev.IsAction("down"))
            TryMove(0, 1);
        else if (ev.IsAction("down_left"))
            TryMove(-1, 1);
        else if (ev.IsAction("left"))
            TryMove(-1, 0);
        else if (ev.IsAction("up_left"))
            TryMove(-1, -1);
        else if (ev.IsAction("reveal_map"))
            RevealMap();
    }

    private void RevealMap()
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                visibilityMap.SetCell(x, y, -1);
            }
        }
    }

    private void TryMove(int directionX, int directionY)
    {
        // NPCS SHOULD TAKE TURNS HERE!

        // Find and move the player.
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == Tile.At)
                {
                    if (map[x + directionX, y + directionY] == Tile.Period)
                    {
                        SetTile(x + directionX, y + directionY, Tile.At);
                        SetTile(x, y, Tile.Period);
                        player.SetPosition(tileMap.MapToWorld(new Vector2(x + directionX, y + directionY)));
                        return;
                    }
                }
            }
        }
    }

    private void UpdateVisibility()
    {
        Vector2 playerCenter = new Vector2();
        var spaceState = GetWorld2d().DirectSpaceState;
        Vector2 playerPosition = new Vector2();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == Tile.At)
                {
                    playerPosition = new Vector2(x, y);
                    playerCenter = TileToPixelCenter(x,y);
                }
            }
        }

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (visibilityMap.GetCell(x, y) == 0)
                {
                    int xDirection = (x < playerPosition.x) ? 1 : -1;
                    int yDirection = (y < playerPosition.y) ? 1 : -1;

                    Vector2 testPoint = TileToPixelCenter(x, y) + new Vector2(xDirection, yDirection) * (tileMap.CellSize * 0.5f);

                    var occlusion = spaceState.IntersectRay(playerCenter, testPoint);

                    if (occlusion.Count > 0)
                    {
                        Vector2 position = (Vector2)occlusion["position"];
                        if ((position - testPoint).Length() < 1f)
                            visibilityMap.SetCell(x, y, -1);
                        continue;
                    }
                    else
                        visibilityMap.SetCell(x, y, -1);
                }
            }
        }
    }

    private Vector2 TileToPixelCenter(int x, int y)
    {
        return new Vector2((float)(x + 0.5) * tileMap.CellSize.x, (float)((y + 0.5) * tileMap.CellSize.y));
    }
}
