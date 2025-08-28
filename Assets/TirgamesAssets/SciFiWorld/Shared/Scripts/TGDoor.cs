using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGGameEngine
{

    public class TGDoor : MonoBehaviour
    {
        [System.Serializable]
        public class TGDoorItem
        {
            public Transform DoorTransform;
            public Vector3 MoveDistance = new Vector3(1, 0, 0);
            public float MoveSpeed = 1;
            [HideInInspector]
            public Vector3 originPos;
            [HideInInspector]
            public Vector3 sourcePos;
            [HideInInspector]
            public Vector3 destPos;
            [HideInInspector]
            public bool processing = false;
            [HideInInspector]
            public float processValue = 0;
        }
        public bool DoorTriggerMode = true;
        public List<TGDoorItem> DoorsList = new List<TGDoorItem>();
        // private
        bool state=false;
        bool newState=false;
        bool processing=false;
        int processCnt = 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (TGDoorItem item in DoorsList)
            {
                item.originPos = item.DoorTransform.localPosition;
                item.processing = false;
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (DoorTriggerMode)
                newState = true;    
        }

        private void OnTriggerExit(Collider other)
        {
            if (DoorTriggerMode)
                newState = false;
        }


        public void DoorOpen()
        {
            newState = true;
        }

        public void DoorClose()
        {
            newState = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (state!=newState && !processing)
            {
                state = newState;
                processing = true;
                processCnt = 0;
                foreach (TGDoorItem item in DoorsList)
                {
                    item.sourcePos = state ? item.originPos : item.originPos + item.MoveDistance;
                    item.destPos = state ? item.originPos + item.MoveDistance : item.originPos;
                    if (item.DoorTransform)
                        item.processing = true;
                    item.processValue = 0;
                    processCnt++; 
                }
            }
            // process open/close            
            foreach (TGDoorItem item in DoorsList)
            {
                if (item.processing)
                {
                    item.DoorTransform.localPosition = Vector3.Lerp(item.sourcePos, item.destPos, item.processValue);
                    item.processValue += item.MoveSpeed * Time.deltaTime;
                    if (item.processValue > 1)
                    {
                        item.DoorTransform.localPosition = item.destPos;
                        item.processing = false;
                        processCnt--;
                    }
                }
            }
            // all doors processed
            if (processCnt == 0)
            {
                processing = false;
            }
        }
    }
}
