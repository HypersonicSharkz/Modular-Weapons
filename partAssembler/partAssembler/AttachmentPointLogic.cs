using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;

namespace partAssembler
{

    class AttachmentPointLogic : MonoBehaviour
    {
        public Item item;
        public String partType;
        public string[] legalAttachments;

        public bool isAttached;

        public float attachRange = 0.1f;

        // -------------------------------- //



        private float itemMass;
        private Vector3 itemCoM;
        private float itemDrag;
        private float itemAngularDrag;


        public List<Renderer> bladeRenderes = new List<Renderer>();


        public List<ItemModule> addedModules = new List<ItemModule>();


        public void Initialize()
        {
            isAttached = false;
            item.OnUngrabEvent += logic_OnUngrabEvent;
            Debug.Log("Initialized");


            Debug.Log("Damager count" + item.data.damagers.Count);

        }


        public bool isHandle(Item itemRef)
        {
            bool isHandle = false;
            foreach (CustomReference customReference in itemRef.definition.customReferences)
            {
                if (customReference.transform.GetComponent<AttachmentPointLogic>())
                {
                    if (customReference.transform.GetComponent<AttachmentPointLogic>().partType == "handle")
                    {
                        isHandle = true;
                    }
                }
            }
            
            
            return isHandle;
        }

        private void logic_OnUngrabEvent(Handle handle, Interactor interactor, bool throwing)
        {

            List<AttachmentPointLogic> possibleAttachments = new List<AttachmentPointLogic>();
            

            

            foreach (Item item1 in Item.list)
            {
                if (item1 != this.item)
                {
                    foreach (CustomReference customReference in item1.definition.customReferences)
                    {
                        if (customReference.transform.GetComponent<AttachmentPointLogic>())
                        {
                            AttachmentPointLogic apl = customReference.transform.GetComponent<AttachmentPointLogic>();
                            if (item1.handlers.Count > 0 && isHandle(item1))
                            {
                                if (apl.legalAttachments.Contains(this.partType) && this.GetComponent<AttachmentPointLogic>().legalAttachments.Contains(apl.partType))
                                {
                                    if (!apl.isAttached && !this.isAttached)
                                    {
                                        possibleAttachments.Add(apl);
                                    }
                                }
                            }

                        }
                    }
                }

            }


            if (possibleAttachments.Count > 0)
            {
                Transform close = GetClosestPoint(possibleAttachments);

                if (close != null)
                {
                    AttachmentPointLogic apl1 = close.GetComponent<AttachmentPointLogic>();
                    if (Vector3.Distance(apl1.transform.position, this.transform.position) < attachRange)
                    {
                        apl1.isAttached = true;
                        this.isAttached = true;

                        AttachObjects(this.GetComponent<AttachmentPointLogic>(), apl1, handle.item, interactor);
                    }

                }

            }
        }



