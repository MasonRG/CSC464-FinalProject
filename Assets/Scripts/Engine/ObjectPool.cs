using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Engine
{
	[Serializable]
	public class ObjectPool<T> where T : new()
	{
		protected readonly HashSet<T> active = new HashSet<T>();
        protected readonly HashSet<T> inactive = new HashSet<T>();

        protected readonly Func<T> onCreate = null;
        protected readonly Action<T> onReturn = null;
        protected readonly Action<T> onGet = null;

		public int TotalItems { get { return active.Count + inactive.Count; } }
		public int ActiveItems { get { return active.Count; } }
		public int AvailableItems { get { return inactive.Count; } }
        public int MaxPooledItems { get; private set; }

		public ObjectPool(Func<T> onCreate = null, Action<T> onReturn = null, Action<T> onGet = null, int? maxPooledItems = null)
		{
			this.onCreate = onCreate;
			this.onReturn = onReturn;
			this.onGet = onGet;

            if (maxPooledItems.HasValue && maxPooledItems.Value >= 0)
            {
                MaxPooledItems = maxPooledItems.Value;
            }
            else
            {
                //We will set a max of 100 items for any pool that isnt specified. Unlimited just seems like too many and a potential hazard...
                MaxPooledItems = 100;
            }
		}
        
        
		public T GetNextObject()
		{
			T obj;
			if (inactive.Count == 0)
			{
				if (onCreate != null)
				{
					obj = onCreate();//The oncreate method should NOT return null ever - if it does we essentially ignore it
					if (EqualityComparer<T>.Default.Equals(obj, default(T)))
					{
						obj = new T(); //Don't allow null so we avoid infinite recursion
					}
				}
				else
				{
					obj = new T();
				}
			}
			else
			{
				obj = inactive.First();
				inactive.Remove(obj);
			}

			if (EqualityComparer<T>.Default.Equals(obj, default(T)))
			{
				Debug.LogWarning("Object in objectPool was null! Deleting object from the pool and fetching again...");
				return GetNextObject();
			}

			active.Add(obj);
			if (onGet != null)
			{
				onGet(obj);
			}

			return obj;
		}

		public void ReturnObject(T obj)
		{
            if (!active.Contains(obj))
            {
                Debug.LogWarning("Trying to return an object to the pool that didn't come out of the pool.");
                return;
            }

			active.Remove(obj);

			if (EqualityComparer<T>.Default.Equals(obj, default(T)))
			{
				Debug.LogWarning("Object being returned to objectPool is null! Clearing from pool...");
				return;
			}
            
			if (onReturn != null) onReturn(obj);

            if (TotalItems >= MaxPooledItems)
            {
                if (obj is Component)
                {
                    UnityEngine.Object.Destroy((obj as Component).gameObject);
                }
                //else let GC deal with it
            }
            else
            {
                inactive.Add(obj);
            }
        }
	}
}