using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BS;
using UnityEngine;

namespace partAssembler
{
    public class ItemModulePart : ItemModule
    {

        public List<attachmentPointsData> attachmentPoints = new List<attachmentPointsData>();

        public class attachmentPointsData
        {
            public string referenceName;
            public string partType;
            public string[] legalAttachments;
        }

        public override void OnItemLoaded(Item item)
        {

            base.OnItemLoaded(item);

            foreach (attachmentPointsData point in attachmentPoints)
            {
                //Debug.Log("Item Loaded" + point.referenceName);
                AttachmentPointLogic script = item.definition.GetCustomReference(point.referenceName).gameObject.AddComponent<AttachmentPointLogic>();
                script.item = item;
                script.partType = point.partType;
                script.legalAttachments = point.legalAttachments;

                script.Initialize();
            }
        }
    }


}
