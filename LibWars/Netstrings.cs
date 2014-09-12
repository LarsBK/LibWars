
using System.IO;
using System;
namespace LibWars
{
	public static class Netstrings
	{
		public static void Write(BinaryWriter stream, string s) {
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (s);
			byte[] numBytes = System.Text.Encoding.ASCII.GetBytes (""+bytes.Length);
			stream.Write (numBytes);
			//stream.Write ('1');
			stream.Write (':');
			//stream.Write (' ');
			stream.Write (bytes);
			stream.Write (',');
		}

		public static string Read(Stream input) {

			int size = 0;

			for(int i = 0; ; ++i) {
				if (i >= 16) {
					throw new InvalidDataException("netstring too large, giving up");
				}
				int b = input.ReadByte ();
				if (b == ':') {
					if (i == 0) {
						throw new InvalidDataException("malformed netstring");
					}
					break;
				}
				int n = b - '0';
				if (n < 0 || n > 9) {
					throw new InvalidDataException("malformed netstring");
				}
				size = size*10 + n;
			}


			int bytesRead = 0;
			byte[] buffer = new byte[size];
			while (bytesRead < size) {
				int ret = input.Read (buffer, bytesRead, size - bytesRead);
				//Debug.Log(String.Format("Read returned {0}, offset {1}, size{2}", ret, bytesRead, size));
				if (ret > 0) {
					bytesRead += ret;
				}
			}
			if (input.ReadByte() != ',') {
				throw new InvalidDataException ("malformed netstring");
			}
			string done = System.Text.Encoding.UTF8.GetString(buffer);
			return done;
		}
	}
}

