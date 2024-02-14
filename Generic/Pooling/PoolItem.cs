using System.Collections;
using UnityEngine;

namespace Architome
{
    public class PoolItem : MonoActor
    {

        #region Pool Functions
        Pool source;

        public void HandlePool(Pool pool)
        {
            this.source = pool;
        }

        public void Return()
        {
            source.Return(this);
        }

        #endregion

    }
}