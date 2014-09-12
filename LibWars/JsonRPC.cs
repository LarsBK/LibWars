using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace LibWars
{
	public class Request
	{
		public Request(string method, uint id, object param) {
			this.method = method;
			this.id = id;
			this.param = param;
		}
		public readonly string method;
		public readonly uint id;
		[JsonProperty("params")]
		public readonly object param;
		public string jsonrpc = "2.0";
	}

	public class BasicResponse
	{
		public uint? id;
		public string jsonrpc;
	}

	public class Response<T,E> : BasicResponse
	{
		public Response() {}
		public class Error
		{
			public int code;
			public string message;
			public E data;
			public RequestError<E, T> getException() {
				return new RequestError<E, T>(this);
			}
		}
		public T result;
		public Error error;
	}

	public class RequestError<T, O> : Exception
	{
		public readonly Response<O, T>.Error error;
		public RequestError(Response<O, T>.Error r) :
			base( String.Format ("RPC error: {0} - {1}", r.code, r.message))
		{
			error = r;
		}
	}

}