        private void AttachObjects(AttachmentPointLogic point1, AttachmentPointLogic point2, Item ungrabbedItem, Interactor interactor)
        {

            //ungrabbed item definition
            ItemDefinition ungrabbedItemDefinition = point1.GetComponentInParent<ItemDefinition>();
            ItemDefinition point2ItemDefinition = point2.GetComponentInParent<ItemDefinition>();

            

            // Getting item position
            Transform point1ItemTransform = point1.GetComponentInParent<ItemDefinition>().transform;
            Transform point2ItemTransform = point2.GetComponentInParent<ItemDefinition>().transform;


            Item point2Item = point2ItemTransform.GetComponent<Item>();
            ItemData point2ItemData = point2Item.data;





            point1ItemTransform.MoveAlign(point1.transform, point2.transform);


            // -------------------------------------------------------------------- //
            //                                  Renderes and BladeMeshes            //
            // -------------------------------------------------------------------- //

            foreach (Renderer renderer in ungrabbedItemDefinition.renderers)
            {
                renderer.gameObject.transform.parent = point2ItemTransform;
                point2ItemDefinition.renderers.Add(renderer);
                if (point1.partType == "blade")
                {
                    bladeRenderes.Add(renderer);
                    Debug.Log("ADDED BLADE " + "Total " + bladeRenderes.Count);
                }
            }

            Debug.Log("Total blades " + bladeRenderes.Count);

            // -------------------------------------------------------------------- //
            //                                  Damagers                            //
            // -------------------------------------------------------------------- //

            foreach (ItemData.Damager damager1 in point2ItemData.damagers.ToList())
            {
                if (ungrabbedItem.data.damagers.Contains(damager1))
                {
                    point2ItemData.damagers.Remove(damager1);
                    Debug.Log("Removed damager");
                }
            }

            Debug.Log(point2ItemData.damagers.Count);

            point2ItemData.damagers.AddRange(ungrabbedItem.data.damagers);

            Debug.Log(point2ItemData.damagers.Count);

            foreach (DamagerDefinition damagerDefinition in ungrabbedItem.GetComponentsInChildren<DamagerDefinition>())
            {
                damagerDefinition.colliderGroup.transform.parent = point2ItemTransform;
                
                Debug.Log("ColliderGroup Name " + damagerDefinition.colliderGroup.name);
                damagerDefinition.transform.parent = point2ItemTransform;
                Debug.Log("damagerDefinition " + damagerDefinition.name);
                
            }

            point2ItemDefinition.colliderGroups.AddRange(ungrabbedItemDefinition.colliderGroups);

            point2Item.damagers.Clear();

            foreach (ItemData.Damager damager in point2ItemData.damagers)
            {
                foreach (DamagerDefinition damagerDefinition in point2.GetComponentInParent<ItemDefinition>().GetComponentsInChildren<DamagerDefinition>())
                {
                    if (damagerDefinition.name == damager.transformName)
                    {
                        Damager[] components2 = damagerDefinition.GetComponents<Damager>();
                        for (int j = 0; j < components2.Length; j++)
                        {
                            UnityEngine.Object.Destroy(components2[j]);
                        }
                        if (damagerDefinition.colliderGroup == null)
                        {
                            Debug.LogError("colliderGroup on DamagerDefinition is null");
                        }
                        else
                        {
                            Damager damager2 = damagerDefinition.gameObject.AddComponent<Damager>();
                            damager2.Load(Catalog.current.GetData<DamagerData>(damager.damagerID, true));
                            point2Item.damagers.Add(damager2);
                        }
                    }
                }
            }

            // -------------------------------------------------------------------- //
            //                                  CustomReference                     //
            // -------------------------------------------------------------------- //

            Debug.Log("Custom ref start");

            foreach (CustomReference customReference in ungrabbedItemDefinition.customReferences)
            {
                if (customReference.transform != null)
                {
                    customReference.transform.parent = point2ItemTransform;
                    point2ItemDefinition.customReferences.Add(customReference);
                } else
                {
                    Debug.LogError("Customreference transform" + customReference.name + " on item " + ungrabbedItemDefinition.itemId + " is not set?");
                }

            }


            // -------------------------------------------------------------------- //
            //                                  Reset Mass and stuff                //
            // -------------------------------------------------------------------- //



            point2Item.rb.ResetCenterOfMass();

            Vector3 CoM = point2Item.rb.centerOfMass;

            this.itemCoM = CoM;

            point2Item.rb.mass = (point2Item.rb.mass + ungrabbedItem.rb.mass);

            this.itemMass = point2Item.rb.mass;

            point2Item.rb.angularDrag = 0f;

            this.itemAngularDrag = point2Item.rb.angularDrag;

            point2Item.rb.drag = 0f;

            this.itemDrag = point2Item.rb.drag;

            point2Item.OnGrabEvent += this.fixItemInfo;

            // -------------------------------------------------------------------- //
            //                                  Whooshs                             //
            // -------------------------------------------------------------------- //


            foreach (ItemData.Whoosh whoosh in point2ItemData.whooshs.ToList())
            {
                if (ungrabbedItem.data.whooshs.Contains(whoosh))
                {
                    point2ItemData.whooshs.Remove(whoosh);
                }                    
            }


            foreach (WhooshPoint whooshPoint in ungrabbedItemDefinition.whooshPoints)
            {
                whooshPoint.transform.parent = point2ItemTransform;
            }

            point2ItemDefinition.whooshPoints.AddRange(ungrabbedItemDefinition.whooshPoints);
            point2ItemData.whooshs.AddRange(ungrabbedItem.data.whooshs);


            foreach (ItemData.Whoosh whoosh in point2ItemData.whooshs.ToList())
            {
                foreach (WhooshPoint whooshPoint in point2ItemDefinition.whooshPoints)
                {
                    if (whooshPoint.name == whoosh.transformName)
                    {
                        FXData data = Catalog.current.GetData<FXData>(whoosh.fxId, true);
                        if (data != null)
                        {
                            Whoosh whoosh2 = whooshPoint.gameObject.GetComponent<Whoosh>();
                            if (!whoosh2)
                            {
                                whoosh2 = whooshPoint.gameObject.AddComponent<Whoosh>();
                            }
                            whoosh2.Load(data, whoosh.trigger, whoosh.minVelocity, whoosh.maxVelocity);
                        }
                    }
                }
            }

            // -------------------------------------------------------------------- //
            //                                  Modules WIP                         //
            // -------------------------------------------------------------------- //

            foreach (ItemModule itemModule in ungrabbedItem.data.modules)
            {
                Debug.Log(itemModule.type.Name);
                if (itemModule.type.Name != "ItemModuleAI" && itemModule.type.Name != "ItemModulePart")
                {

                    addedModules.Add(itemModule);

                }
            }

            Debug.Log("Total modules added " + addedModules.Count);

            // Get all modules on the new item and find all added modules
            List<ItemModule> totalAddedModules = new List<ItemModule>();

            foreach (CustomReference customReference1 in point2ItemDefinition.customReferences)
            {
                if (customReference1.transform.GetComponent<AttachmentPointLogic>())
                {
                    AttachmentPointLogic apl = customReference1.transform.GetComponent<AttachmentPointLogic>();

                    if (apl.addedModules.Count > 0)
                    {
                        totalAddedModules.AddRange(apl.addedModules);
                    }

                }
            }

            Debug.Log("Total added modules to item " + totalAddedModules.Count);


            foreach (ItemModule itemModule1 in totalAddedModules)
            {
                point2ItemData.modules.Add(itemModule1);

                itemModule1.OnItemLoaded(point2Item);

                point2ItemData.modules.Remove(itemModule1);
            }



            // -------------------------------------------------------------------- //
            //                                  Despawn                             //
            // -------------------------------------------------------------------- //

            ungrabbedItem.ResetObjectCollision();
            ungrabbedItem.SetColliderAndMeshLayer(VRManager.GetLayer(LayerName.MovingObject));

            ungrabbedItem.Despawn();

            point2Item.RefreshCollision();

            item = point2Item;

        }





        Transform GetClosestPoint(List<AttachmentPointLogic> nearBy)
        {
            Transform tMin = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;
            foreach (AttachmentPointLogic t in nearBy)
            {
                if (t != null)
                {
                    float dist = Vector3.Distance(t.transform.position, currentPos);
                    if (dist < minDist)
                    {
                        tMin = t.transform;
                        minDist = dist;
                    }
                }
            }

            if (tMin == null)
            {
                return null;
            }

            return tMin;
        }


        public void fixItemInfo(Handle handle, Interactor interactor)
        {
            Rigidbody rb = handle.item.rb;


            if (this.itemMass != null && this.itemAngularDrag != null && this.itemDrag != null && this.itemCoM != null) {
                rb.mass = this.itemMass;
                rb.angularDrag = this.itemAngularDrag;
                rb.drag = this.itemDrag;
                rb.centerOfMass = this.itemCoM;
            } else
            {
                Debug.LogError("ItemData not set in pointlogic?");
                return;
            }


        }
    }
}
