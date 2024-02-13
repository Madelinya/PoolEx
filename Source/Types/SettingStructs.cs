using System;

namespace PoolEx.SettingStructs
{
    using Delegates;
    
    [Serializable]
    public struct StackPoolSettingStruct<POOLED>
    {
        public IsInstanceValidDelegate<POOLED> IsInstanceValid;
        public CreateDelegate<POOLED> Create;
        public DestroyDelegate<POOLED> Destroy;
        public OnBorrowDelegate<POOLED> OnBorrow;
        public OnReturnDelegate<POOLED> OnReturn;

        public int maxCap;
        public int cap;
        public int minAct;
        public int startAt;
        public bool preFill;


    }
}