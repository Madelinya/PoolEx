namespace PoolEx.Interfaces
{
   public interface IBasePoolManager<POOLED_TYPE>
   {
       //Buffer
       public int Capacity { get; }
       public int MaxCapacity { get; }
       public int RemainingCapacity { get; }
       
       //Pool and such
       public ref POOLED_TYPE[] Pool { get; }
       public ref POOLED_TYPE this[int i] { get; }
       
       //Managing
       public void ChangeCapacity(int i);
       public void ResizeBuffer(int i);
       
       public int? Borrow(int length = 1);
       public void Return(int id = 0);
       
       public void DestroyElement(ref POOLED_TYPE el);
       public POOLED_TYPE MakeElement(int ind);
       
       public void FillPool();
       public void DrainPool();
       public void MakeFullPool(int length);
       
   }
   
   public interface IStackPoolManager<POOLED_TYPE> : IBasePoolManager<POOLED_TYPE>
   {
       public int MinActive { get; }
       public int Current { get; }
       public ref POOLED_TYPE GetEnd { get; }
       public ref POOLED_TYPE GetCurrent { get; }
       public ref POOLED_TYPE GetNext { get; }
       public ref POOLED_TYPE GetPrevious { get; }
       public bool CanGoNext { get; }
       public bool CanGoPrev { get; }
       
       public void SetActive(int i);
       public void Next();
       public void Previous();
   
   }
   /*
   public interface IComplexPoolManager<POOLED_TYPE> : IBasePoolManager<POOLED_TYPE>
   {
       //Manages Individual instances of an object. Does not neccesairily have to be a game object :)
       public new bool Return(int uid);
       
       
       public int? InstanceUId(int unid);
       public int? InstanceBorrowUId(int unid);
       public int? InstanceIndex(int unid);
   
       public int? InstanceBorrowIndex(int unid);
   
       public int RemainingCapacity { get; }
       
       public int BorrowGroupSize(int unid);
   } */
}


