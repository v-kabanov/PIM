// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2013-05-30
// Comment		
// **********************************************************************************************/

using System;

namespace AuNoteLib.Util
{
    public static class StatisticalFunctions
    {
        public static double? StDev(double[] values)
        {
            return CalculateStDev(values, false);
        }



        public static double? StDevP(double[] values)
        {
            return CalculateStDev(values, true);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="entirePopulation"></param>
        /// <returns>
        /// null if <paramref name="values"/> contains less than 2 elements
        /// </returns>
        private static double? CalculateStDev(double[] values, bool entirePopulation)
        {

            int count = 0;
            double var = 0;
            double prec = 0;
            double dSum = 0;
            double sqrSum = 0;

            int adjustment = 1;

            if (entirePopulation)
                adjustment = 0;

            foreach (double val in values)
            {
                dSum += val;
                sqrSum += val * val;
                count += 1;
            }

            if (count > 1)
            {
                var = count * sqrSum - (dSum * dSum);
                prec = var / (dSum * dSum);

                // Double is only guaranteed for 15 digits. A difference

                // with a result less than 0.000000000000001 will be considered zero.

                if (prec < 1E-15 || var < 0)
                {
                    var = 0;
                }
                else
                {
                    var = var / (count * (count - adjustment));
                }

                return Math.Sqrt(var);
            }

            return null;
        }
    }
}
