using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace TestWrapper
{
    public class Class1 : DynamicObject
    {
        public T As<T>() where T : class
        {
           
        }
    }
}
