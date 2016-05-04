// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;

namespace PimTest
{
    public interface INoteHeader
    {
        string Id { get; }
        DateTime CreateTime { get; }

        string Name { get; }
    }
}