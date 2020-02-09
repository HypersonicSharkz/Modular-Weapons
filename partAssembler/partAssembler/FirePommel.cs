using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BS;

namespace partAssembler
{
    public class FirePommel : MonoBehaviour
    {

        public ParticleSystem particleSystem;

        private Shader shader;

        public void Initialize(Item finalItem)
        {



            //Debug.Log("INITIALIZE CALLED");
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

                //Debug.Log("custom reference");
                if (customReference1.name == "Effect")
                {
                    //Debug.Log("IS EFFECT");
                    shader = customReference1.transform.GetComponent<Renderer>().sharedMaterial.shader;
                    
                }
            }

            //Debug.Log("DONE REF");

            if (shader != null)
            {
                //Debug.Log("SHADE != NULL");
                if (bladeRenderesTotal.Count > 0)
                {
                    //Debug.Log(">0");
                    foreach (Renderer renderer in bladeRenderesTotal)
                    {
                        //Debug.Log("RENDERER");
                        renderer.sharedMaterial.shader = shader;

                        // Particle system
                        ParticleSystem ps = Instantiate(particleSystem, renderer.transform);

                        ps.gameObject.transform.localScale = new Vector3(1, 1, 1);
                        ps.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                        ps.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        var shape = ps.shape;

                        shape.shapeType = ParticleSystemShapeType.Mesh;
                        shape.mesh = renderer.GetComponent<MeshFilter>().mesh;

                    }

                    finalItem.OnCollisionEvent += FinalItem_OnCollisionEvent;
                }
            }
        }

        private void FinalItem_OnCollisionEvent(ref CollisionStruct collisionInstance)
        {
            //Debug.Log("oncol");
            if (collisionInstance.damageStruct.damage > 0)
            {
                //Debug.Log("dmg >0");
                if (collisionInstance.damageStruct.hitRagdollPart)
                {
                    //Debug.Log("ragdoll");
                    Creature creature = collisionInstance.targetCollider.GetComponentInParent<Creature>();
                    //Debug.Log("creat");

                    if (!collisionInstance.damageStruct.hitRagdollPart.GetComponentInChildren<MeshRenderer>())
                    {
                        return;
                    }

                    Renderer renderer = collisionInstance.damageStruct.hitRagdollPart.GetComponentInChildren<MeshRenderer>();

                    if (renderer.GetComponentInChildren<ParticleSystem>())
                    {
                        return;
                    }

                    //Debug.Log("no paticles");

                    ParticleSystem ps = Instantiate(particleSystem, renderer.transform);

                    

                    ps.gameObject.transform.localScale = new Vector3(1, 1, 1);
                    ps.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                    ps.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

                    var main = ps.main;

                    main.loop = false;
                    main.duration = 5;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
                    main.gravityModifier = -0.2f;

                    var shape = ps.shape;
                    shape.shapeType = ParticleSystemShapeType.Mesh;
                    shape.mesh = renderer.GetComponent<MeshFilter>().mesh;
                    

                    var em = ps.emission;

                    em.rateOverTime = 10;




                    StartCoroutine(Burn(creature, ps, renderer.material));


                }
            }
        }


        IEnumerator Burn(Creature creature, ParticleSystem particleSystem, Material material)
        {
            //Debug.Log("BURN");


           

            while (particleSystem.isPlaying)
            {
                //Debug.Log("alive?");
                CollisionStruct collisionStruct = new CollisionStruct(new DamageStruct(Damager.DamageType.Poison, 5), null, null, null, null, null, null, null);
                creature.health.Damage(ref collisionStruct);

                yield return new WaitForSeconds(1);

            }

            //Debug.Log("done");

            Destroy(particleSystem.gameObject);

        }
    }
}
