using System;
using System.Collections.Generic;
using System.Numerics;

using MinecraftSharp.Engine.Graphics;

using Silk.NET.OpenGL;

namespace MinecraftSharp.Client;

internal class Chunk
{
    private static readonly (Vector3 normal, Vector3[] offsets, Vector2[] uvs)[] FaceDefinitions =
    [
        // Left (-X)
        (new Vector3(-1, 0, 0),
            new Vector3[] { new(0, 0, 0), new(0, 1, 0), new(0, 1, 1), new(0, 0, 1) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
        // Right (+X)
        (new Vector3(1, 0, 0),
            new Vector3[] { new(1, 0, 1), new(1, 1, 1), new(1, 1, 0), new(1, 0, 0) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
        // Bottom (-Y)
        (new Vector3(0, -1, 0),
            new Vector3[] { new(0, 0, 1), new(1, 0, 1), new(1, 0, 0), new(0, 0, 0) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
        // Top (+Y)
        (new Vector3(0, 1, 0),
            new Vector3[] { new(0, 1, 0), new(1, 1, 0), new(1, 1, 1), new(0, 1, 1) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
        // Front (-Z)
        (new Vector3(0, 0, -1),
            new Vector3[] { new(1, 0, 0), new(1, 1, 0), new(0, 1, 0), new(0, 0, 0) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
        // Back (+Z)
        (new Vector3(0, 0, 1),
            new Vector3[] { new(0, 0, 1), new(0, 1, 1), new(1, 1, 1), new(1, 0, 1) },
            new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) }
        ),
    ];

    private static readonly (int dx, int dy, int dz)[] NeighborOffsets =
    [
        (-1, 0, 0), // Left
        (1, 0, 0),  // Right
        (0, -1, 0), // Bottom
        (0, 1, 0),  // Top
        (0, 0, -1), // Front
        (0, 0, 1),  // Back
    ];

    private readonly Block[,,] _blocks;

    private Mesh<Vertex, uint>? _mesh;

    public Chunk(GL gl, int x, int y, int z, byte size)
    {
        X = x;
        Y = y;
        Z = z;
        _blocks = new Block[size, size, size];
        CreateMesh(gl);
    }

    public int X { get; }

    public int Y { get; }

    public int Z { get; }

    public Block GetBlock(int x, int y, int z)
        => _blocks[x, y, z];

    public void SetBlock(int x, int y, int z, Block block)
        => _blocks[x, y, z] = block;

    public void Update(double deltaTime)
    {

    }

    public void Draw()
    {
        _mesh?.Draw();
    }

    private void CreateMesh(GL gl)
    {
        Dictionary<Vertex, uint> vertexCache = [];
        List<Vertex> vertices = [];
        List<uint> indices = [];
        uint nextIndex = 0;

        int xLength = _blocks.GetLength(0);
        int yLength = _blocks.GetLength(1);
        int zLength = _blocks.GetLength(2);

        Span<uint> faceIndices = stackalloc uint[4];
        for (int x = 0; x < xLength; x++)
        {
            for (int y = 0; y < yLength; y++)
            {
                for (int z = 0; z < zLength; z++)
                {
                    Block block = _blocks[x, y, z];
                    if (block.Type == BlockType.Air)
                        continue;

                    for (int face = 0; face < 6; face++)
                    {
                        int nx = x + NeighborOffsets[face].dx;
                        int ny = y + NeighborOffsets[face].dy;
                        int nz = z + NeighborOffsets[face].dz;

                        bool isFaceVisible =
                            nx < 0 || nx >= xLength ||
                            ny < 0 || ny >= yLength ||
                            nz < 0 || nz >= zLength ||
                            _blocks[nx, ny, nz].Type == BlockType.Air;

                        if (!isFaceVisible)
                            continue;

                        var (normal, offsets, uvs) = FaceDefinitions[face];

                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 pos = new(x + offsets[i].X, y + offsets[i].Y, z + offsets[i].Z);
                            Vertex vertex = new(pos, normal, uvs[i]);
                            if (!vertexCache.TryGetValue(vertex, out uint idx))
                            {
                                vertexCache[vertex] = idx = nextIndex++;
                                vertices.Add(vertex);
                            }
                            faceIndices[i] = idx;
                        }
                        // Two triangles per face: 0-1-2, 0-2-3
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[1]);
                        indices.Add(faceIndices[2]);
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[2]);
                        indices.Add(faceIndices[3]);
                    }
                }
            }
        }

        if (vertices.Count == 0)
            return;

        _mesh = new Mesh<Vertex, uint>(gl, vertices, indices, []);
    }
}