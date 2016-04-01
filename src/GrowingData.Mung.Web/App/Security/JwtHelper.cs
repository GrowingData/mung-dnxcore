using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using JWT;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using GrowingData.Utilities.DnxCore;
//using Microsoft.IdentityModel.Tokens.Jwt;

namespace GrowingData.Mung.Web {
	public class JwtHelper {
		public static NewtonsoftJsonSerializer Serializer = new NewtonsoftJsonSerializer();

		public class NewtonsoftJsonSerializer : IJsonSerializer {

			public string Serialize(object obj) {
				// Implement using favorite JSON Serializer
				return JsonConvert.SerializeObject(obj);
			}

			public T Deserialize<T>(string json) {
				// Implement using favorite JSON Serializer
				return JsonConvert.DeserializeObject<T>(json);
			}
		}

		//public static void CreateCertificate(string outPath) {

		//	using (var rng = RandomNumberGenerator.Create()) {
		//		//RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();
		//		byte[] keyForHmacSha256 = new byte[64];
		//		rng.GetBytes(keyForHmacSha256);

		//		///////////////////////////////////////////////////////////////////
		//		// Create signing credentials for the signed JWT.
		//		// This object is used to cryptographically sign the JWT by the issuer.
		//		SigningCredentials sc = new SigningCredentials(
		//										new InMemorySymmetricSecurityKey(keyForHmacSha256),
		//										"http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
		//										"http://www.w3.org/2001/04/xmlenc#sha256");

		//	}
		//}


		//public static RSAParameters GenerateRSAParameters() {

		//	var rsa = new RSACryptoServiceProvider(2048);

		//	RSACryptoServiceProvider myRSA = new RSACryptoServiceProvider(2048);

		//	RSAParameters publicKey = myRSA.ExportParameters(false);
		//	RSAParameters privateKey = myRSA.ExportParameters(true);


		//	return privateKey;
		//}

		//public static SymmetricSecurityKey KeyFromSecret(string secret) {


		//	var bytes = Convert.FromBase64String(secret);
		//	var hmac = new HMACSHA256(bytes);

		//	return new SymmetricSecurityKey(bytes);

		public static void InitializeJsonSerialization() {
			JsonWebToken.JsonSerializer = new CustomJsonSerializer();
		}

		public static string GenerateSecret() {

			using (var rng = RandomNumberGenerator.Create()) {
				byte[] keyForHmacSha256 = new byte[32];
				rng.GetBytes(keyForHmacSha256);
				return Convert.ToBase64String(keyForHmacSha256);
			}
		}

		public class CustomJsonSerializer : IJsonSerializer {
			public string Serialize(object obj) {
				// Implement using favorite JSON Serializer
				return JsonConvert.SerializeObject(obj);
			}

			public T Deserialize<T>(string json) {
				// Implement using favorite JSON Serializer
				return JsonConvert.DeserializeObject<T>(json);
			}
		}
	}
}