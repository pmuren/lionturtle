using System;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;

namespace lionturtle;

public static class DataGenerator
{
    public static List<Module> allModules;
    public static Dictionary<Module, HashSet<Module>[]> moduleSupportMap;
    public static List<AxialPosition[]> triangleCoordinateGroups;

    static DataGenerator()
    {
        allModules = GenerateAllModules();
        moduleSupportMap = GenerateModuleSupportMap();
        triangleCoordinateGroups = GenerateTriangleCoordinateGroups();
    }

    public static List<Module> GenerateAllModules()
    {
        Module[] archetypes = new Module[]{
            new Module( //000000
                new Connector("white", 0),
                new Connector("white", 0),
                new Connector("white", 0),
                new Connector("white", 0),
                new Connector("white", 0),
                new Connector("white", 0)
            ),
            new Module( //110000
                new Connector("red", 0),
                new Connector("purple", 1),
                new Connector("black", 0),
                new Connector("green", -1),
                new Connector("white", -1),
                new Connector("blue", -1)
            ),
            new Module( //111000
                new Connector("red", 0),
                new Connector("green", 1),
                new Connector("blue", 1),
                new Connector("black", 0),
                new Connector("green", -1),
                new Connector("blue", -1)
            ),
            new Module( //111100
                new Connector("red", 0),
                new Connector("green", 1),
                new Connector("white", 1),
                new Connector("blue", 1),
                new Connector("black", 0),
                new Connector("purple", -1)
            )
        };

    List<Module> allModules = new List<Module>();
        foreach(Module archetype in archetypes)
        {
            for(int direction = 0; direction < 6; direction++)
            {
                allModules.Add(archetype.GetRotated(direction));
                if (archetype == archetypes[0]) break; //no need to repeat symmetric one
            }
        }

        return allModules;
    }

    public static Dictionary<Module, HashSet<Module>[]> GenerateModuleSupportMap()
    {
        var supportMap = new Dictionary<Module, HashSet<Module>[]>();
        var allModules = DataGenerator.allModules;

        foreach (Module homeModule in allModules)
        {
            for (int direction = 0; direction < 6; direction++)
            {
                foreach(Module remoteModule in allModules)
                {
                    if(homeModule.Supports(remoteModule, direction))
                    {
                        if (!supportMap.ContainsKey(homeModule))
                        {
                            supportMap.Add(homeModule, new HashSet<Module>[]
                            {
                                new HashSet<Module>(),
                                new HashSet<Module>(),
                                new HashSet<Module>(),
                                new HashSet<Module>(),
                                new HashSet<Module>(),
                                new HashSet<Module>(),
                            });
                        }
                        supportMap[homeModule][direction].Add(remoteModule);
                    }
                }
            }
        }

        return supportMap;
    }

