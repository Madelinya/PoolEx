namespace PoolEx.Delegates
{
    public delegate bool IsInstanceValidDelegate<POOLED>(ref POOLED pooled);

    public delegate POOLED CreateDelegate<POOLED>(int i);
    public delegate void DestroyDelegate<POOLED>(ref POOLED pooled);

    public delegate void OnBorrowDelegate<POOLED>(ref POOLED pooled);
    public delegate void OnReturnDelegate<POOLED>(ref POOLED pooled);
}