using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace PersonNarrowShow
{
    public class PersonNarrowShow : MonoBehaviour
    {
        public Transform parent;

        public enum type
        {
        }

        private void Awake()
        {
            Debug.Log("Awake parent:{0}" + parent);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }
    }
}