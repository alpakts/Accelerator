using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Litium.Accelerator.Security
{
    /// <summary>
    /// Generate random passwords.
    /// </summary>
    public class RandomStringGenerator
    {
        //Removed invalid characters '1','l','I','0','O','`','´','=','\'','"','<', '>' , '^', '~'
        private static readonly char[] ValidCharacters = new[]
                                                             {
                                                                 '(', ')',  '!', '@', '#', '$', '%',  '&', '*', '-', '+', '\\', '{', '}', '[', ']', ':', ';', ',', '.', '?', '/','_',
                                                                 '2', '3', '4', '5', '6', '7', '8', '9',
                                                                 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                                                                 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
                                                             };

        // removed 0 1
        private static readonly char[] ValidCharactersDigits = new[]
                                                                   {
                                                                       '2', '3', '4', '5', '6', '7', '8', '9'
                                                                   };

        //removed l
        private static readonly char[] ValidCharactersLower = new[]
                                                                  {
                                                                      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
                                                                  };

        //removed ' ` ´ " = < > ^ ~
        private static readonly char[] ValidCharactersSymbols = new[]
                                                                    {
                                                                        '(', ')', '!', '@', '#', '$', '%', '&', '*', '-', '+', '\\', '{', '}', '[', ']', ':', ';', ',', '.', '?', '/','_'
                                                                    };

        // removed I O 
        private static readonly char[] ValidCharactersUpper = new[]
                                                                  {
                                                                      'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
                                                                  };

        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <returns></returns>
        public static string Generate()
        {
            return Generate(6, 10);
        }

        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <param name="length">Password length.</param>
        /// <returns></returns>
        public static string Generate(int length)
        {
            return Generate(length, length);
        }

        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <param name="minLength">Minimum number of characters in password.</param>
        /// <param name="maxLength">Maximum number of characters in password.</param>
        /// <returns></returns>
        public static string Generate(int minLength, int maxLength)
        {
            if (minLength < 1)
                throw new ArgumentException("Password length rule, minimum 1 characters.", "minLength");

            if (maxLength < minLength)
                throw new ArgumentException("Password length rule, maxium length can not be short then minimum.", "maxLength");

            // Create a new instance of the RNGCryptoServiceProvider.
            using var randomNumberGenerator = RandomNumberGenerator.Create();

            //Byte array to store randomNumbers
            byte[] randomNumbers;

            //Generate password length.
            int passwordDelta = maxLength - minLength;
            int passwordLength;
            if (passwordDelta > 0)
            {
                randomNumbers = new byte[1];
                randomNumberGenerator.GetBytes(randomNumbers);
                passwordLength = minLength + (randomNumbers[0] % (passwordDelta + 1));
            }
            else
                passwordLength = maxLength;


            //Build password.
            StringBuilder password;
            randomNumbers = new byte[passwordLength];
            randomNumberGenerator.GetBytes(randomNumbers);
            password = new StringBuilder(passwordLength);

            for (int i = 0; i < passwordLength; i++)
            {
                int randomNumber = randomNumbers[i] % ValidCharacters.Length;
                char c = ValidCharacters[randomNumber];
                password.Append(c);
            }

            return password.ToString();
        }


        /// <summary>
        /// Generates a random password.
        /// </summary>
        /// <param name="lower">Number of lower characters in password.</param>
        /// <param name="upper">Number of upper characters in password.</param>
        /// <param name="symbols">Number of symbols in password.</param>
        /// <param name="digits">Number of digits in password.</param>
        /// <returns></returns>
        public static string Generate(int lower, int upper, int symbols, int digits)
        {
            if (lower < 0)
                throw new ArgumentException("Can not be negative", "lower");
            if (upper < 0)
                throw new ArgumentException("Can not be negative", "upper");
            if (symbols < 0)
                throw new ArgumentException("Can not be negative", "symbols");
            if (digits < 0)
                throw new ArgumentException("Can not be negative", "digits");


            int passwordLength = lower + upper + symbols + digits;
            if (passwordLength < 1)
                throw new ArgumentException("Password length rule, minimum 1 characters.");

            // Create a new instance of the RNGCryptoServiceProvider.
            using var randomNumberGenerator = RandomNumberGenerator.Create();

            CharacterType[] charType = new CharacterType[passwordLength];
            int i = 0, j = 0, k = 0, l = 0;
            for (; i < lower; i++)
            {
                charType[i] = CharacterType.Lower;
            }
            for (; j < upper; j++)
            {
                charType[i + j] = CharacterType.Upper;
            }
            for (; k < symbols; k++)
            {
                charType[i + j + k] = CharacterType.Symbol;
            }
            for (; l < digits; l++)
            {
                charType[i + j + k + l] = CharacterType.Digit;
            }


            byte[] charOrder = new byte[passwordLength];
            randomNumberGenerator.GetBytes(charOrder);
            SortedList sl = new SortedList(passwordLength);
            for (i = 0; i < charOrder.Length; i++)
            {
                if (!sl.ContainsKey((int)charOrder[i]))
                    sl.Add((int)charOrder[i], i);
                else
                    sl.Add(int.MaxValue - i, i);
            }


            //Build password.
            StringBuilder password;
            byte[] randomNumbers = new byte[passwordLength];
            randomNumberGenerator.GetBytes(randomNumbers);
            password = new StringBuilder(passwordLength);

            for (i = 0; i < passwordLength; i++)
            {
                int characterType = (int)sl.GetByIndex(i);

                switch (charType[characterType])
                {
                    case CharacterType.Lower:
                        password.Append(ValidCharactersLower[randomNumbers[i] % ValidCharactersLower.Length]);
                        break;
                    case CharacterType.Upper:
                        password.Append(ValidCharactersUpper[randomNumbers[i] % ValidCharactersUpper.Length]);
                        break;
                    case CharacterType.Symbol:
                        password.Append(ValidCharactersSymbols[randomNumbers[i] % ValidCharactersSymbols.Length]);
                        break;
                    case CharacterType.Digit:
                        password.Append(ValidCharactersDigits[randomNumbers[i] % ValidCharactersDigits.Length]);
                        break;
                }
            }

            return password.ToString();
        }

        #region Nested type: CharacterType

        private enum CharacterType
        {
            Lower = 0,
            Upper = 1,
            Symbol = 2,
            Digit = 3
        }

        #endregion
    }
}
