using System;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace LibWars
{
		public class Client
		{
				private TcpClient m_client;
				private BinaryWriter writer;
				private BinaryReader reader;
				private Thread receiver;
				private bool running = true;
				private uint m_nextId = 0;
				//TODO: not static!
				private static Dictionary<uint, IFutureResult> _pending = new Dictionary<uint, IFutureResult> ();
				
				private static Client m_instance = new Client();
				public static Client Instance {
					get { return m_instance;}
				}
				
				public void connect (string hostname, int port)
				{
						m_client = new TcpClient (hostname, port);
						writer = new BinaryWriter (m_client.GetStream ());
						reader = new BinaryReader (m_client.GetStream ());
						receiver = new Thread(receiverLoop);
						receiver.Start();
						//Thread t = new Thread (new ThreadStart (d.receive));
						//t.Start ();
				}
				
				public interface IFutureResult {
						void handleResponse(string resp);
				};

				public class FutureResult<T, E > : IFutureResult
				{
						enum FutureState
						{
								WAITING,
								SUCCESS,
								ERROR
						}

						FutureState state = FutureState.WAITING;
						ManualResetEvent onDone = new ManualResetEvent (false);
						T result;
						private readonly object sync = new object ();
						RequestError<E, T> error;

						public delegate void SuccessHandler (T t);

						public delegate void ErrorHandler (RequestError<E,T> e);

						public event SuccessHandler onSuccess;
						public event ErrorHandler onError;

						public FutureResult ()
						{

						}
						
						public void handleResponse(string resp) {
						try {
							Response<T, E> r = JsonConvert.DeserializeObject<Response<T, E>>(resp);
								if (r.error != null) {
										SetError (r.error.getException ());
								} else {
										SetResult (r.result);
								}
						} catch (Exception e) {
							throw e;
						}
						}

						public void SetResult (T t)
						{
								lock (sync) {
										if (state != FutureState.WAITING) {
												throw new Exception ("invalid state");
										} else {
												result = t;
												state = FutureState.SUCCESS;
												onDone.Set ();
												try{
													onSuccess(t);
												}
												catch(NullReferenceException){}
										}
								}
						}

						public void SetError (RequestError<E, T> e)
						{
								lock (sync) {
										if (state != FutureState.WAITING) {
												throw new Exception ("invalid state");
										} else {
												error = e;
												state = FutureState.ERROR;
												onDone.Set ();
												try{
													onError(e);
												}
												catch(NullReferenceException){}
										}
								}
						}

						public T GetResult (int timeoutMs)
						{
								bool gotSignal = onDone.WaitOne (timeoutMs);
								if (gotSignal == false) {
										throw new TimeoutException ();
								}
								lock (sync) {
										if (state == FutureState.SUCCESS) {
												return result;
										} else {
												throw error;
										} 

								}
						}
				}

				public FutureResult<T, object> Call<T> (string method, object param)
				{
						Request req = new Request (method, m_nextId, param);
						string jsonReq = JsonConvert.SerializeObject (req);
						Netstrings.Write (writer, jsonReq);
			
						FutureResult<T, object> fr = new FutureResult<T, object> ();
						_pending[m_nextId] = fr;
						m_nextId++;

						return fr;


				}
				
				protected void receiverLoop() {
					while(running) {
						processResponse();
					}
					foreach(IFutureResult fr in _pending.Values) {
						//TODO: Cancel
					}
				}

				protected void processResponse()
				{
						string r = Netstrings.Read (m_client.GetStream ());
						try {
							BasicResponse br = JsonConvert.DeserializeObject<BasicResponse>(r);
							if(!br.id.HasValue) {
								//Debug.LogWarning("Got response without id!" +  r);
								return;
							}
							IFutureResult fr = _pending[br.id.Value];
							fr.handleResponse(r);
						} catch (JsonException e) {
								m_client.Close ();
								//Debug.LogError ("Invalid json received");
								throw;
						}
				}
		}
}

