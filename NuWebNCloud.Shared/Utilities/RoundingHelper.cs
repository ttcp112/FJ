using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Utilities
{
    public class RoundingHelper
    {
        //A. Option 1: (for phase 1) - Apply for both customer or merchant/store when makes payment.
        //1. Decimal value 0, 1, 2 -> after rounding become 0. Ex 100.01 ->100.00
        //2. Decimal value 3, 4, 5, 6, 7 -> after rounding become 5. Ex 100.04 ->100.05
        //3. Decimal value 8, 9, 10 -> after rounding become 10. Ex. 100.08 -> 100.10

        //B. Option 2 (for phase 2)
        //B1. When Customer pays the bill
        //1. Decimal value < 5 -> after rounding become 0. Ex. 100.04 -> 100.00
        //2. Decimal value >= 5 -> after rounding become 5. Ex. 100.06 -> 100.05; 100.09 -> 100.05

        //B2. When Merchant/store pays
        //1. Decimal value <= 5 -> after rounding become 5. Ex. 100.04 -> 100.05
        //2. Decimal value > 5 -> after rounding become 10. Ex. 100.06 -> 100.10

        //For both the above option, the rounding should be applied in two steps:
        //1. Round the value to 2 decimal numbers with rule > 5 -> round up; < 5 round down. Ex 100.137 -> 100.14; 100.134 -> 100.13. 
        //2. Apply Option1 or Option2 base on the system setting.

        public enum RoundingOptionEnum
        {
            Option1,
            Option2
        }

        public static decimal Rounding(decimal input, RoundingOptionEnum option)
        {
            decimal result = 0;
            switch (option)
            {
                case RoundingOptionEnum.Option1:
                    result = RoundOption1(input);
                    break;

                case RoundingOptionEnum.Option2:
                    //result = RoundOption1(input);
                    break;
            }

            return result;
        }

        //1. Round the value to 2 decimal numbers with rule > 5 -> round up; < 5 round down. Ex 100.137 -> 100.14; 100.134 -> 100.13. 
        public static decimal RoundUp2Decimal(decimal input)
        {
            input = Math.Round(input + (decimal)0.0005, 2);
            return input;
        }

        private static decimal RoundOption1(decimal input)
        {
            //round up Ex 100.137 -> 100.14; 100.134 -> 100.13.
            decimal result = RoundUp2Decimal(input);
            string[] temp = result.ToString().Split('.');
            if (temp.Length == 2)
            {
                decimal integerPart = decimal.Parse(temp[0]);
                string decimalPart = temp[1];
                if (decimalPart.Length == 2)//only handle if there is 2 decimal numbers
                {
                    string temp1 = decimalPart.Substring(0, 1);
                    string temp2 = decimalPart.Substring(1, 1);
                    switch (temp2)
                    {
                        case "0":
                        case "1":
                        case "2":
                            temp2 = "0";
                            break;
                        case "3":
                        case "4":
                        case "5":
                        case "6":
                        case "7":
                            temp2 = "5";
                            break;
                        case "8":
                        case "9":
                            temp2 = "10";
                            break;
                    }

                    if (temp2 == "10")
                    {
                        temp1 = (int.Parse(temp1) + 1).ToString();
                        temp2 = "0";
                        if (temp1 == "10")
                        {
                            integerPart = integerPart + (decimal)1;
                            temp1 = "0";
                        }
                    }
                    result = decimal.Parse(integerPart.ToString() + "." + temp1 + temp2);

                    //decimal decimalPartNew = (decimal)(temp1 * 10 + temp2);
                    //if (decimalPartNew >= 100)
                    //{
                    //    //round up to dollar. 0.98 ---> 1.00
                    //    integerPart = integerPart + (decimal)1;
                    //    result = decimal.Parse(integerPart.ToString());
                    //}
                    //else
                    //{
                    //    if (decimalPartNew < 10)
                    //    {
                    //        //0.07 --> 0.0
                    //        integerPart = integerPart + (decimal)1;
                    //        result = decimal.Parse(integerPart.ToString());
                    //    }
                    //    else
                    //    {
                    //        result = decimal.Parse(integerPart.ToString() + "." + decimalPartNew);
                    //    }
                    //}
                }
            }
            return result;
        }

        public static double RoundUpTo2Decimal(double input)
        {
            decimal result = RoundUp2Decimal((decimal)input);
            return (double)result;
        }
    }
}
