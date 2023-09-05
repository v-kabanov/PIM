// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2014-09-08
// Comment		
// **********************************************************************************************/

namespace FulltextStorageLib.Util;

public class SingletonFactory<T> : IFactory<T>
{
    private readonly T _instance;

    public SingletonFactory(T instance)
    {
        _instance = instance;
    }

    public T Create()
    {
        return _instance;
    }
}