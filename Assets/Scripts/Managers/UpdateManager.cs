using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace QFramWork
{
    public class UpdateManager : Singleton<UpdateManager>
    {
        public delegate void mDelegate();
        private mDelegate test;
        // Start is called before the first frame update
        void Start()
        {
            test += Test;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Test()
        {

        }
    }
}
