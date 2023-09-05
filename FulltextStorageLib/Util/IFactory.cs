// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2014-09-05
// Comment		
// **********************************************************************************************/
namespace FulltextStorageLib.Util;

public interface IFactory<out T>
{
    T Create();
}