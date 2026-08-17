using System;
using SerializeReferenceEditor;

namespace Demo
{
    [Serializable]
    public abstract class BaseTestData
    {
    }
    
    [Serializable, SRName("Old Test")]
    public class OldTestData : BaseTestData
    {
        public int Value;
    }
    
    [Serializable, SRName("Old New Test")]
    public class NewTestData : BaseTestData
    {
        public string Value;
    }
}
