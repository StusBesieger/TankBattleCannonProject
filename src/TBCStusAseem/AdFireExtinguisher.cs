using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using Modding;
using Modding.Serialization;
using Modding.Modules;
using Modding.Blocks;
using skpCustomModule;
using Vector3 = UnityEngine.Vector3;

namespace TBCStusSpace
{
    [XmlRoot("TBCAddFireExtinguisherModule")]
    [Reloadable]
    public class TBCAddFireExtinguisherModule : BlockModule
    {
        [XmlElement("ExtinguishingKey")]
        [RequireToValidate]
        public MKeyReference ExtinguishingKey;

        [XmlElement("ExtinguishingNum")]
        [DefaultValue(0)]
        [Reloadable]
        public int ExtinguishingNum;

        [XmlArray("Sounds")]
        [RequireToValidate]
        [CanBeEmpty]
        [XmlArrayItem("AudioClip", typeof(ResourceReference))]
        public object[] Sounds;
    }
    public class TBCAddFireExtinguisherBehaviour : BlockModuleBehaviour<TBCAddFireExtinguisherModule>
    {
        public int Enum = 2;
        public bool firefightngOK = false;
        public MKey extinguishingkey;
        public AudioClip AudioClip;
        public AudioSource audioSource;
        public GameObject BlockObject;
        public GameObject SimulatingObject;
        public FireTag[] FireTags;

        public override void OnBlockPlaced()
        {
            base.OnBlockPlaced();

        }
        public override void OnSimulateStart()
        {
            base.OnSimulateStart();
            extinguishingkey = GetKey(Module.ExtinguishingKey);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            AudioClip = ModResource.GetAudioClip("FireEx");
            SimulatingObject = this.transform.parent.gameObject;
            StartCoroutine(GetParentObject());
        }
        IEnumerator GetParentObject()
        {
            yield return new WaitForFixedUpdate();
            while (SimulatingObject.name != "Simulation Machine")
            {
                SimulatingObject = SimulatingObject.transform.parent.gameObject;
            }

            FireTags = SimulatingObject.GetComponentsInChildren<FireTag>();
        }
        public override void SimulateFixedUpdateHost()
        {
            base.SimulateFixedUpdateHost();

        }
        public override void SimulateUpdateAlways()
        {
            base.SimulateUpdateAlways();
            if (extinguishingkey.IsPressed || extinguishingkey.EmulationPressed())
            {
                firefightngOK = true;
                if(Enum > 0)
                {
                    audioSource.PlayOneShot(AudioClip);
                }
            }
        }
        public override void SimulateUpdateClient()
        {
            base.SimulateUpdateClient();
        }
        public override void SimulateUpdateHost()
        {
            base.SimulateUpdateHost();
            if (firefightngOK)
            {
                if (Enum > 0)
                {
                    foreach (FireTag fireTag in FireTags)
                    {
                        fireTag.WaterHit();
                    }
                    Enum--;
                    
                }
                firefightngOK = false;
            }
        }
    }
}
