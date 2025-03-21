using System;
namespace lionturtle;

public class Slot
{
	public HashSet<Module> modules;
	public Dictionary<Module, HashSet<Module>>[] supportedModules;

	public Slot(HashSet<Module> initialModules)
	{
        modules = new HashSet<Module>();

        supportedModules = new Dictionary<Module, HashSet<Module>>[6];
		for(int direction = 0; direction < 6; direction++)
		{
			supportedModules[direction] = new Dictionary<Module, HashSet<Module>>();
		}

        foreach (Module module in initialModules)
        {
            AddModule(module);
        }
    }

	public void AddModule(Module newModule)
	{
		modules.Add(newModule);
		for(int direction = 0; direction < 6; direction++)
		{
            foreach (Module supportedModule in DataGenerator.moduleSupportMap[newModule][direction])
			{
                if (!supportedModules[direction].ContainsKey(supportedModule))
				{
					supportedModules[direction].Add(supportedModule, new HashSet<Module>());
				}
				supportedModules[direction][supportedModule].Add(newModule);
			}
		}

		//newly supported modules???????????????????????
		//	if the slot already existed but did not support something in a neighbor
		//	that neighbor should learn about it
		//	but don't we only add modules right at creation time?

		//testing plan
		//	build up
		//	break down
		//	dendrites
	}

	///this method is a little hard to read and therefore sus
    public HashSet<Module>[] RemoveModule(Module oldModule)
    {
		var newlyUnsupportedModules = new HashSet<Module>[6];

        modules.Remove(oldModule);

        for (int direction = 0; direction < 6; direction++)
        {
			newlyUnsupportedModules[direction] = new HashSet<Module>();
            foreach (Module supportedModule in DataGenerator.moduleSupportMap[oldModule][direction])
            {
				if (supportedModules[direction].ContainsKey(supportedModule))
				{
                    supportedModules[direction][supportedModule].Remove(oldModule);

                    if (supportedModules[direction][supportedModule].Count == 0)
                    {
                        newlyUnsupportedModules[direction].Add(supportedModule);
                        supportedModules[direction].Remove(supportedModule);
                    }
				}
			}
        }

		return newlyUnsupportedModules;
    }

}
