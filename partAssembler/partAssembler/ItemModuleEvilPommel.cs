using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;

namespace partAssembler
{
    class ItemModuleEvilPommel : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            Debug.Log("EVIL POMMEL LOAD");
            base.OnItemLoaded(item);

            EvilPommel evilPommel = item.gameObject.AddComponent<EvilPommel>();
            evilPommel.particleSystem = item.definition.GetCustomReference("Particles").transform.GetComponent<ParticleSystem>();

            evilPommel.Initialize(item);
        }
    }
}
