
using ArrayExpanded;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using PoolEx.Interfaces;




public interface IConnectable
{
    public void Connect( Rigidbody2D to);
}






public struct PooledData : IComparer<PooledData>
{
    public PooledData(int uid)
    {
        if (uid < 0)
        {
            throw new Exception("Only uid 0 and above are valid");
        }

        this.uid = uid;
        index = uid;
        borrowIndex = null;
        borrowUId = null;
    }
    private int index;//Used to store the index inside a pool 
    private int uid; //Unique ID of an object, NEVER changes
    private int? borrowUId; //  ID of the borrow instance
    private int? borrowIndex; //actual idnex in order in the pool itself


    public void SetIndex(int ind)
    {
        if (ind < 0)
        {
            throw new Exception("Only positive indicies allowed");
        }
        index = ind;
    }

    public void SetBorrowIndex(int ind)
    {
        if (ind < 0)
        {
            throw new Exception("Only positive indicies allowed");
        }

        borrowIndex = ind;
    }

    public void Borrow(int uid)
    {
        if (borrowUId.HasValue || uid < 0)
        {
            throw new Exception("Only positive indicies allowed and overwriting set borrow index is not allowed");
        }

        borrowUId = uid;
        
    }

    public void Return()
    {
        borrowIndex = null;
        borrowUId = null;
    }

    public int Index => index; // where in the entire pool order it is
    public int UId => uid; //Never changes even when re-ordered
    public int? BorrowUId => borrowUId;//stays the same as long as borrowed
    public int? BorrowIndex => borrowIndex; //where in order of all the borrowed
    
        
    //Comparer implementation
    
    public int Compare(PooledData x, PooledData y)
    {
        // Compare based on the 'Value' property
        if (!x.BorrowUId.HasValue && !y.BorrowUId.HasValue)
        {
            return x.UId.CompareTo(y.UId);
        }
        else if (!x.BorrowUId.HasValue)
        {
            return 1;
        }
        else if (!y.BorrowUId.HasValue)
        {
            return -1;
        }
        else if (((int)x.BorrowUId).CompareTo((int)y.BorrowUId) != 0)
        {
            return ((int)x.BorrowUId).CompareTo((int)y.BorrowUId);
        }
        return x.UId.CompareTo(y.UId);
    }
}

