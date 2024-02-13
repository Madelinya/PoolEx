


namespace PoolEx.Pools
{
    using Interfaces;
    using Delegates;
    using SettingStructs;
    using System;
    using UnityEngine;
    
    public class StackPool<POOLED_TYPE> : BasePool<POOLED_TYPE>, IStackPoolManager<POOLED_TYPE> where POOLED_TYPE : class?
    {
        protected int capacity = 1;
        protected int maxCapacity = 1;
        protected int minActive = 1;
        protected int current = 0;

        public override int MaxCapacity => maxCapacity;
        public override int RemainingCapacity => capacity - current - 1; 
        public int Current => current;
        public override int Capacity => capacity;
        public int MinActive => minActive;
        public bool CanGoNext => current < capacity;
        public bool CanGoPrev => current >= minActive;
        public override ref POOLED_TYPE this[int i] => ref pool[i];
        public override ref POOLED_TYPE[] Pool => ref pool;
        
        public ref POOLED_TYPE GetNext
        {
            get
            {
                if (current < capacity)
                {
                    return ref pool[current + 1];
                }
                throw new Exception("out of high range");
            }
        }
        
        public ref POOLED_TYPE  GetPrevious
        {
            get
            {
                if (current > 0)
                {
                    return ref pool[current - 1];
                }
                throw new Exception("out of low range");
            }
        }
        
        public ref POOLED_TYPE  GetCurrent
        {
            get
            {
                if (current <= capacity && current >= 0)
                {
                    return ref pool[current];
                }
                throw new Exception("out of range");
            }
        }

        public ref POOLED_TYPE GetEnd => ref pool[0];
        
        protected POOLED_TYPE[] pool;

        public StackPool(StackPoolSettingStruct<POOLED_TYPE> settings)
        {
            this.Create = settings.Create;
            this.Destroy = settings.Destroy ;
            this.OnReturn = settings.OnReturn;
            this.OnBorrow = settings.OnBorrow;
            this.IsInstanceValid = settings.IsInstanceValid;

            maxCapacity = settings.maxCap;
            capacity = settings.cap;
            minActive = settings.minAct;
            current = settings.startAt;
            
            if (maxCapacity < 1)
            {
                throw new Exception("Only non 0 positive max capacity allowed");
            }

            if (minActive < 1)
            {
                throw new Exception("Onlu non 0 positive min active allowed");
            }

            if (settings.preFill)
            {
                MakeFullPool(maxCapacity); 
            }
            else
            {
                MakePool(maxCapacity);
            }
            
            Validate();
        }
        public StackPool(
            CreateDelegate<POOLED_TYPE> Create,
            DestroyDelegate<POOLED_TYPE> Destroy,
            OnBorrowDelegate<POOLED_TYPE> OnBorrow,
            OnReturnDelegate<POOLED_TYPE> OnReturn,
            IsInstanceValidDelegate<POOLED_TYPE> IsInstanceValid, 
            int? maxCap = null, int? cap = null, int? minAct = null, int? startAt = null, bool preFill = true)
        {
            this.Create = Create;
            this.Destroy = Destroy ;
            this.OnReturn = OnReturn;
            this.OnBorrow = OnBorrow;
            this.IsInstanceValid = IsInstanceValid;
            
            maxCapacity = maxCap ?? maxCapacity;
            capacity = cap ?? capacity;
            minActive = minAct ?? minActive;
            current = startAt ?? current;
            
            if (maxCapacity < 1)
            {
                throw new Exception("Only non 0 positive max capacity allowed");
            }

            if (minActive < 1)
            {
                throw new Exception("Onlu non 0 positive min active allowed");
            }

            if (preFill)
            {
                MakeFullPool(maxCapacity); 
            }
            else
            {
                MakePool(maxCapacity);
            }
            Validate();
            
        }
 
        private void Validate()
        {
            ChangeCapacity(capacity);
            SetActive(current + 1);
            ChangeCapacity(capacity);
        }
        
