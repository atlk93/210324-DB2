using System;

namespace MyClassLibrary
{
    public class myLib
    {
        public static int Count(char deli, string str)    // str 문자열의 deli 구분자의 개수 + 1
        {
            string[] Strs = str.Split(deli);
            int n = Strs.Length;
            return n - 1;
        }
        public static string GetToKen(int index, char deli, string str)
        {
            string[] Strs = str.Split(deli);
            string ret = Strs[index];
            return ret;
        }
    }
}
