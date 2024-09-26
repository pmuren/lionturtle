using System;
using System.Reflection;

namespace lionturtle;

public static class DataGenerator
{
    public static List<Module> allModules;
    public static Dictionary<Module, HashSet<Module>[]> moduleSupportMap;

    static DataGenerator()
    {
        allModules = DataGenerator.GenerateAllModules();
        moduleSupportMap = DataGenerator.GenerateModuleSupportMap();
    }

    public static List<Module> GenerateAllModules()
    {
        Module[] archetypes = new Module[]{
        //    new Module( //000000
        //        new Connector[] {
        //            new Connector("white", 0),
        //            new Connector("white", 0),
        //            new Connector("white", 0),
        //            new Connector("white", 0),
        //            new Connector("white", 0),
        //            new Connector("white", 0)
        //        }
        //    ),
        //    new Module( //110000
        //        new Connector[] {
        //            new Connector("purple", 1),
        //            new Connector("black", 0),
        //            new Connector("green", -1),
        //            new Connector("white", -1),
        //            new Connector("blue", -1),
        //            new Connector("red", 0)
        //        }
        //    ),
        //    new Module( //111000
        //        new Connector[] {
        //            new Connector("green", 1),
        //            new Connector("blue", 1),
        //            new Connector("black", 0),
        //            new Connector("green", -1),
        //            new Connector("blue", -1),
        //            new Connector("red", 0)
        //        }
        //    ),
        //    new Module( //111100
        //        new Connector[] {
        //            new Connector("green", 1),
        //            new Connector("white", 1),
        //            new Connector("blue", 1),
        //            new Connector("black", 0),
        //            new Connector("purple", -1),
        //            new Connector("red", 0)
        //        }
        //    )
        //};

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
        var allModules = GenerateAllModules();

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
}