﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using System.Threading.Tasks;

namespace GrowingData.Utilities.DnxCore {
	public static class StringHashing {

		public static string HashStringSHA1(this string valueUTF8) {
			byte[] valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
           
			var sha1 = SHA1.Create();
			byte[] hashBytes = sha1.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");

		}
		public static string HashStringMD5(this string valueUTF8) {
			byte[] valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
            var md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");

		}

		public static string HashStringSHA256(this string valueUTF8) {
			byte[] valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
            var shar2 = SHA256.Create();
			byte[] hashBytes = shar2.ComputeHash(valueBytes);
			return BitConverter.ToString(hashBytes).Replace("-", "");

		}

		public static string CreateSalt() {
			//Generate a cryptographic random number.
			int size = 256;
            using (var rng = RandomNumberGenerator.Create()) {
                byte[] buff = new byte[size];
                rng.GetBytes(buff);

                // Return a Base64 string representation of the random number.
                return Convert.ToBase64String(buff);
            }
		}


		public static string HashStrings(string valueUTF8, string saltBase64) {
			byte[] valueBytes = Encoding.UTF8.GetBytes(valueUTF8);
			byte[] saltBytes = Convert.FromBase64String(saltBase64);

			byte[] hashBytes = HashBytes(valueBytes, saltBytes);

			return Convert.ToBase64String(hashBytes);
		}

		public static byte[] HashBytes(byte[] value, byte[] salt) {
			byte[] saltedValue = value.Concat(salt).ToArray();
			// Alternatively use CopyTo.
			//var saltedValue = new byte[value.Length + salt.Length];
			//value.CopyTo(saltedValue, 0);
			//salt.CopyTo(saltedValue, value.Length);

			return SHA256.Create().ComputeHash(saltedValue);
		}


	}
}
