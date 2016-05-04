// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;

namespace PimTest
{
    public interface INote : INoteHeader
    {
        DateTime LastUpdateTime { get; }

        string Text { get; }

        bool IsTransient { get; }
    }
}