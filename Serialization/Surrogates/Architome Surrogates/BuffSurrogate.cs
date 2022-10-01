using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Architome
{
    public class BuffSurrogate : ArchitomeSurrogate
    {

        public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var buff = (BuffInfo)obj;

            info.AddValue("buffID", buff._id);
        }

        public override object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            
            var buffId = (int)info.GetValue("buffID", typeof(int));

            var buffInfo = dataMaps.buffs[buffId];

            obj = buffInfo;

            return obj;
        }
    }
}
