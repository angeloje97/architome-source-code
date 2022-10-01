using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace Architome
{
    public class ArchitomeSurrogate : ISerializationSurrogate
    {
        public static DataMap.Maps dataMaps;

        public static DataMap.Maps DataMaps
        {
            get
            {
                dataMaps ??= DataMap.active._maps;
                return dataMaps;
            }
        }

        public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
        }

        public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return obj;
        }
    }
}