/*
public interface IPoolManager<POOLED_TYPE, SOURCE_TYPE> 
    where SOURCE_TYPE : IPoolable<POOLED_TYPE> 
    where POOLED_TYPE:IPooled
{
    //Manages Individual instances of an object. Does not neccesairily have to be a game object :)
    public void MakePool(int length);
    public void MakeFullPool(int length);

    public int Borrow(int lenngth = 1); //returns positive UID if there was success
    public bool Return(int uid);
    
    
    public int? InstanceUId(int unid);
    public int? InstanceBorrowUId(int unid);
    public int? InstanceIndex(int unid);

    public int? InstanceBorrowIndex(int unid);

    public int RemainingCapacity { get; }
    public int MaxCapacity { get; }

    public int BorrowGroupSize(int unid);
    
    public ref SOURCE_TYPE Source { get; }
    public ref POOLED_TYPE[] Pooled { get; }
    public ref POOLED_TYPE this[int i] { get; }


}

public interface IChainPoolManager<POOLED_TYPE, SOURCE_TYPE> : IPoolManager<POOLED_TYPE, SOURCE_TYPE>
    where SOURCE_TYPE : IPoolable<POOLED_TYPE>
    where POOLED_TYPE : IConnectable, IPooled
{
    public int ChainLength(int unid);
    public POOLED_TYPE[] GetChain(int unid);
    
    public new ref POOLED_TYPE[] this[int i] { get; }
    public ref POOLED_TYPE this[int i, int j] { get; }
}
    


//Add a method to return group range
//Add a method to return partial array with only the group

public class Pool<POOLED_TYPE, SOURCE_TYPE> : IPoolManager<POOLED_TYPE, SOURCE_TYPE>
        where POOLED_TYPE : IPooled
        where SOURCE_TYPE : IPoolable<POOLED_TYPE>
{ 
    public Pool(SOURCE_TYPE src)
    {
        source = src;
    }

    //Fields
    protected int maxCapacity = 0;
    private PooledData[] pooledData;
    public POOLED_TYPE[] pooled;
    protected SOURCE_TYPE source;

    
    protected int nextUid = 0;
    protected int borrowAmount = 0;
    protected int freeSpace = 0;

    //Properties
    public ref SOURCE_TYPE Source => ref source;
    public ref POOLED_TYPE[] Pooled => ref pooled;
    public int MaxCapacity => maxCapacity;

    public int RemainingCapacity => freeSpace;
    //Indexer
    public ref POOLED_TYPE this[int i] => ref pooled[i];
    
    
    //Helpers
    public void MakePool(int length)
    {
        if (length < 1)
        {
            throw new Exception("Only positive non 0 values are accepted!");
        }
        maxCapacity = length;
        pooled = new POOLED_TYPE[maxCapacity];
        pooledData = new PooledData[maxCapacity];
        for (int i = 0; i < maxCapacity; i++)
        {
            pooledData[i] = new PooledData(i);
        }

        freeSpace = maxCapacity;
    }
    
    public void MakeFullPool(int length)
    {
        if (length < 1)
        {
            throw new Exception("Only positive non 0 values are accepted!");
        }

        maxCapacity = length;
        pooled = source.GetFullPool(length);
        pooledData = new PooledData[maxCapacity];
        for (int i = 0; i < maxCapacity; i++)
        {
            pooledData[i] = new PooledData(i);
        }
        freeSpace = maxCapacity;

    }

    private void OrderPool()
    {
        
        Array.Sort(pooledData, pooled);
    }

    protected void FlushPool()
    {
        for (int i = 0; i < maxCapacity; i++)
        {
            pooledData[i].Return();
            pooled[i].Off();
        }
        OrderPool();
    }

    protected virtual void SetIndicies()
    {
        for (int i = 0; i < maxCapacity; i++)
        {
            pooledData[i].SetIndex(i);
        }
    }

    public virtual int Borrow(int length = 1)
    {
        var ind = Array.FindIndex(pooledData, (pld) => !pld.BorrowUId.HasValue);
        
        if (freeSpace < length)
        {
            return -1;
        }
        
        if (ind < 0)
        {
            return ind;
        }

        FindNextFreeBorrowUId();
        for (int i = ind; i < ind + length; i++)
        {
            pooledData[i].Borrow(nextUid);
            pooledData[i].SetBorrowIndex(borrowAmount);
        }
        
        freeSpace -= length;
        borrowAmount++;
        
        return nextUid;
    }

    public virtual bool Return(int unid)
    {
        
        if (!IsBorrowUId(unid))
        {
            return false;
        }
        
        var ind = Array.FindIndex(pooledData, (pld) => pld.BorrowUId == unid);
        
        freeSpace += BorrowGroupSize(unid);
        borrowAmount--;

        
        for (int i = 0; i < maxCapacity; i++)
        {
            if (pooledData[i].BorrowIndex < unid)
            {
                continue;
            }

            if (pooledData[i].BorrowIndex == unid)
            {
                pooledData[i].Return();
                pooled[i].Off();
            }
            if (pooledData[i].BorrowIndex > unid)
            {
                pooledData[i].SetBorrowIndex((int)pooledData[i].BorrowIndex - 1);
            }
        }
        OrderPool();
        SetIndicies();
        return true;
    }
    
    private void IncrementBorrowUId()
    {
        nextUid = (nextUid + 1) % maxCapacity;
    }

    private int FindNextFreeBorrowUId()
    {
        while (IsBorrowUId(nextUid))
        {
            IncrementBorrowUId();
        }

        return nextUid;
    }
    
    //validation

    
        //Checks for existence of things
    protected virtual bool IsBorrowIndex(int bIndex)
    {
        return Array.FindIndex(pooledData, (pld) => pld.BorrowIndex == bIndex) >= 0;
    }
    protected virtual bool IsBorrowUId(int bIndex)
    {
        return Array.FindIndex(pooledData, (pld) => pld.BorrowUId == bIndex) >= 0;
    }
    protected virtual bool IsUId(int bIndex)
    {
        return Array.FindIndex(pooledData, (pld) => pld.UId == bIndex) >= 0;
    }

    protected virtual bool IsIndex(int bIndex)
    {
        return Array.FindIndex(pooledData, (pld) => pld.Index == bIndex) >= 0;
    }
    
    
        //Instance to position in pool
    protected virtual bool IsInstanceInPool(int unid)
    {
        return InstanceIndex(unid) >= 0;
    }

    public virtual int? InstanceIndex(int unid)
    {
        return ArrayEx.FindRange(pooled, unid, equalsInstanceId)?.Start.Value ?? null;
    }

    public virtual int? InstanceBorrowUId(int unid)
    {
        return null; //pooledData[InstanceIndex(unid)].BorrowUId;
    }

    public int? InstanceUId(int unid)
    {
        return null; //pooledData[InstanceIndex(unid)].UId;
    }

    public int? InstanceBorrowIndex(int unid)
    {
        return null; //pooledData[InstanceIndex(unid)].BorrowIndex;
    }

        //Borrow group stuff
    public int BorrowGroupSize(int unid)
    {
        var range = BorrowGroupRange(unid);
        if (range == null)
        {
            return 0;
        }
        return ((Range)range).End.Value - ((Range)range).Start.Value;
    }

    private Range? BorrowGroupRange(int unid)
    {
        return ArrayEx.FindRange(pooledData, unid, equalsBorrowUId);
    }

    private readonly Func<PooledData, int, bool> equalsBorrowUId = (pld, unique) => pld.BorrowUId == unique;
    private readonly Func<PooledData, int, bool> equalsBorrowIndex = (pld, unique) => pld.BorrowIndex == unique;
    private readonly Func<PooledData, int, bool> equalsUId = (pld, unique) => pld.UId == unique;
    private readonly Func<POOLED_TYPE, int, bool> equalsInstanceId = (pld, unique) => pld.UId == unique;
    
    
    public void Stringify()
    {
        foreach (var v in pooledData)
        {
            Debug.LogWarning($"BUID - {v.BorrowUId}  BIND - {v.BorrowIndex}  IND - { v.Index} UID - {v.UId}");
        }
    }
}

*/

namespace ArrayExpanded
{
    
    public static class ArrayEx
    {
        public static Range? FindRange<T, T2>(T[] source, T2 condition, Func<T, T2, bool> rule)
        {
            var first = -1;
            var last = -1;
            for (int i = 0; i < source.Length; i++)
            {
                if (rule(source[i], condition))
                {
                    if (first < 0)
                    {
                        first = i;
                    }
                }
                else
                {
                    if (first >= 0)
                    {
                        last = i;
                        return first..last;
                    }
                }
            }

            if (first < 0)
            {
                return null;
            }

            return first..source.Length;
        }
    }
}



