using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;

namespace partAssembler
{
    class ItemModuleFirePommel : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            //Debug.Log("FIRE POMMEL LOAD");
            base.OnItemLoaded(item);


            FirePommel firePommel = item.gameObject.AddComponent<FirePommel>();
            firePommel.particleSystem = item.definition.GetCustomReference("Particles").transform.GetComponent<ParticleSystem>();

            firePommel.Initialize(item);


        }
    }
}
