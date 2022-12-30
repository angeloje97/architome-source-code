using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Serialization
{
    [Serializable]
    public class SaveUIModule
    {
        [Serializable]
        public class ModuleData
        {
            public string name;
            public Vector2 positionSaved;

            public ModuleData(ModuleInfo module)
            {
                name = module.ToString();
                positionSaved = module.transform.position;
            }

            public void Update(ModuleInfo module)
            {
                positionSaved = module.transform.position;
            }
        }

        public List<ModuleData> moduleDatas;

        public SaveUIModule()
        {
            moduleDatas = new();
            
        }

        public ModuleData DataModule(ModuleInfo module)
        {
            foreach (var moduleData in moduleDatas)
            {
                if (module.ToString().Equals(moduleData.name))
                {
                    return moduleData;
                }
            }

            return null;

        }


        public void SaveModule(ModuleInfo module)
        {
            var data = DataModule(module);

            if (data == null)
            {
                data = new(module);
                moduleDatas.Add(data);
                return;
            }

            data.Update(module);
        }

        public void LoadModule(ModuleInfo module)
        {
            var data = DataModule(module);

            if (data == null) return;

            module.transform.position = data.positionSaved;
        }

        
    }
}
