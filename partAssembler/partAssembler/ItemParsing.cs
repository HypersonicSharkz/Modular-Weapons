using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BS;

namespace partAssembler
{
    class ItemPartInfo : MonoBehaviour
    {
        public List<Renderer> bladeRenderes = new List<Renderer>();
        public List<ColliderGroup> bladeColliders = new List<ColliderGroup>();
        
    }
}
