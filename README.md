# PoolEx
> My utility classes for object pooling in unity. cutom object pool implementation to suit my needs better.
> 
## Overview
This implementation supposes that there are two objects when it comes to object pooling. The `SOURCE` and the `POOLED`. The `SOURCE` contains all pre-requisites for creating the `POOLED` structures. The pool object itsel, that manages the pool in various ways, be it based on a stack or queue or anything else implements the interface ` IPoolManager<POOLED_TYPE, SOURCE_TYPE>` where `SOURCE_TYPE : IPoolable<POOLED_TYPE>` where `POOLED_TYPE:IPooled` or an interface that inherits from this one. It is done this way so that the implementations that concern each obejct are within the implementation of each object. It is also intended to potentially handle any custom obejct, not only `GameObject` objects

The `SOURCE` contain creation and re-initialization methods. 

The `POOLED` contains methods for querrying activity, de/activating instance, Getting Unique ID ,and destroying itself.

## SOURCE
The `SOURCE` Implements the interface `IPooled<T>` where `T:IPooled` which by default contains 4 methods.

- `T[] GetFullPool(int length)`
- `void FillExistingPool(ref T[] pool)`
- `void RefillExistingPool(ref T[] pool)`
- `void Remake(ref T inst)`

  All of these methods just pretain to actually making the `Array<T>` that the poooling manager can use and manage. THey are all meant to be implemented by the user on the `SOURCE` object to allow for very fine control.

  ### `T[] GetFullPool(int length)`
    It's intended purpose is to create and return the array itself. It instantiates all the instances for the pool according to the implementation within the `ScriptableObject` itself. This is up to the user to implement. It is done this way so that the user has complete control over how the objects are created. It is similar to passing the creation delegate to the object pooling solution that is built into Unity.
  
  ### `void FillExistingPool(ref T[] pool)`
    It's intended purpose it to fill-up or top up an existing pool whose buffer has been changed. It does not overwrite already existing instances so it is a safe method to use in order to fill a pool in a non-destructive way.
  
  ### `void RefillExistingPool(ref T[] pool)`
    It's intended purpose is to overwrite any existing data within the supplied buffer. It essentially meant to call `Remake(ref T inst)` on every member within the pool. It is also meant to first destroy the instance if it already exists.

  ### `void Remake(ref T inst)`
    It's intended purpose it to re-initialize the `POOLED` object that has been supplied as a refference. Once again it is done like so in order to allow the user to exert fine control over the creation process.

  ## POOLED