        //Borrowing methods
        public override int? Borrow(int length = 1)
        {
            if (IsInstanceValid(ref GetNext))
            {
                OnBorrow(ref GetNext);
            }
            else
            { //Just a safety. Should not happen much, but allows for quiet repair, or can be intended in case prefill was disabled
                RemakeElement(ref GetNext, current + 1);
            }
            
            current++;
            return null;
        }

        public override void Return(int id = 0)
        {
            if (IsInstanceValid(ref GetCurrent))
            {
                OnReturn(ref GetCurrent);
            }
            else
            { //Just a safety. Should not happen much, but allows for quiet repair, or can be intended in case prefill was disabled
                RemakeElement(ref GetCurrent, current);
            }
            current--;
        }

        public virtual void Next()
        {
            if (!CanGoNext)
            {
                return;
            }
            Borrow();
        }
        
        public virtual void Previous()
        {
            if (!CanGoPrev)
            {
                return;
            }
            Return();
        }
        public virtual void SetActive(int i)
        {
            if (i < 0)
            {
                throw new Exception("Only positive active number allowed");
            }

            if (i > capacity)
            {
                throw new Exception("attempt to set active number out of range");
            }

            current = i <= minActive ? minActive - 1 : i - 1;

            if (pool.Length < 1)
            {
                return;
            }
            
            Activate(i);
        }
        protected virtual void Activate(int i)
        {
            if (i < 0)
            {
                throw new Exception("Only positive active number allowed");
            }
            
            if (i > capacity)
            {
                throw new Exception("attempt to set active number out of range");
            }
            
            for (int j = 0; j < pool.Length; j++)
            {
                if(!IsInstanceValid(ref pool[j]) && j <= current)
                { //Actively checks tries to remake only neccesarily active elements
                    RemakeElement(ref pool[j], current + 1);
                }
                
                if (IsInstanceValid(ref pool[j]))
                {  //Wrapped in if just to be safe
                    if (j < minActive || j <= current)
                    {
                    
                        OnBorrow(ref pool[j]);
                        continue;
                    }
                    OnReturn(ref pool[j]);
                }
            } 
        }
        
        //Element creation and destruction
        public override void DestroyElement(ref POOLED_TYPE el)
        {
            
            if (el == null)
            {
                return;
            }
            Destroy(ref el);
            el = null;
        }
        public override POOLED_TYPE MakeElement(int i)
        {
            return Create(i);
        }

        public override void RemakeElement(ref POOLED_TYPE el, int i)
        {
            DestroyElement(ref el);
            el = MakeElement(i);
        }

        //Pool filling and draining
        public override void MakeFullPool(int length)
        {
            if (pool != null)
            {
                DrainPool();
            }
            MakePool(length);
            FillPool();
        }

        public override void FillPool()
        {
            for (int i = 0; i < maxCapacity; i++)
            {
                pool[i] ??= MakeElement(i);
            }
        }

        public override void DrainPool()
        {
            for (int i = 0; i < maxCapacity; i++)
            {
                DestroyElement(ref pool[i]);
            }
        }

        //Poolsize managemenet and creation
        public override void MakePool(int length)
        {
            if (pool == null)
            {
                pool = new POOLED_TYPE[length];
            }
            else
            {
                ResizeBuffer(length);
            }
        }

        public override void ChangeCapacity(int i)
        {
            if (!(maxCapacity > i))
            {
                throw new Exception("Trying to set capacity of a pool over maximum allowed capacity! \n " +
                                    "Consider resizing the buffer first");
            }

            capacity = i;
        }
        public override void ResizeBuffer(int i)
        {
            if (i == maxCapacity)
            {
                return;
            }
            
            if (i < 1)
            {
                throw new Exception("Only non zero positive lengths are allowed");
            }

            if (capacity > i)
            {
                capacity = i;
            }

            if (current > i)
            {
                current = i; 
            }
            
            if (i < maxCapacity)
            {
                for (int j = i - 1; j < maxCapacity; j++)
                {
                    Destroy(ref pool[j]);
                }
            }
            
            Array.Resize(ref pool, i);
            FillPool();
        }

    }

}
