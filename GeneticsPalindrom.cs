using System;
using System.IO;
using System.Linq;

namespace GeneticsPalindrom
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Insert lenght of palindrom mask:");
            int Palindrom_Mask_length = 0;           
            if (!int.TryParse(Console.ReadLine(), out Palindrom_Mask_length) || Palindrom_Mask_length <= 0)
            {
                Console.WriteLine("Palindrom Mask length must be positive number");
                Console.ReadLine();
                Environment.Exit(0);
            }

            Console.WriteLine("Insert lenght of medior:");
            int Medior_length = 0;
            if (!int.TryParse(Console.ReadLine(), out Medior_length) || Medior_length <= 0)
            {
                Console.WriteLine("Medior length must be positive number");
                Console.ReadLine();
                Environment.Exit(0);
            }
            int[] i;
            var lala = i.Where(x => x > 0).Count();
            string GEN_Sequence = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\GenSequence.txt");

            if (GEN_Sequence.Length < 2*Palindrom_Mask_length + Medior_length)
            {
                Console.WriteLine("Palindrom mask and medior length exceeds gen sequence length");
                Console.ReadLine();
                Environment.Exit(0);
            }

            string response = Find_Gen_Palindrom(GEN_Sequence, Palindrom_Mask_length, Medior_length);

            Console.WriteLine(response);

            Console.ReadLine();
        }
        public static string Find_Gen_Palindrom(string GEN_Sequence, int Palindrom_Mask_length, int Medior_length)
        {
            string response = string.Empty;
            int whole_palindrom_pattern_length = 2 * Palindrom_Mask_length + Medior_length;

            for (int i = 0; i < GEN_Sequence.Length; i++)
            {
                if (GEN_Sequence.Length - i >= whole_palindrom_pattern_length)
                {
                    response += Find_Palindrom(GEN_Sequence.Substring(i, whole_palindrom_pattern_length), Palindrom_Mask_length, Medior_length, i);
                }
                
            }

            return string.IsNullOrEmpty(response) ? $"Gen sequence contains no palindroms for palindrom mask = {Palindrom_Mask_length} and for medior = {Medior_length}" : response;
        }
        public static string Find_Palindrom(string Exact_Sequence, int Palindrom_Mask_length, int Medior_length, int index)
        {
            string response = string.Empty;

            string first_part = Exact_Sequence.Substring(0, Palindrom_Mask_length);
            string medior = Exact_Sequence.Substring(Palindrom_Mask_length, Medior_length);
            string second_part = Exact_Sequence.Substring(Palindrom_Mask_length + Medior_length, Palindrom_Mask_length);

            for (int i = 0; i < Palindrom_Mask_length; i++)
            {
                if (IsComplement(first_part[i].ToString(), second_part[Palindrom_Mask_length - 1 - i].ToString()))
                {
                    if (i == Palindrom_Mask_length - 1)
                    {
                        string palindrom_mask_without_medior = first_part + second_part;
                        string complement_palindrom_mask_without_medior = TransformToComplement(palindrom_mask_without_medior);
                        if (IsPalindrom(palindrom_mask_without_medior, complement_palindrom_mask_without_medior))
                        {
                            response = $"palindrom found: {Exact_Sequence}, start index = {index}, end index = {index + Exact_Sequence.Length}\r\n";
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return response;
        }
        public static bool IsComplement(string a, string b)
        {
            if (a.Equals("g", StringComparison.InvariantCultureIgnoreCase) && b.Equals("c", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else if (a.Equals("c", StringComparison.InvariantCultureIgnoreCase) && b.Equals("g", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else if (a.Equals("a", StringComparison.InvariantCultureIgnoreCase) && b.Equals("t", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else if (a.Equals("t", StringComparison.InvariantCultureIgnoreCase) && b.Equals("a", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string TransformToComplement(string sequence)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(sequence);
            for (int i = 0; i < stringBuilder.Length; i++)
            {
                if (stringBuilder[i].ToString().Equals("g", StringComparison.InvariantCultureIgnoreCase))
                {
                    stringBuilder[i] = 'c';
                }
                else if (stringBuilder[i].ToString().Equals("c", StringComparison.InvariantCultureIgnoreCase))
                {
                    stringBuilder[i] = 'g';
                }
                else if (stringBuilder[i].ToString().Equals("a", StringComparison.InvariantCultureIgnoreCase))
                {
                    stringBuilder[i] = 't';
                }
                else if (stringBuilder[i].ToString().Equals("t", StringComparison.InvariantCultureIgnoreCase))
                {
                    stringBuilder[i] = 'a';
                }
            }
            return stringBuilder.ToString();
        }
        public static bool IsPalindrom(string sequence_1, string sequence_2)
        {
            var array = sequence_2.ToCharArray();
            Array.Reverse(array);
            string sequence_2_reversed = new string(array);
            if (sequence_1.Equals(sequence_2_reversed, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
