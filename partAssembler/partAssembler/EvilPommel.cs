using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;

namespace partAssembler
{
    

    class EvilPommel : MonoBehaviour
    {
        public ParticleSystem particleSystem;

        public void Initialize(Item finalItem)
        {
            Debug.Log("INITIALIZE CALLED");
            List<Renderer> bladeRenderesTotal = new List<Renderer>();

            foreach (CustomReference customReference1 in finalItem.definition.customReferences.ToList())
            {
                if (customReference1.transform.GetComponent<AttachmentPointLogic>())
                {
                    if (customReference1.transform.GetComponent<AttachmentPointLogic>().bladeRenderes.Count > 0)
                    {
                        bladeRenderesTotal.AddRange(customReference1.transform.GetComponent<AttachmentPointLogic>().bladeRenderes);
                    }
                }
            }

            Debug.Log("DONE REF");

            if (bladeRenderesTotal.Count > 0)
            {
                Debug.Log(">0");
                foreach (Renderer renderer in bladeRenderesTotal)
                {

                    // Particle system
                    ParticleSystem ps = Instantiate(particleSystem, renderer.transform);

                    ps.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    ps.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                    ps.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    var shape = ps.shape;

                    shape.shapeType = ParticleSystemShapeType.Mesh;
                    shape.mesh = renderer.GetComponent<MeshFilter>().mesh;

                }
            }
            
        }
    }
}
