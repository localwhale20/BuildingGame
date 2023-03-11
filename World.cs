using System.Diagnostics;
using System.IO.Compression;

namespace BuildingGame;

public class World
{
    public const string WORLD_SAVE_HEADER = "BGWORLD2";
    public string WorldFile { get; set; } = "level.dat";
    public const int CHUNK_AREA = 16;
    public Chunk[,] Chunks { get; private set; } = null!;
    public Vector2 SpawnPos { get; set; }

    public World()
    {
        FlushChunks();
    }

    public void Draw()
    {
        for (int cx = 0; cx < CHUNK_AREA; cx++)
        {
            for (int cy = 0; cy < CHUNK_AREA; cy++)
            {
                var chunk = Chunks[cx, cy];

                if (chunk != null)
                    chunk.Draw(cx * 16, cy * 16);
            }
        }
    }

    int tick = 0;
    private void UpdateLiqiud(string id)
    {
        var waterBlocks = new List<(int x, int y)>();
        for (int x = 0; x < CHUNK_AREA * Chunk.SIZE; x++)
        {
            for (int y = 0; y < CHUNK_AREA * Chunk.SIZE; y++)
            {
                if (GetTile(x, y) == 0) continue;

                if (IsTileA(x, y, id))
                {
                    waterBlocks.Add((x, y));
                }
            }
        }

        foreach (var waterBlock in waterBlocks)
        {
            int x = waterBlock.x;
            int y = waterBlock.y;

            if (IsTileA(x - 1, y, 0) && !IsTileA(x, y + 1, 0) && !IsTileA(x, y + 1, id))
                PlaceTile(x - 1, y, Tile.GetNId(id));
            if (IsTileA(x + 1, y, 0) && !IsTileA(x, y + 1, 0) && !IsTileA(x, y + 1, id))
                PlaceTile(x + 1, y, Tile.GetNId(id));

            if (IsTileA(x, y + 1, 0))
                PlaceTile(x, y + 1, Tile.GetNId(id));
        }
    }

    public void UpdateObsidian()
    {
        string id = "lava";
        var waterBlocks = new List<(int x, int y)>();
        for (int x = 0; x < CHUNK_AREA * Chunk.SIZE; x++)
        {
            for (int y = 0; y < CHUNK_AREA * Chunk.SIZE; y++)
            {
                if (GetTile(x, y) == 0) continue;

                if (IsTileA(x, y, id))
                {
                    waterBlocks.Add((x, y));
                }
            }
        }

        foreach (var waterBlock in waterBlocks)
        {
            int x = waterBlock.x;
            int y = waterBlock.y;

            if (!IsTileA(x, y - 1, 0) && IsTileA(x, y - 1, "water"))
                PlaceTile(x, y, Tile.GetNId("obsidian"));
            if (!IsTileA(x, y + 1, 0) && IsTileA(x, y + 1, "water"))
                PlaceTile(x, y, Tile.GetNId("obsidian"));
            if (!IsTileA(x - 1, y, 0) && IsTileA(x - 1, y, "water"))
                PlaceTile(x, y, Tile.GetNId("obsidian"));
            if (!IsTileA(x + 1, y, 0) && IsTileA(x + 1, y, "water"))
                PlaceTile(x, y, Tile.GetNId("obsidian"));
        }
    }

    public void UpdateSand()
    {
        string id = "sand";
        var waterBlocks = new List<(int x, int y)>();
        for (int x = 0; x < CHUNK_AREA * Chunk.SIZE; x++)
        {
            for (int y = 0; y < CHUNK_AREA * Chunk.SIZE; y++)
            {
                if (GetTile(x, y) == 0) continue;

                if (IsTileA(x, y, id))
                {
                    waterBlocks.Add((x, y));
                }
            }
        }

        foreach (var waterBlock in waterBlocks)
        {
            int x = waterBlock.x;
            int y = waterBlock.y;

            if (IsTileA(x, y + 1, 0) || IsTileA(x, y + 1, "water"))
            {
                PlaceTile(x, y + 1, Tile.GetNId("sand"));
                PlaceTile(x, y, 0);
            }
            if (IsTileA(x, y + 1, "lava"))
            {
                PlaceTile(x, y, Tile.GetNId("glass"));
            }
        }
    }

    // public void UpdatePhysics()
    // {
    //     string sandId = "sand";
    //     string waterId = "water";
    //     string lavaId = "lava";
    //     string obisdianId = "obsidian";
    // }

    public void Update()
    {
        if (tick >= 20) tick = 0;
        tick++;

        if (!Settings.EnablePhysics) return;

        if (tick == 7) UpdateObsidian();
        if (tick == 15) UpdateSand();
        if (tick == 10) UpdateLiqiud("water");
        if (tick == 20) UpdateLiqiud("lava");
    }

    public bool IsTileA(int x, int y, string id)
    {
        return IsTileA(x, y, Tile.GetNId(id));
    }

    public bool IsTileA(int x, int y, byte id)
    {
        return IsValidTile(x, y) && GetTile(x, y) == id;
    }