    public static Dictionary<AxialPosition, Vertex> GenerateV24Vertices(int hexType, int hexRotation)
    {
        //Generate xy positions for corners
        Vector3[] cornerVertices = new Vector3[6];
        for (int direction = 0; direction < 6; direction++)
        {
            int degrees = direction * 60 + 30;
            double radians = Math.PI * degrees / 180.0f;
            cornerVertices[direction] = new Vector3(
                (float)Math.Sin(radians),
                (float)Math.Cos(radians),
                0
            );
        }

        AxialPosition[] dualDirections = new AxialPosition[]
        {
            new AxialPosition(+2, -1), new AxialPosition(+1, -2),
            new AxialPosition(-1, -1), new AxialPosition(-2, +1),
            new AxialPosition(-1, +2), new AxialPosition(+1, +1)
        };

        AxialPosition[] cornerAxials = dualDirections.Select((direction) => direction * 2).ToArray();

        //   Vector3[][] vertexRings = new Vector3[3][]{
        //       new Vector3[1], //rotate none
        // new Vector3[6], //rotate by 1
        // new Vector3[12]  //rotate by 2
        //};

        AxialPosition[][] vertexRings = new AxialPosition[3][]{
            new AxialPosition[1], //rotate none
		    new AxialPosition[6], //rotate by 1
		    new AxialPosition[12]  //rotate by 2
	    };

        vertexRings[0][0] = new AxialPosition(0, 0);
        for (int i = 0; i < vertexRings[1].Length; i++)
        {
            vertexRings[1][i] = cornerAxials[i] / 2;
        }
        for (int i = 0; i < vertexRings[2].Length; i++)
        {
            if (i % 2 == 0)
            {
                vertexRings[2][i] = cornerAxials[i / 2];
            }
            else
            {
                vertexRings[2][i] = (cornerAxials[i / 2] + cornerAxials[(i / 2 + 1) % 6]) / 2;
            }
        }

        Dictionary<AxialPosition, Vertex> vertices24 = new();

        //Vector3[] v24Positions = vertexRings.SelectMany(v => v).ToArray(); //flatten

        Dictionary<int, float[][]> heightsForTypes = new Dictionary<int, float[][]>{
            {0, new float[3][]{
                new float[]{0},
                new float[]{0, 0, 0, 0, 0, 0},
                new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            }},
            {2, new float[3][]{
                new float[]{0.5f},
                new float[]{0.75f, 0.75f, 0.375f, 0.25f, 0.25f, 0.375f},
                new float[]{1, 1, 1, 0.5f, 0, 0, 0, 0, 0, 0, 0, 0.5f}
            }},
            {3, new float[3][]{
                new float[]{0.5f},
                new float[]{0.6875f, 0.8125f, 0.6875f, 0.3125f, 0.1875f, 0.3125f},
                new float[]{1, 1, 1, 1, 1, 0.5f, 0, 0, 0, 0, 0, 0.5f}
            }},
            {4, new float[3][]{
                new float[]{0.5f},
                new float[]{0.625f, 0.75f, 0.75f, 0.625f, 0.25f, 0.25f},
                new float[]{1, 1, 1, 1, 1, 1, 1, 0.5f, 0, 0, 0, 0.5f}
            }}
        };

        AxialPosition position = new AxialPosition(0, 0);
        var heightRings = heightsForTypes[hexType];
        vertices24.Add(position, new Vertex(position, heightRings[0][0]));

        position = dualDirections[0];
        for (int i = 0; i < 6; i++)
        {
            int rotatedIndex = (i + hexRotation) % 6;
            vertices24.Add(position, new Vertex(position, heightRings[1][rotatedIndex]));

            position += dualDirections[(i + 2) % 6];
        }

        position = dualDirections[0] * 2;
        for (int i = 0; i < 12; i++)
        {
            int rotatedIndex = (i + hexRotation * 2) % 12;

            vertices24.Add(position, new Vertex(position, heightRings[2][rotatedIndex]));

            position += dualDirections[((i / 2) + 2) % 6];
        }

        return vertices24;

        //Inner triangles
        //a0, b0, b1
        //a0, b1, b2
        //a0, b2, b3
        //a0, b3, b4
        //a0, b4, b5
        //a0, b5, b0

        //List<Vector3[]> triangles = new List<Vector3[]>
        //{
        //    //inner triangles
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][0], vertexRings[1][1]
        //    },
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][1], vertexRings[1][2]
        //    },
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][2], vertexRings[1][3]
        //    },
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][3], vertexRings[1][4]
        //    },
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][4], vertexRings[1][5]
        //    },
        //    new Vector3[]{
        //        vertexRings[0][0], vertexRings[1][5], vertexRings[1][0]
        //    }
        //};

        //for (int i = 0; i < 6; i++)
        //{
        //    triangles.Add(new Vector3[]
        //    {
        //        vertexRings[1][i], vertexRings[2][i*2], vertexRings[2][i*2+1]
        //    });
        //    triangles.Add(new Vector3[]
        //    {
        //        vertexRings[1][i], vertexRings[2][i*2+1], vertexRings[1][(i+1)%6]
        //    });
        //    triangles.Add(new Vector3[]
        //    {
        //        vertexRings[1][(i+1)%6], vertexRings[2][i*2+1], vertexRings[2][(i*2+2)%12]
        //    });
        //}

        //Outer Triangles
        //b0, c0, c1
        //    b0, c1, b1
        //        b1, c1, c2

        //b1, c2, c3
        //    b1, c3, b2
        //        b2, c3, c4

        //b2, c4, c5
        //    b2, c5, b3
        //        b3, c5, c6

        //b3, c6, c7
        //    b3, c7, b4
        //        b4, c7, c8

        //b4, c8, c9
        //    b4, c9, b5
        //        b5, c9, c10

        //b5, c10, c11
        //    b5, c11, b0
        //        b0, c11, c0
    }

    public static Dictionary<AxialPosition, Vertex> PopulateVertexTypes(
        Dictionary<AxialPosition, Vertex> vertices24,
        int hexType,
        VertexType[] cornerTypes,
        VertexType[] edgeTypes
    )
    {
        VertexType defaultType = hexType == 0 ? VertexType.Flat : VertexType.Slope;

        vertices24[new AxialPosition(0, 0)].type = defaultType;
        for(int i = 0; i < 6; i++)
        {
            vertices24[Constants.dualDirections[i]].type = defaultType;
        }

        AxialPosition position = Constants.dualDirections[0] * 2; //top right corner
        for (int i = 0; i < 12; i++)
        {
            if(i % 2 == 0) //corner
            {
                vertices24[position].type = cornerTypes[i / 2];
            }
            else
            {
                vertices24[position].type = edgeTypes[((i+2) / 2)%6];
            }
            position += Constants.dualDirections[(i/2 + 2)%6];
        }

        return vertices24;
    }

    public static List<AxialPosition[]> GenerateTriangleCoordinateGroups()
    {
        List<AxialPosition[]> triangles = new();

        AxialPosition walkingPosition = Constants.dualDirections[0];
        for (int i = 0; i < 6; i++)
        {
            //add inward triangle with edge from v0 to v1
            AxialPosition[] inwardTriangle = new AxialPosition[]
            {
                walkingPosition,
                walkingPosition + Constants.dualDirections[(i + 2) % 6],
                walkingPosition + Constants.dualDirections[(i + 3) % 6],
            };
            triangles.Add(inwardTriangle);

            //add outward triangle with edge from v0 to v1
            AxialPosition[] outwardTriangle = new AxialPosition[]
            {
                walkingPosition,
                walkingPosition + Constants.dualDirections[(i + 1) % 6],
                walkingPosition + Constants.dualDirections[(i + 2) % 6],
            };
            triangles.Add(outwardTriangle);

            walkingPosition += Constants.dualDirections[(i + 2) % 6];
        }

        walkingPosition = Constants.dualDirections[0] * 2;
        for (int i = 0; i < 12; i++)
        {
            //add inward triangle with edge from v0 to v1
            AxialPosition[] inwardTriangle = new AxialPosition[]
            {
                walkingPosition,
                walkingPosition + Constants.dualDirections[((i/2) + 2) % 6],
                walkingPosition + Constants.dualDirections[((i/2) + 3) % 6],
            };
            triangles.Add(inwardTriangle);

            walkingPosition += Constants.dualDirections[((i / 2) + 2) % 6];
        }

        return triangles;
    }
}