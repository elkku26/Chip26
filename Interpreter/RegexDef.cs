using System;

using System. Text.RegularExpressions;

namespace RegexDefinitions
{
    public class RegexDefine
    {

        public static Regex A_addr = new Regex(@"A([0-9]|[A-F]){3}");

        public static Regex B_addr = new Regex(@"B([0-9]|[A-F]){3}");

        public static Regex C_addr = new Regex(@"C([0-9]|[A-F]){3}");

        public static Regex D_addr = new Regex(@"D([0-9]|[A-F]){3}");

        public static Regex E_skp = new Regex(@"E([0-9]|[A-F]{1})9E");

        public static Regex E_sknp = new Regex(@"E([0-9]|[A-F]{1})A1");

        public static Regex F_load_from_dt = new Regex(@"F([0-9]|[A-F])07");

        public static Regex F_load_key = new Regex(@"F([0-9]|[A-F]{1})0A");

        public static Regex F_load_to_dt = new Regex(@"F([0-9]|[A-F]{1})15");

        public static Regex F_load_to_st = new Regex(@"F([0-9]|[A-F]{1})18");

        public static Regex Add_i_vx = new Regex(@"F([0-9]|[A-F]{1})1E");

        public static Regex Load_f_vx = new Regex(@"F([0-9]|[A-F]{1})29");

        public static Regex Load_b_vx = new Regex(@"F([0-9]|[A-F]{1})33");

        public static Regex Load_i_vx = new Regex(@"F([0-9]|[A-F]{1})55");

        public static Regex Load_vx_i = new Regex(@"F([0-9]|[A-F]{1})65");



        public static Regex One_addr = new Regex(@"1([0-9]|[A-F]){3}");

        public static Regex Two_addr = new Regex(@"2([0-9]|[A-F]){3}");

        public static Regex Three = new Regex(@"3.{3}");

        public static Regex Four = new Regex(@"4.{3}");

        public static Regex Five = new Regex(@"5.{3}");

        public static Regex Six = new Regex(@"6.{3}");

        public static Regex Seven = new Regex(@"7.{3}");



        public static Regex Eight_load = new Regex(@"8..0");

        public static Regex Eight_or = new Regex(@"8..1");

        public static Regex Eight_and = new Regex(@"8..2");

        public static Regex Eight_xor = new Regex(@"8..3");

        public static Regex Eight_add = new Regex(@"8..4");

        public static Regex Eight_sub = new Regex(@"8..5");

        public static Regex Eight_shr = new Regex(@"8..6");

        public static Regex Eight_subn = new Regex(@"8..7");

        public static Regex Eight_shl = new Regex(@"8..E");

        public static Regex Nine = new Regex(@"9..0");
        
    }
}
