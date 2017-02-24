using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralRoguelike
{

    /* To use this class in Unity, extend this class like so:
     * [Serializable] public class SerializableDictionaryOf_string_int
     *  : SerializableDictionary<string, int> { }
     *  
     *  Then make sure to call Deserialize on instances of the child class in Awake().
     */
    [Serializable]
    public class SerializableDictionary<TK, TV>
    {
        public List<TK> keys;
        public List<TV> values;

        /// <summary>
        /// Creates a Dictionary based on the contained keys and values.
        /// </summary>
        /// <returns>A Dictionary based on the contained keys and values.</returns>
        public Dictionary<TK, TV> DeSerialize()
        {
            var dict = new Dictionary<TK, TV>();

            for (int i = 0; i < keys.Count; ++i)
            {
                dict.Add(keys[i], values[i]);
            }

            //this = null;

            return dict;
        }
    }
}