    public void PlaceTile(int x, int y, TileInfo tile)
    {
        var area = GetChunkArea(x, y);
        var chunk = Chunks[area.cx, area.cy];

        if (chunk != null)
        {
            chunk.Tiles[area.lcx, area.lcy] = tile;
        }

    }

    public void PlaceTile(int cx, int cy, int x, int y, TileInfo tile)
    {
        var chunk = Chunks[cx, cy];

        if (chunk != null)
        {
            chunk.Tiles[x, y] = tile;
        }

    }

    public TileInfo GetTile(int x, int y)
    {
        var area = GetChunkArea(x, y);
        var chunk = Chunks[area.cx, area.cy];

        if (chunk != null)
            return chunk.Tiles[area.lcx, area.lcy];
        return 0;
    }

    public bool IsValidTile(int x, int y)
    {
        if (x < 0 || x >= CHUNK_AREA * Chunk.SIZE || y < 0 || y >= CHUNK_AREA * Chunk.SIZE)
            return false;
        return true;
    }

    public TileInfo GetTile(int cx, int cy, int x, int y)
    {
        var chunk = Chunks[cx, cy];
        if (chunk != null)
            return chunk.Tiles[x, y];
        return 0;
    }

    public (int cx, int cy, int lcx, int lcy) GetChunkArea(int wx, int wy)
    {

        int cx = Math.Clamp((int)(wx / Chunk.SIZE), 0, CHUNK_AREA - 1);
        int cy = Math.Clamp((int)(wy / Chunk.SIZE), 0, CHUNK_AREA - 1);

        int lcx = Math.Clamp((wx - Chunk.SIZE * cx), 0, Chunk.SIZE - 1);
        int lcy = Math.Clamp((wy - Chunk.SIZE * cy), 0, Chunk.SIZE - 1);

        return (cx, cy, lcx, lcy);
    }

    public void Save()
    {
        try
        {
            using FileStream fs = File.OpenWrite(WorldFile);
            using GZipStream gs = new GZipStream(fs, CompressionMode.Compress);
            using BinaryWriter bw = new BinaryWriter(gs);

            bw.Write(WORLD_SAVE_HEADER);

            bw.Write(SpawnPos.X);
            bw.Write(SpawnPos.Y);

            for (int x = 0; x < World.CHUNK_AREA * Chunk.SIZE; x++)
            {
                for (int y = 0; y < World.CHUNK_AREA * Chunk.SIZE; y++)
                {
                    WriteTile(bw, GetTile(x, y));
                }
            }
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private void FlushChunks()
    {
        Chunks = new Chunk[CHUNK_AREA, CHUNK_AREA];

        for (int x = 0; x < CHUNK_AREA; x++)
        {
            for (int y = 0; y < CHUNK_AREA; y++)
            {
                Chunks[x, y] = new Chunk();
            }
        }
    }

    public void Load(string path = "level.dat")
    {
        try
        {
            WorldFile = path;
            if (!File.Exists(path)) return;

            FlushChunks();

            using FileStream fs = File.OpenRead(path);
            using GZipStream gs = new GZipStream(fs, CompressionMode.Decompress);
            using BinaryReader br = new BinaryReader(gs);
            string header = br.ReadString();
            // throw new Exception("test");

            if (header == "LVL")
            {
                Console.WriteLine("world using old format, reading it using it");
                Console.WriteLine("creating backup");

                using FileStream backupFs = File.OpenWrite(path + ".old");
                using BinaryWriter backupBw = new BinaryWriter(backupFs);
                backupBw.Write("LVL");

                Console.WriteLine("reading world");

                for (int x = 0; x < World.CHUNK_AREA * Chunk.SIZE; x++)
                {
                    for (int y = 0; y < World.CHUNK_AREA * Chunk.SIZE; y++)
                    {
                        byte tile = br.ReadByte();
                        PlaceTile(x, y, tile);
                        backupBw.Write(tile);
                    }
                }

                Console.WriteLine("success");
            }
            else if (header != WORLD_SAVE_HEADER) throw new IOException("world header is invalid");
            else
            {
                SpawnPos = new Vector2(br.ReadSingle(), br.ReadSingle());
                for (int x = 0; x < World.CHUNK_AREA * Chunk.SIZE; x++)
                {
                    for (int y = 0; y < World.CHUNK_AREA * Chunk.SIZE; y++)
                    {
                        PlaceTile(x, y, ReadTile(br));
                    }
                }
            }

        }
        catch (IOException ex) { Console.WriteLine("bad world file\n" + ex); }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private void WriteTile(BinaryWriter bw, TileInfo tile)
    {
        bw.Write(tile.Type);
        bw.Write(tile.Flags.Rotation);
        bw.Write(tile.Flags.Flip);
    }

    private TileInfo ReadTile(BinaryReader br)
    {
        return new TileInfo(br.ReadByte(), new TileFlags(br.ReadSingle(), br.ReadBoolean()));
    }
}