# PoolEx
> Scratch everything that you knew about Object pooling or Unity's object pooling, for this implementation works in the exact same manner. As everything become crab, I am now become a Unity.
## Overview
As previously mentioned this object pooling works very similar to unity object pool system. Under the hood, each object that handles the pooling inherits from `BasePool` and implements an interface that inherits from `IBasePoolManager`. Therefore the absolute core object for this object pooling solution looks as such `BasePool<POOLED_TYPE> : IBasePoolManager<POOLED_TYPE> where POOLED_TYPE : class?`. The Pool itself manages the creation, deletion, validation, borrowing, and returning of items, and it does so via assignable delegate functions. This decision was made to allow all the declarations to be nicely colocated. Under the hood it uses a simple array to hold all the refferences.
### Dictionary
| Term          | Definition    | 
| :-------------: |:-------------:| 
| Buffer / Pool | The actual array being used to hold the refferences to objects. |
| Pooled | The actual instance within the pool buffer. |
| Borrow     | Taking an instance from the pool to be used. | 
| Return     | Returning an instance into the pool making it possible to be reused.|     |  
| Create | Making the actual managed object.      | 
| Destroy | Cleaning up and nulling the instance of the object, often for re-creation. |
| Validation | Can mean either checking if an instance is null or validating the field values. |

---
---

# Code
Overall it is rather simple. In overview this system consists of 3 parts, which are also separated by namespaces. The `Pools` which hold the actual implementation of management of the system. The interfaces of `I***PoolManager`. Those just hold the outward facing declarations of methods. These interfaces are paired with their coresponding objects. These two parts are both generics which means the user can supply the object type. The last part are the type definitions. Those hold definitions for delegates and also for settings objects, whic can be used to nicely pass all the necessary config into the pool.


## Types of pools
- (Abstract) Base Pool
- Stack pool
- ~~Complex pool~~
- ~~Dumb pool~~

## `BasePool<POOLED_TYPE>`
Base pool is abstract, therefore there is no actual functionality, but it defines a set of common functions with common intended functionality for all of the other pools. All of these are also declared in `IBasePoolManager`
### Fields
XXX
### Poperties
XXX
### Methods

#### `MakePool(int length)`
XXX
#### `MakeFullPool(int length)`
XXX
#### `FillPool()`
XXX
#### `DrainPool()`
XXX
#### `ChangeCapacity(int size)`
XXX
#### `ResizeBuffer(int length)`
XXX

