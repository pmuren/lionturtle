using System;
namespace lionturtle;

public class Slot
{
	public HashSet<Module> modules;
	public Dictionary<Module, List<Module>>[] supportedModules;

	public Slot(List<Module> initialModules)
	{
        modules = new HashSet<Module>();

        supportedModules = new Dictionary<Module, List<Module>>[6];
		for(int direction = 0; direction < 6; direction++)
		{
			supportedModules[direction] = new Dictionary<Module, List<Module>>();
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
					supportedModules[direction].Add(supportedModule, new List<Module>());
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

    public List<Module>[] RemoveModule(Module oldModule)
    {
		var newlyUnsupportedModules = new List<Module>[6];

        for (int direction = 0; direction < 6; direction++)
        {
			newlyUnsupportedModules[direction] = new List<Module>();
            foreach (Module supportedModule in DataGenerator.moduleSupportMap[oldModule][direction])
            {
				if (supportedModules[direction].ContainsKey(supportedModule))
				{
					var supportingModules = supportedModules[direction][supportedModule];
                    supportingModules.Remove(oldModule);

                    if (supportingModules.Count == 0)
                    {
                        newlyUnsupportedModules[direction].Add(supportedModule);
                        supportedModules[direction].Remove(supportedModule);
                    }
                }
            }
        }

        modules.Remove(oldModule);
		return newlyUnsupportedModules;
    }

}
