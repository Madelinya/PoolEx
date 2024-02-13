using System;
using UnityEngine;

namespace PoolEx.Pools
{
    using Interfaces;
    using Delegates;

#nullable enable
    public abstract class BasePool<POOLED_TYPE> : IBasePoolManager<POOLED_TYPE> where POOLED_TYPE : class?
    {
        //Callbacks
        protected IsInstanceValidDelegate<POOLED_TYPE> IsInstanceValid;
        protected CreateDelegate<POOLED_TYPE> Create;
        protected DestroyDelegate<POOLED_TYPE> Destroy;
        protected OnBorrowDelegate<POOLED_TYPE> OnBorrow;
        protected OnReturnDelegate<POOLED_TYPE> OnReturn;

        //Access
        public abstract ref POOLED_TYPE[] Pool { get; }
        public abstract ref POOLED_TYPE this[int i] { get; }

        public abstract int RemainingCapacity { get; }
        
        
        //Buffer stuff
        public abstract int Capacity { get; }
        public abstract int MaxCapacity { get; }
        
        public abstract void ChangeCapacity(int i);
        public abstract void MakePool(int capacity);
        public abstract void ResizeBuffer(int i);
        
        public abstract int? Borrow(int length = 1);
        public abstract void Return(int id = 0);
        
        
        //pool filling and draining
        public abstract void DestroyElement(ref POOLED_TYPE el);
        public abstract POOLED_TYPE MakeElement(int ind);
        public abstract void MakeFullPool(int length);
        public abstract void DrainPool();
        public abstract void FillPool();
        public abstract void RemakeElement(ref POOLED_TYPE seg, int ind);
    }
}